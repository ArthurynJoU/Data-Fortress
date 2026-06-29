using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Supervises the in-game HUD and situational popup screens.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Server Health UI")]
    [SerializeField]
    private TextMeshProUGUI _healthText;

    [Header("Score UI")]
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    private int _score = 0;
    private ScoreSender _sender;

    [Header("Screen & Panels")]
    [SerializeField]
    private GameObject _gameOverPanel;
    [SerializeField]
    private GameObject _victoryPanel;
    [SerializeField]
    private GameObject _pausePanel;
    [SerializeField]
    private GameObject _settingsPanel;

    private void Start()
    {
        if ( _gameOverPanel != null )
        {
            _gameOverPanel.SetActive(false);
        }
        if ( _victoryPanel != null )
        {
            _victoryPanel.SetActive(false);
        }
        if ( _pausePanel != null )
        {
            _pausePanel.SetActive(false);
        }
        if ( _settingsPanel != null )
        {
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
            _settingsPanel.SetActive(false);
        }

        if ( LevelManager.Instance != null )
        {
            LevelManager.Instance.OnHealthChanged += UpdateHealthUI;
            LevelManager.Instance.OnLevelWon += UpdateScoreUI;
            LevelManager.Instance.OnLevelWon += ShowVictoryScreen;
            LevelManager.Instance.OnLevelLost += ShowGameOverScreen;
        }

        if ( GameSessionController.Instance != null )
        {
            GameSessionController.Instance.OnPauseStateChanged += TogglePauseScreen;
        }

        _score = 0;
        _sender = FindFirstObjectByType<ScoreSender>();
    }

    private void OnDestroy()
    {
        if ( LevelManager.Instance != null )
        {
            LevelManager.Instance.OnHealthChanged -= UpdateHealthUI;
            LevelManager.Instance.OnLevelWon -= UpdateScoreUI;
            LevelManager.Instance.OnLevelWon -= ShowVictoryScreen;
            LevelManager.Instance.OnLevelLost -= ShowGameOverScreen;
        }

        if ( GameSessionController.Instance != null )
        {
            GameSessionController.Instance.OnPauseStateChanged -= TogglePauseScreen;
        }

        _score = 0;
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if ( _healthText != null )
        {
            _healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
        }
    }

    private void UpdateScoreUI()
    {
        if ( LevelManager.Instance != null )
        _score = LevelManager.Instance.GetScore();

        _scoreText.text = $"Your score: {_score}";
    }

    private void ShowGameOverScreen()
    {
        if ( _gameOverPanel != null )
        {
            _gameOverPanel.SetActive(true);
        }
    }

    private void ShowVictoryScreen()
    {
        if ( _victoryPanel != null )
        {
            _victoryPanel.SetActive(true);
        }

        if ( _sender != null )
        _sender.SendScore(_score);
    }

    private void TogglePauseScreen(bool isPaused)
    {
        if ( _pausePanel != null )
        {
            _pausePanel.SetActive(isPaused);
        }
    }

    public void ButtonRestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Btn_NextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        int nextLevel = currentLevel + 1;

        if ( LevelManager.Instance != null && nextLevel >= LevelManager.Instance.TotalLevels )
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
            return;
        }

        PlayerPrefs.SetInt("SelectedLevel", nextLevel);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButtonOpenSettings()
    {
        if ( _settingsPanel != null )
        {
            _settingsPanel.SetActive(true);
        }
    }

    public void ButtonCloseSettings()
    {
        if ( _settingsPanel != null )
        {
            _settingsPanel.SetActive(false);
        }
    }

    public void Btn_ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Btn_RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}