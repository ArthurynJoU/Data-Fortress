using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// A single centre for processing physical rays (Raycast) from the mouse/touch.
/// It's responsible for finding points on the grid for Drag-and-Drop and displaying Tooltips.
/// </summary>
public class PlayerInteractionController : MonoBehaviour
{
    [Header("Raycast Masks")]
    [Tooltip("If you click on the board, then you need to find the cell to do next point.")]
    [SerializeField] 
    private LayerMask _gameBoardLayer;
    [Tooltip("If you hover your mouse over the entity to see the tooltip, you don’t need to look for the board.")]
    [SerializeField] 
    private LayerMask _hoverLayer;

    [Header("Tooltip Settings")]
    [Tooltip("How long do you need to hover the mouse over the object?")]
    [SerializeField] 
    private float _hoverDelay = 3f;
    private bool _isTooltipActive;

    [Header("Drag-and-Drop Settings")]
    [SerializeField] 
    private Color _validColour = new Color(0f, 1f, 0f, 0.5f);   // green
    [SerializeField] 
    private Color _invalidColour = new Color(1f, 0f, 0f, 0.5f); // red

    private CardUI _selectedCardUI;

    /// <summary>
    /// Stores information about the card that the you are currently dragging with the mouse.
    /// </summary>
    private struct DragContext
    {
        public HeroCardBase Card;
        public int HandIndex; // its position in the hand (to return or remove)

        // to check if you’re carrying anything at the moment
        public bool IsActive => Card != null;
        public void Clear() 
        { 
            Card = null; 
            HandIndex = -1; 
        }
    }
    private DragContext _dragData;

    /// <summary>
    /// Stores data about the prefab of the tower that hovers over the cursor.
    /// </summary>
    private struct HologramContext
    {
        public GameObject RootObject;
        public Renderer[] Renderers; // link to resources to change the colour to green/red

        public void SetActive(bool isActive)
        {
            if ( RootObject != null && RootObject.activeSelf != isActive )
            {
                RootObject.SetActive(isActive);
            }
        }

        public void DestroyHologram()
        {
            if ( RootObject != null )
            {
                Destroy(RootObject);
            }
        }
    }
    private HologramContext _hologram;

    private CardManager _cardBoss;
    private Camera _mainCamera;
    private GameObject _currentHoveredObject;
    private float _currentHoverTimer;

    private void Start()
    {
        _mainCamera = Camera.main;
        _cardBoss = FindFirstObjectByType<CardManager>();
    }

    // are you up to something?
    private void Update()
    {
        if ( _dragData.IsActive ) // yes
        {
            HandleDragging(); // to move the hologram of the tower with mouse to check whether it’s possible to build
        }
        else // no
        {
            HandleTooltips(); // to display a tooltip
            HandleTowerRemoval(); // to call the delete command
        }
    }

