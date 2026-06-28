using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// The principal orchestrator of the game loop.
/// Regulates the flow of time, input processing, and the execution sequence for all primary modules.
/// </summary>
public class GameSessionController: MonoBehaviour
{
    public static GameSessionController Instance { get; private set; }

    [Header("Core Dependencies")]
    [SerializeField]
    private GameBoard _gameBoard;
    [SerializeField]
    private EnemySpawner _enemySpawner;
    [SerializeField]
    private CardManager _cardBoss;

    private bool _isPaused = false;
    private bool _isGameOver = false;

    public event Action<bool> OnPauseStateChanged;

    private List<ITickable> _deployedTowers = new List<ITickable>();
    private List<ITickable> _activeEffects = new List<ITickable>();

    private GameObject _currentEnvironment;
    private LevelBase _myLevelData;

    private void Awake()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _myLevelData = LevelManager.Instance.CurrentLevel;

        if ( _gameBoard != null && _myLevelData != null )
        {
            if ( _myLevelData.EnvironmentPrefab != null )
            {
                _currentEnvironment = Instantiate(_myLevelData.EnvironmentPrefab, Vector3.zero, Quaternion.identity);
            }

            int width = _myLevelData.GridSize.x;
            int height = _myLevelData.GridSize.y;

            _gameBoard.Initialize(width, height);
        }
        else
        {
            Debug.LogError("GameBoard or LevelBase not appointed to GameSessionController!");
        }

        Time.timeScale = 1f;
        _isPaused = false;
    }

    private void Update()
    {
        HandleInput();

        if ( _isPaused || _isGameOver )
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        if ( _enemySpawner != null && _enemySpawner is ITickable tickableSpawner )
        {
            tickableSpawner.Tick(deltaTime);
        }

        int tIndex = _deployedTowers.Count - 1;
        while ( tIndex >= 0 )
        {
            _deployedTowers[tIndex].Tick(deltaTime);
            tIndex--;
        }
        
        int eIndex = _activeEffects.Count - 1;
        while ( eIndex >= 0 )
        {
            _activeEffects[eIndex].Tick(deltaTime);
            eIndex--;
        }
        
        if ( EnemyCollection.Instance != null && EnemyCollection.Instance is ITickable tickableEnemies )
        {
            tickableEnemies.Tick(deltaTime);
        }

        if ( _cardBoss != null )
        {
            _cardBoss.Tick(deltaTime);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if ( _enemySpawner != null && EnemyCollection.Instance != null )
        {
            if ( _enemySpawner.IsFinished() && EnemyCollection.Instance.ActiveCount == 0 )
            {
                _isGameOver = true;

                if ( LevelManager.Instance != null )
                {
                    LevelManager.Instance.Victory();
                }
            } 
        }
    }

    private void HandleInput()
    {
        if ( Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame )
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if ( _isPaused )
        {
            _isPaused = false;
        }
        else
        {
            _isPaused = true;
        }

        if ( _isPaused )
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        OnPauseStateChanged?.Invoke(_isPaused);
    }

    public void RegisterTower(ITickable tower) 
    {
        _deployedTowers.Add(tower);
        EnemyCollection.Instance.UpdateAllRoutes();
    }
    
    public void UnregisterTower(ITickable tower)
    {
        _deployedTowers.Remove(tower);
        EnemyCollection.Instance.UpdateAllRoutes();
    }

    public void RegisterEffect(ITickable effect)
    {
        if ( !_activeEffects.Contains(effect) )
        {
            _activeEffects.Add(effect);
        }
    }
    
    public void UnregisterEffect(ITickable effect)
    {
        if ( _activeEffects.Contains(effect) )
        {
            _activeEffects.Remove(effect);
        }
    }
}