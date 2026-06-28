using System;
using UnityEngine;

/// <summary>
/// Supervises the global state of the current mission, including core integrity and victory conditions.
/// </summary>
public class LevelManager: MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField]
    private float _serverHP = 100f;
    [SerializeField]
    private CardManager _cardBoss;

    [Header("Level Data")]
    [SerializeField]
    private LevelBase[] _allLevels;
    
    private LevelBase _currentLevel;
    private float _maximumCoreHealth;
    private int _totalEnemiesSpawned = 0;

    public LevelBase CurrentLevel => _currentLevel;
    public int TotalLevels => _allLevels.Length;

    public event Action<float, float> OnHealthChanged;
    public event Action OnLevelLost;
    public event Action OnLevelWon;

    private void Awake()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        _maximumCoreHealth = _serverHP;

        int selectedIndex = PlayerPrefs.GetInt("SelectedLevel", 1);
        Debug.Log($"Mission {selectedIndex} initialised.");

        int arrayIndex = selectedIndex - 1;

        if ( arrayIndex >= 0 && arrayIndex < _allLevels.Length )
        {
            _currentLevel = _allLevels[arrayIndex];
        }
        else
        {
            _currentLevel = _allLevels[0];
        }
    }

    public void OnEnemyReached(float damage)
    {
        _serverHP -= damage;
        
        float reportedHealth = _serverHP;
        if ( reportedHealth < 0f )
        {
            reportedHealth = 0f;
        }
        
        OnHealthChanged?.Invoke(reportedHealth, _maximumCoreHealth);

        if ( _serverHP <= 0f )
        {
            GameOver();
        }
    }

    public void OnEnemySpawned()
    {
        _totalEnemiesSpawned++;
    }

    public void AddEnergy(float amount)
    {
        if ( _cardBoss != null )
        {
            _cardBoss.AddEnergy(amount);
        }
    }

    public void Victory()
    {
        Debug.Log("MISSION ACCOMPLISHED! The core server remains secure.");
        OnLevelWon?.Invoke();
        Time.timeScale = 0f;
    }

    private void GameOver()
    {
        Debug.Log("CRITICAL FAILURE! Core defenses have been breached.");
        OnLevelLost?.Invoke();
        Time.timeScale = 0f;
    }
}