using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LibraryInfoPanel : MonoBehaviour
{
    public static LibraryInfoPanel Instance { get; private set; }

    [Header("UI Links")]
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _description;

    [Header("Category Panels")]
    [SerializeField]
    private GameObject _towersPanel;
    [SerializeField]
    private GameObject _enemiesPanel;
    [SerializeField]
    private GameObject _effectsPanel;
    [SerializeField]
    private GameObject _infoPanel;

    private void Awake()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CloseAllPanels();
        ClearInfo();
    }

    public void CloseAllPanels()
    {
        _towersPanel.SetActive(false);
        _effectsPanel.SetActive(false);
        _enemiesPanel.SetActive(false);
        _infoPanel.SetActive(false);
    }

    public void ShowPanel(GameObject targetPanel)
    {
        CloseAllPanels();
        targetPanel.SetActive(true);
    }

    public void ShowInfo(IInfoProvider info)
    {
        if ( info == null )
        {
            return;
        }

        _title.text = info.GetTitle();
        _description.text = info.GetDescription();

        _infoPanel.SetActive(true);
    }

    public void ClearInfo()
    {
        _title.text = "Select an item...";
        _description.text = "";
    }

    public void Btn_OpenTowers() => ShowPanel(_towersPanel);
    public void Btn_OpenEffects() => ShowPanel(_effectsPanel);
    public void Btn_OpenEnemies() => ShowPanel(_enemiesPanel);
}