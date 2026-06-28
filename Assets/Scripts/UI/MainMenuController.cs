using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orchestrates navigation between primary application states and sub-menus.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject _mainMenuPanel;
    [SerializeField]
    private GameObject _levelsPanel;
    [SerializeField]
    private GameObject _libraryPanel;
    [SerializeField]
    private GameObject _settingsPanel;
    [SerializeField]
    private GameObject _aboutPanel;
    [SerializeField]
    private GameObject _rulesPanel;

    private void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        ShowPanel(_mainMenuPanel);
    }

    public void ShowPanel(GameObject targetPanel)
    {
        _mainMenuPanel.SetActive(false);
        _levelsPanel.SetActive(false);
        _libraryPanel.SetActive(false);
        _settingsPanel.SetActive(false);
        _aboutPanel.SetActive(false);
        _rulesPanel.SetActive(false);

        targetPanel.SetActive(true);
    }

    public void Btn_OpenStart() => ShowPanel(_levelsPanel);

    public void Btn_OpenLibrary()
    {
        ShowPanel(_libraryPanel);
        if ( LibraryInfoPanel.Instance != null )
        {
            LibraryInfoPanel.Instance.CloseAllPanels();
        }
    }

    public void Btn_OpenSettings() => ShowPanel(_settingsPanel);
    public void Btn_OpenAbout() => ShowPanel(_aboutPanel);

    public void Btn_OpenRules()
    {
        ShowPanel(_rulesPanel);
        if ( Rules.Instance != null )
        {
            Rules.Instance.CloseAllPanels();
        }
    }

    public void Btn_BackToMain() => ShowPanel(_mainMenuPanel);

    public void Btn_QuitGame()
    {
        Debug.Log("Shutting down the application...");
        Application.Quit();
    }

    public void Btn_StartLevel(int levelIndex)
    {
        Debug.Log($"Initiating level sequence: {levelIndex}");
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }
}