    private void HandleDragging()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelDrag();
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // looking for a layer of board
        bool hitBoard = Physics.Raycast(ray, out RaycastHit hit, 1000f, _gameBoardLayer);
        if (hitBoard)
        {
            Tile hoveredTile = GameBoard.Instance.GetTileFromPosition(hit.point); // so that the hologram sits nicely
            if (hoveredTile != null && _hologram.RootObject != null)
            {
                _hologram.SetActive(true);
                _hologram.RootObject.transform.position = hoveredTile.WorldPosition;

                bool isPathValid = _cardBoss.ValidateCardPlacement(_dragData.Card, hoveredTile);

                bool hasEnoughEnergy = false;
                if ( _cardBoss.CurrentEnergy >= _dragData.Card.Cost )
                {
                    hasEnoughEnergy = true;
                }

                bool isValid = false;
                if ( isPathValid && hasEnoughEnergy )
                {
                    isValid = true;
                }

                if ( isValid ) 
                {
                    SetHologramColour(_validColour);
                }
                else
                {
                    SetHologramColour(_invalidColour);
                }

                if ( Mouse.current.leftButton.wasReleasedThisFrame || Mouse.current.leftButton.wasPressedThisFrame )
                {
                    // so that you don’t place a tower on the board by accidentally clicking the Pause button, for example
                    if ( EventSystem.current.IsPointerOverGameObject() )
                    {
                        return;
                    }

                    if ( isValid )
                    {
                        if ( _selectedCardUI != null )
                        {
                            _selectedCardUI.PlayDissolveAndAppear();
                        }

                        _cardBoss.PlayCard(_dragData.HandIndex, hoveredTile.WorldPosition);
                        CancelDrag();                
                    }
                    else
                    {
                        Debug.Log("Construction is not possible: the path is blocked or there is no power!");
                    }
                }
            }
        }
        else // if the cursor is not over the board
        {
            _hologram.SetActive(false);

            if ( (Mouse.current.leftButton.wasReleasedThisFrame || Mouse.current.leftButton.wasPressedThisFrame) && 
                 !EventSystem.current.IsPointerOverGameObject() )
            {
                CancelDrag();
            }
        }
    }

    public void CancelDrag()
    {
        _dragData.Clear();
        _hologram.DestroyHologram();

        if ( _selectedCardUI != null )
        {
            _selectedCardUI.CloseCardTooltip();
            _selectedCardUI.SetSelected(false);
            _selectedCardUI = null;
        }
    }

    // to call when you click on the card
    public void BeginDrag(HeroCardBase card, int handIndex, CardUI uiElement)
    {
        _dragData = new DragContext 
        { 
            Card = card, 
            HandIndex = handIndex 
        };
        _hologram.DestroyHologram();

        if ( _selectedCardUI != null )
        {
            _selectedCardUI.SetSelected(false);
        }

        _selectedCardUI = uiElement;
        _selectedCardUI.SetSelected(true);

        _hologram.RootObject = Instantiate(card.PrefabToSpawn.gameObject);

        // so that the hologram doesn’t look like a player
        foreach (var script in _hologram.RootObject.GetComponentsInChildren<MonoBehaviour>())
        {
            script.enabled = false;
        }
        foreach (var col in _hologram.RootObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        _hologram.Renderers = _hologram.RootObject.GetComponentsInChildren<Renderer>();

        if ( TooltipUI.Instance != null )
        {
            TooltipUI.Instance.StopHover();
        }
    }

    private void SetHologramColour(Color colour)
    {
        if (_hologram.Renderers == null)
        {
            return;
        }

        foreach ( var rend in _hologram.Renderers )
        { 
            foreach ( var mat in rend.materials )
            {
                if ( mat.HasProperty("_BaseColor") )
                {
                    mat.SetColor("_BaseColor", colour);
                }
                if ( mat.HasProperty("_Color") )
                { 
                    mat.color = colour; 
                }

                if ( mat.HasProperty("_EmissionColor") )
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", colour * 1.5f);
                }
            }
        }
    }

    private void HandleTooltips()
    {
        if ( TooltipUI.Instance == null || Mouse.current == null )
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if ( Physics.Raycast(ray, out RaycastHit hit, 100f, _hoverLayer) )
        {
            if ( _currentHoveredObject == hit.collider.gameObject )
            {
                // this is necessary to ensure that the _hoverDelay timer works regardless of whether the game time is slowed down or not
                _currentHoverTimer += Time.unscaledDeltaTime;
                if ( _currentHoverTimer >= _hoverDelay && !_isTooltipActive )
                {
                    IInfoProvider info = hit.collider.GetComponentInParent<IInfoProvider>();
                    if ( info != null )
                    {
                        TooltipUI.Instance.StartHover(info);
                        _isTooltipActive = true;
                    }
                }
            }
            else
            {
                _currentHoveredObject = hit.collider.gameObject;
                _currentHoverTimer = 0f;
                if ( _isTooltipActive )
                {
                    TooltipUI.Instance.StopHover();
                    _isTooltipActive = false;
                }
            }
        }
        else
        {
            if ( _currentHoveredObject != null )
            {
                _currentHoveredObject = null;
                _currentHoverTimer = 0f;
                if ( _isTooltipActive )
                {
                    TooltipUI.Instance.StopHover();
                    _isTooltipActive = false;
                }
            }
        }
    }

        private void HandleTowerRemoval()
        {
            if ( Mouse.current == null )
            {
                return;
            }

            if ( Mouse.current.rightButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject() )
            {
                Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if ( Physics.Raycast(ray, out RaycastHit hit, 100f, _hoverLayer) )
                {
                    TowerBase tower = hit.collider.GetComponentInParent<TowerBase>();

                    if ( tower != null )
                    {
                        if ( _cardBoss != null )
                        {
                            _cardBoss.AddEnergy(-10f);
                        }

                        tower.Die();
                    }
                }
            }
        }
    }