using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages the in-game economy and handles the distribution of defensive modules (cards) to the player's hand.
/// </summary>
public class CardManager: MonoBehaviour
{
    [Header("Economy")]
    [SerializeField]
    private float _energy = 100f;
    [SerializeField] 
    private float _passiveEnergyPerSecond = 1f;

    private float _energyTimer = 0f;

    [Header("Dependencies")]
    [SerializeField]
    private GameContentFactory _contentFactory;

    [Header("Rules")]
    [SerializeField] 
    private LayerMask _obstacleLayers;

    private Queue<HeroCardBase> _rotationCycle = new Queue<HeroCardBase>();
    private HeroCardBase[] _currentHand;
    private Dictionary<HeroCardBase, int> _cardDrawCounts = new Dictionary<HeroCardBase, int>();
    private LevelBase _myLevelData;

    public HeroCardBase NextCard { get; private set; }
    public float CurrentEnergy => _energy;

    public event Action<int, HeroCardBase> OnCardUpdated;
    public event Action<float> OnEnergyChanged;
    public event Action<HeroCardBase> OnNextCardChanged;

    public void Start()
    {
        _myLevelData = LevelManager.Instance.CurrentLevel;
        
        if ( _myLevelData.DeckList == null )
        {
            return;
        }

        if ( _myLevelData.DeckList.Count == 0 )
        {
            return;
        }

        _currentHand = new HeroCardBase[_myLevelData.GetHandSize()];

        for ( int i = 0; i < _myLevelData.GetHandSize(); i++ )
        {
            _currentHand[i] = GetRandomCard();
            OnCardUpdated?.Invoke(i, _currentHand[i]);
        }

        NextCard = GetRandomCard();
        OnNextCardChanged?.Invoke(NextCard);
        OnEnergyChanged?.Invoke(_energy);
    }

    private HeroCardBase GetRandomCard()
    {
        List<HeroCardBase> availableCards = new List<HeroCardBase>(_myLevelData.DeckList);
        
        if ( _currentHand != null )
        {
            foreach ( var card in _currentHand )
            {
                if ( card != null && availableCards.Contains(card) )
                {
                    availableCards.Remove(card);
                }
            }
        }

        if ( NextCard != null && availableCards.Contains(NextCard) )
        {
            availableCards.Remove(NextCard);
        }

        if ( availableCards.Count == 0 )
        {
            Debug.LogWarning("Insufficient unique cards in the deck. A duplicate will be permitted.");
            return _myLevelData.DeckList[UnityEngine.Random.Range(0, _myLevelData.DeckList.Count)];
        }

        int minDraws = int.MaxValue;

        foreach ( var card in availableCards )
        {
            if ( !_cardDrawCounts.ContainsKey(card) )
            {
                _cardDrawCounts[card] = 0;
            }

            if ( _cardDrawCounts[card] < minDraws )
            {
                minDraws = _cardDrawCounts[card];
            }
        }

        List<HeroCardBase> leastDrawnCards = new List<HeroCardBase>();

        foreach ( var card in availableCards ) 
        { 
            if ( _cardDrawCounts[card] == minDraws )
            {
                leastDrawnCards.Add(card);
            }
        }

        int randomIndex = UnityEngine.Random.Range(0, leastDrawnCards.Count);
        HeroCardBase selectedCard = leastDrawnCards[randomIndex];
        _cardDrawCounts[selectedCard]++;

        return selectedCard;
    }

    public void DrawCard(int handIndex)
    {
        _currentHand[handIndex] = NextCard;
        OnCardUpdated?.Invoke(handIndex, NextCard);

        NextCard = GetRandomCard();
        OnNextCardChanged?.Invoke(NextCard);
    }

    /// <summary>
    /// Processes the deployment logic of a module from the player's hand.
    /// Validates resource availability, spatial clearance, and structural integrity of the network path.
    /// </summary>
    public bool PlayCard(int handIndex, Vector3 spawnPosition)
    {
        if ( handIndex < 0 || handIndex >= _currentHand.Length )
        {
            return false;
        }

        HeroCardBase cardToPlay = _currentHand[handIndex];
        
        if ( cardToPlay == null )
        {
            return false;
        }

        if ( _energy < cardToPlay.Cost )
        {
            Debug.Log("Insufficient energy reserves!");
            return false;
        }

        Tile targetTile = GameBoard.Instance.GetTileFromPosition(spawnPosition);
        
        if ( targetTile.Type == Tile.TileType.Tower )
        {
            targetTile.isTowerFree = false;
        }

        _energy -= cardToPlay.Cost;
        OnEnergyChanged?.Invoke(_energy);

        GameEntity spawnedObject = _contentFactory.GetProduct(cardToPlay.PrefabToSpawn);
        spawnedObject.transform.position = targetTile.WorldPosition;
        spawnedObject.gameObject.SetActive(true);

        foreach ( Renderer r in spawnedObject.GetComponentsInChildren<Renderer>(true) )
        {
            r.enabled = true;
        }

        DrawCard(handIndex);

        if ( EnemyCollection.Instance != null )
        {
            EnemyCollection.Instance.UpdateAllRoutes();
        }

        return true;
    }

    public void AddEnergy(float amount)
    {
        _energy += amount;
        OnEnergyChanged?.Invoke(_energy);
    }

    public float GetEnergy()
    {
        return _energy;
    }

    public void Tick(float deltaTime)
    {
        _energyTimer += deltaTime;

        if ( _energyTimer >= 1f )
        {
            AddEnergy(_passiveEnergyPerSecond);
            _energyTimer -= 1f;
        }
    }

    public bool ValidateCardPlacement(HeroCardBase card, Tile hoveredTile)
    {
        if ( card == null || hoveredTile == null )
        {
            return false;
        }

        bool isTowerCard = card.PrefabToSpawn.GetComponent<TowerBase>() != null;

        if ( isTowerCard )
        {
            if ( hoveredTile.Type == Tile.TileType.Tower && hoveredTile.isTowerFree )
            {
                return GameBoard.Instance.IsTileValidForPlacement(hoveredTile);
            }
        }
        else
        {
            if ( hoveredTile.Type == Tile.TileType.Nothing )
            {
                return true;
            }
        }

        return false;
    }
}