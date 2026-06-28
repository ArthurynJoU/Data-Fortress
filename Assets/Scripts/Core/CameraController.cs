using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField]
    private float _rotationSpeed = 100f;
    [SerializeField]
    [Tooltip("Smoothness of stopping")]
    private float _smoothRotation = 5f;

    [Header("Camera Distance")]
    [SerializeField]
    private float _cameraHeight = 15f;
    [SerializeField]
    private float _orbitDistance = 15f;
    [SerializeField]
    private float _minDistance = 5f;
    [SerializeField]
    private float _maxDistance = 30f;
    [SerializeField]
    private float _zoomSpeed = 2f;

    private Vector3 _boardCentre;
    private bool _isCentreCalculated = false;

    private float _currentRotationY = 0f;
    private float _targetRotationY = 0f;
    private float _targetDistance;

    private void Start()
    {
        _targetRotationY = 45f;
        _currentRotationY = _targetRotationY;
        _targetDistance = _orbitDistance;
    }

    private void Update()
    {
        if ( !_isCentreCalculated )
        {
            CalculateBoardCentre();
        }

        HandleInput();
        ApplyOrbitalRotation();
    }

    private void CalculateBoardCentre()
    {
        if ( LevelManager.Instance != null && LevelManager.Instance.CurrentLevel != null && GameBoard.Instance != null )
        {
            Vector2Int gridSize = LevelManager.Instance.CurrentLevel.GridSize;
            Tile centreTile = GameBoard.Instance.GetTile(new Vector2Int(gridSize.x / 2, gridSize.y / 2));

            if ( centreTile != null )
            {
                _boardCentre = centreTile.WorldPosition;
                _isCentreCalculated = true;
            }
        }
    }

    private void HandleInput()
    {
        if ( Mouse.current != null )
        {
            float rawScroll = Mouse.current.scroll.ReadValue().y;
            float scroll = 0f;

            if ( rawScroll > 0 )
            {
                scroll = 1f;
            }
            else if ( rawScroll < 0 )
            {
                scroll = -1f;
            }

            _targetDistance -= _rotationSpeed * scroll * _zoomSpeed * SettingsManager.GlobalSensitivity * Time.deltaTime;
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }

        if ( Keyboard.current != null )
        {
            float currentRotSpeed = _rotationSpeed * SettingsManager.GlobalSensitivity;
            float rotInput = 0f;
            float zoomInput = 0f;

            if ( Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed )
            {
                rotInput += 1f;
            }
            if ( Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed )
            {
                rotInput -= 1f;
            }
            if ( Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed )
            {
                zoomInput -= 1f;
            }
            if ( Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed )
            {
                zoomInput += 1f;
            }

            _targetRotationY += rotInput * currentRotSpeed * Time.deltaTime;

            if ( zoomInput != 0f )
            {
                _targetDistance += zoomInput * currentRotSpeed * Time.deltaTime;
                _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            }
        }
    }

    private void ApplyOrbitalRotation()
    {
        if ( !_isCentreCalculated )
        {
            return;
        }

        _currentRotationY = Mathf.Lerp(_currentRotationY, _targetRotationY, _smoothRotation * Time.unscaledDeltaTime);
        _orbitDistance = Mathf.Lerp(_orbitDistance, _targetDistance, _smoothRotation * Time.unscaledDeltaTime);

        float radians = _currentRotationY * Mathf.Deg2Rad;

        /* Calculation of Camera coordinates: newX, newZ
         * * C(newX, newZ) - Camera
         * *
         * |\
         * |c\
         * |  \
         * |   \     .Y-axis
         * |    \             A
         * |     \            |
         * |      \           |
         * |       \          |
         * |        \         |
         * |         \        | Z-axis
         * |          \       |
         * |           \      |
         * |            \     |
         * |             \    |
         * |              \   
         * | a            b \
         * *---------------*
         * A  ----------->   B(_boardCentre.x, _boardCentre.z) - Board Centre
         * X-axis
         * * Right-angled triangle ABC, where angle a = 90 degrees
         * BC - hypotenuse, i.e. _orbitDistance
         * Point B is _boardCentre
         * Point A is the intersection of AB and AC at a right angle a
         * Angle c is _currentRotationY
         * AB is length along X-axis from A to B. 
         * AC is length along Z-axis from B to C.
         * * AB = sin(c) * BC
         * AC = cos(c) * BC
         * * newX = _boardCentre.x + AB
         * newZ = _boardCentre.z - AC
         * */
        float newX = _boardCentre.x + Mathf.Sin(radians) * _orbitDistance;
        float newZ = _boardCentre.z - Mathf.Cos(radians) * _orbitDistance;

        transform.position = new Vector3(newX, _boardCentre.y + _cameraHeight, newZ);
        transform.LookAt(_boardCentre);
    }
}