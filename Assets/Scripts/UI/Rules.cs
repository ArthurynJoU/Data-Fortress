using UnityEngine;

public class Rules : MonoBehaviour
{
    public static Rules Instance { get; private set; }

    [Header("Panels")]
    [SerializeField]
    private GameObject _controlPanel;
    [SerializeField]
    private GameObject _clickPanel;
    [SerializeField]
    private GameObject _additionalPanel;

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
    }

    public void CloseAllPanels()
    {
        _controlPanel.SetActive(false);
        _additionalPanel.SetActive(false);
        _clickPanel.SetActive(false);
    }

    public void Btn_OpenRule1()
    {
        CloseAllPanels();
        _controlPanel.SetActive(true);
    }

    public void Btn_Rule2()
    {
        CloseAllPanels();
        _clickPanel.SetActive(true);
    }

    public void Btn_OpenRule3()
    {
        CloseAllPanels();
        _additionalPanel.SetActive(true);
    }
}