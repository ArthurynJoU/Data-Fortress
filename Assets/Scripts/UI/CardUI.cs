using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Handles the visual representation and interaction logic for a specific defensive module card in the player's hand.
/// </summary>
public class CardUI : MonoBehaviour, IPointerDownHandler
{
    public static CardUI CurrentlySelectedCard = null;

    [Header("Settings")]
    public int HandIndex;

    [Header("UI References")]
    public Image Image;
    public TextMeshProUGUI Cost;
    private CardManager _cardBoss;

    [Header("Tooltips")]
    [SerializeField]
    private GameObject _infoPanel;
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _description;

    private HeroCardBase _currentCard;
    private PlayerInteractionController _interactionController;

    private HeroCardBase _pendingCard; 
    private bool _isAnimating = false;
    public GameObject GlowEffect;

    private Material _dissolveMaterial;

    private void Awake()
    {
        // Cleans up any duplicated components that might occur during instantiation
        CardUI[] clones = GetComponents<CardUI>();
        if ( clones.Length > 1 )
        {
            for ( int i = 1; i < clones.Length; i++ )
            {
                Destroy(clones[i]);
            }
        }

        _interactionController = FindFirstObjectByType<PlayerInteractionController>();
        _cardBoss = FindFirstObjectByType<CardManager>();

        if ( _cardBoss != null )
        {
            _cardBoss.OnCardUpdated -= UpdateCardVisuals;
            _cardBoss.OnEnergyChanged -= UpdateLiveEnergyText;

            _cardBoss.OnCardUpdated += UpdateCardVisuals;
            _cardBoss.OnEnergyChanged += UpdateLiveEnergyText;
        }

        if ( GlowEffect != null )
        {
            GlowEffect.SetActive(false);
        }

        if ( Image != null && Image.material != null )
        {
            _dissolveMaterial = new Material(Image.material);
            Image.material = _dissolveMaterial;
        }

        _infoPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if ( _cardBoss != null )
        {
            _cardBoss.OnCardUpdated -= UpdateCardVisuals;
            _cardBoss.OnEnergyChanged -= UpdateLiveEnergyText;
        }

        if ( CurrentlySelectedCard == this )
        {
            CurrentlySelectedCard = null;
        }
    }

    private void UpdateLiveEnergyText(float currentEnergy)
    {
        UpdateCostText();
    }

    public void SetSelected(bool isSelected)
    {
        if ( GlowEffect != null )
        {
            GlowEffect.SetActive(isSelected);
        }

        if ( isSelected )
        {
            CurrentlySelectedCard = this;
        }
        else if ( CurrentlySelectedCard == this )
        {
            CurrentlySelectedCard = null;
        }

        UpdateCostText();
    }

    private void UpdateCostText()
    {
        if ( _currentCard != null && Cost != null && Cost.enabled )
        {
            float currentEnergy = 0f;
            
            if ( _cardBoss != null )
            {
                currentEnergy = _cardBoss.GetEnergy();
            }

            int energyFloor = Mathf.FloorToInt(currentEnergy);

            if ( CurrentlySelectedCard == this )
            {
                Cost.text = $"{_currentCard.Cost} / {energyFloor}";
            }
            else if ( CurrentlySelectedCard == null )
            {
                Cost.text = $"0 / {energyFloor}";
            }
        }
    }

    private void UpdateCardVisuals(int index, HeroCardBase card)
    {
        if ( index == HandIndex )
        {
            if ( Image == null || Cost == null )
            {
                return;
            }

            _currentCard = card;

            if ( _isAnimating )
            {
                _pendingCard = card;
            }
            else if ( card != null )
            {
                Image.sprite = card.CardArt;

                SetSelected(false);

                Image.enabled = true;
                Cost.enabled = true;
            }
            else
            {
                Image.enabled = false;
                Cost.enabled = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if ( _currentCard != null && _interactionController != null && !_isAnimating )
        {
            _interactionController.BeginDrag(_currentCard, HandIndex, this);
        }
        
        ShowCardTooltip(_currentCard.PrefabToSpawn);
    }

    private void ShowCardTooltip(IInfoProvider info)
    {
        if ( info == null )
        {
            return;
        }

        _title.text = info.GetTitle();
        _description.text = info.GetDescription();

        _infoPanel.SetActive(true);
    }

    public void CloseCardTooltip()
    {
        _infoPanel.SetActive(false);
    }

    public void PlayDissolveAndAppear()
    {
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        _isAnimating = true;
        SetSelected(false);

        float t = 0;
        
        // Dissolution sequence
        while ( t < 1f )
        {
            t += Time.deltaTime * 3f;
            
            if ( _dissolveMaterial != null )
            {
                _dissolveMaterial.SetFloat("_DissolveAmount", t);
            }
            
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.15f, t);
            yield return null;
        }

        // Replacement sequence
        if ( _pendingCard != null )
        {
            _currentCard = _pendingCard;
            Image.sprite = _currentCard.CardArt;

            SetSelected(false);

            _pendingCard = null;
        }

        t = 1f;
        
        // Emergence sequence
        while ( t > 0f )
        {
            t -= Time.deltaTime * 3f;
            
            if ( _dissolveMaterial != null )
            {
                _dissolveMaterial.SetFloat("_DissolveAmount", t);
            }
            
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, 1f - t);
            yield return null;
        }

        if ( _dissolveMaterial != null )
        {
            _dissolveMaterial.SetFloat("_DissolveAmount", 0f);
        }
        
        transform.localScale = Vector3.one;
        _isAnimating = false;
    }
}