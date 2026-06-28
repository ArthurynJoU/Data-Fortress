using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _description;
    [SerializeField]
    private Image _image;

    [Tooltip("RectTransform of the object that should follow the cursor or appear next to it.")]
    [SerializeField]
    private RectTransform _tooltipRect;
    [SerializeField]
    private Vector2 _offset = new Vector2(15f, -15f);

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
        _panel.SetActive(false);
    }

    private void Update()
    {
        if ( !_panel.activeSelf )
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        _tooltipRect.position = mousePosition + _offset;
    }

    public void StartHover(IInfoProvider info)
    {
        if ( info == null )
        {
            return;
        }

        UpdateUI(info);
        _panel.SetActive(true);
    }

    public void StopHover()
    {
        _panel.SetActive(false);
    }

    private void UpdateUI(IInfoProvider info)
    {
        _title.text = info.GetTitle();
        _description.text = info.GetDescription();

        if ( info.GetIcon() != null )
        {
            _image.sprite = info.GetIcon();
            _image.enabled = true;
        }
        else
        {
            _image.enabled = false;
        }
    }
}