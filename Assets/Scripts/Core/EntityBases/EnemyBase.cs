using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The basic logic for all malicious programmes.
/// </summary>
public abstract class EnemyBase : GameEntity, IDamageable, ITickable
{
    [Header("Enemy Type")]
    public EnemyType Category;

    [Header("Movement Parametrs")]
    [SerializeField]
    protected float Speed;
    
    /// <summary>
    /// A numerical variable (usually from 0 to 1 or simply a distance counter) that shows how far the enemy has advanced along the path.
    /// </summary>
    public float PathProgress { get; protected set; }

    [Header("Pathfinding Data")]
    protected List<Vector3> _currentPath;
    protected int _currentPathIndex;
    protected Vector3 _positionFrom;
    protected Vector3 _positionTo;
    protected Vector2Int _destinationCoordinates;

    [Header("Visuals")]
    [SerializeField]
    protected EnemyLevelBase Config;
    [SerializeField]
    private Transform ModelTransform;

    [Header("Death Visuals")]
    [SerializeField]
    private ParticleSystem _deathEffectPrefab;
    [SerializeField]
    private float _deathAnimationTime = 1f;
    [SerializeField]
    private string _shaderDissolveParameter = "_DissolveAmount";
    [SerializeField]
    private Material _dissolveMaterialTemplate;
    
    [Header("Destination Visuals")]
    [SerializeField]
    protected ParticleSystem _destinationTeleportPrefab;
    [SerializeField]
    protected float _destinationAnimationTime = 1f;

    [Header("Status Effect Auras")]
    [SerializeField] 
    private ParticleSystem _fearAuraPrefab;
    [SerializeField] 
    private ParticleSystem _stunAuraPrefab;
    [SerializeField] 
    private ParticleSystem _slowAuraPrefab;

    [Header("Spawn Settings")]
    [SerializeField]
    protected float _spawnImmunityDuration = 0.5f;

    [Header("Audio")]
    [SerializeField]
    protected AudioClip AttackSound;
    [SerializeField]
    protected AudioClip DeathSound;
    [SerializeField]
    protected AudioClip BirthSound;
    [Range(0f, 1f)]
    [SerializeField]
    protected float Volume = 0.6f;

    // Internal States
    protected float _stunTimer;
    protected float _slowTimer;
    protected float _slowFactor = 1f;
    protected float _fearTimer;
    protected float _immunityTimer = 0f;
    protected float _pathOffSet;
    protected float _baseSpeed;
    protected float EnergyForDeath = 10f;
    protected float ScoreForDeath = 10;
    
    private bool _isDying = false;
    private ParticleSystem _activeFearAura;
    private ParticleSystem _activeStunAura;
    private ParticleSystem _activeSlowAura;
    
    private Dictionary<MeshRenderer, Material[]> _originalMaterials = new Dictionary<MeshRenderer, Material[]>();
    private bool _materialsSaved = false;

    // Properties
    public virtual bool IsTargetable { get; set; } = true;
    public bool IsImmune => _immunityTimer > 0f;

    public override void Initialize()
    {
        base.Initialize();

        // Ensures visual integrity upon respawn
        SaveAndRestoreMaterials();
        ClearCrowdControl();

        Collider col = GetComponent<Collider>();
        if ( col != null )
        {
            col.enabled = true;
        }

        _immunityTimer = _spawnImmunityDuration;
        _isDying = false;
        IsTargetable = true;

        if ( Config != null )
        {
            float scale = Config.ScaleRange.RandomValueRange;
            transform.localScale = new Vector3(scale, scale, scale);
            
            _baseSpeed = Config.SpeedRange.RandomValueRange;
            Speed = _baseSpeed;
            _pathOffSet = Config.PathOffSetRange.RandomValueRange;

            if ( ModelTransform != null )
            {
                ModelTransform.localPosition = new Vector3(_pathOffSet, 0, 0);
                ModelTransform.localScale = Vector3.one;
            }
        }

        PathProgress = 0f;

        if ( EnemyCollection.Instance != null )
        {
            EnemyCollection.Instance.Register(this);
        }

        if ( BirthSound != null )
        {
            AudioSource.PlayClipAtPoint(BirthSound, transform.position, Volume);
        }
    }

    public override void Recycle()
    {
        if ( _activeFearAura != null )
        {
            Destroy(_activeFearAura.gameObject);
        }

        if ( _activeStunAura != null )
        {
            Destroy(_activeStunAura.gameObject);
        }

        if ( _activeSlowAura != null )
        {
            Destroy(_activeSlowAura.gameObject);
        }

        if ( EnemyCollection.Instance != null )
        {
            EnemyCollection.Instance.Unregister(this);
        }

        base.Recycle();
    }

    private void SaveAndRestoreMaterials()
    {
        if ( !_materialsSaved )
        {
            MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>(true);
            
            foreach ( MeshRenderer r in allRenderers )
            {
                _originalMaterials[r] = r.sharedMaterials;
            }
            
            _materialsSaved = true;
        }

        foreach ( var mat in _originalMaterials )
        {
            if ( mat.Key != null )
            {
                mat.Key.materials = mat.Value;
            }
        }
    }

    public virtual void Tick(float deltaTime)
    {
        if ( _isDying )
        {
            return;
        }

        if ( _immunityTimer > 0f )
        {
            _immunityTimer -= deltaTime;
        }

        float currentSpeed = Speed;

        if ( _stunTimer > 0f )
        {
            _stunTimer -= deltaTime;
            currentSpeed = 0f;

            if ( _stunTimer <= 0f )
            {
                if ( _activeStunAura != null )
                {
                    Destroy(_activeStunAura.gameObject);
                }
            }
        }
        else
        {
            if ( _activeStunAura != null )
            {
                Destroy(_activeStunAura.gameObject);
            }
        }

        if ( _slowTimer > 0f )
        {
            _slowTimer -= deltaTime;
            
            if ( _stunTimer <= 0f )
            {
                currentSpeed *= _slowFactor;
            }

            if ( _slowTimer <= 0f )
            {
                if ( _activeSlowAura != null )
                {
                    Destroy(_activeSlowAura.gameObject);
                }
            }
        }
        else
        {
            if ( _activeSlowAura != null )
            {
                Destroy(_activeSlowAura.gameObject);
            }
        }

        if ( _fearTimer > 0f )
        {
            _fearTimer -= deltaTime;
            currentSpeed = -Mathf.Abs(currentSpeed);

            if ( _fearTimer <= 0f )
            {
                if ( _activeFearAura != null )
                {
                    Destroy(_activeFearAura.gameObject);
                }
            }
        }
        else
        {
            if ( _activeFearAura != null )
            {
                Destroy(_activeFearAura.gameObject);
            }
        }

        if ( PathProgress < 0f )
        {
            PathProgress = 0f;
        }

        Move(deltaTime, currentSpeed);
    }

    protected virtual void Move(float deltaTime, float actualSpeed)
    {
        if ( _currentPath == null )
        {
            return;
        }

        if ( _currentPathIndex >= _currentPath.Count )
        {
            return;
        }

        float dx = _positionTo.x - _positionFrom.x;
        float dy = _positionTo.y - _positionFrom.y;
        float dz = _positionTo.z - _positionFrom.z;
        float segmentDistance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

        if ( segmentDistance == 0f )
        {
            segmentDistance = 0.001f;
        }

        PathProgress += (actualSpeed * deltaTime) / segmentDistance;

        float currentX = _positionFrom.x + (_positionTo.x - _positionFrom.x) * PathProgress;
        float currentY = _positionFrom.y + (_positionTo.y - _positionFrom.y) * PathProgress;
        float currentZ = _positionFrom.z + (_positionTo.z - _positionFrom.z) * PathProgress;

        transform.position = new Vector3(currentX, currentY, currentZ);

        Vector3 direction = new Vector3(dx, dy, dz).normalized;
        
        if ( direction != Vector3.zero )
        {
            Direction currentDirection = DirectionExtensions.VectorToDirection(direction);
            Quaternion targetRotation = currentDirection.GetRotation();
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 6f);
        }

        if ( PathProgress >= 1f )
        {
            float overshootPercent = PathProgress - 1f;
            float overshootMeters = overshootPercent * segmentDistance;

            _currentPathIndex++;

            if ( _currentPathIndex < _currentPath.Count )
            {
                _positionFrom = _positionTo;
                _positionTo = _currentPath[_currentPathIndex];

                float newDx = _positionTo.x - _positionFrom.x;
                float newDy = _positionTo.y - _positionFrom.y;
                float newDz = _positionTo.z - _positionFrom.z;
                float newSegmentDistance = Mathf.Sqrt(newDx * newDx + newDy * newDy + newDz * newDz);
                
                if ( newSegmentDistance == 0f )
                {
                    newSegmentDistance = 0.001f;
                }

                PathProgress = overshootMeters / newSegmentDistance;
            }
            else
            {
                ReachDestination();
            }
        }
    }

    public void BuildNewRoute()
    {
        if ( GameBoard.Instance == null )
        {
            return;
        }

        if ( CurrentHealth <= 0 )
        {
            return;
        }

        Tile startTile = GameBoard.Instance.GetTileFromPosition(transform.position);
        List<Vector3> newPath = GameBoard.Instance.FindPaths(startTile.Coordinates, _destinationCoordinates);

        if ( newPath != null )
        {
            if ( newPath.Count > 0 )
            {
                SetPath(newPath);
            }
        }
    }

    public virtual void SetPath(List<Vector3> path)
    {
        _currentPath = path;
        _currentPathIndex = 0;
        PathProgress = 0f;

        if ( _currentPath != null )
        {
            if ( _currentPath.Count > 0 )
            {
                transform.position = _currentPath[0];
                _positionFrom = _currentPath[0];

                if ( _currentPath.Count > 1 )
                {
                    _currentPathIndex = 1;
                    _positionTo = _currentPath[1];
                    transform.LookAt(_positionTo);
                }
            }
        }
    }

    public void CopyRouteFrom(EnemyBase parent)
    {
        _currentPath = parent._currentPath;
        _currentPathIndex = parent._currentPathIndex;
        _positionFrom = parent._positionFrom;
        _positionTo = parent._positionTo;
        PathProgress = parent.PathProgress;
        _destinationCoordinates = parent._destinationCoordinates;
    }

    public virtual void TakeDamage(float amount)
    {
        if ( !gameObject.activeInHierarchy )
        {
            return;
        }

        if ( CurrentHealth <= 0 )
        {
            return;
        }

        if ( _isDying )
        {
            return;
        }

        CurrentHealth -= amount;

        if ( CurrentHealth <= 0 )
        {
            Die();
        }
    }

    public void ApplyStun(float duration)
    {
        _stunTimer = duration;

        if ( _activeStunAura == null )
        {
            if ( _stunAuraPrefab != null )
            {
                _activeStunAura = Instantiate(_stunAuraPrefab, transform.position, Quaternion.identity, transform);
            }
        }
    }

    public void ApplySlow(float duration, float factor)
    {
        _slowTimer = duration;
        _slowFactor = factor;

        if ( _activeSlowAura == null )
        {
            if ( _slowAuraPrefab != null )
            {
                _activeSlowAura = Instantiate(_slowAuraPrefab, transform.position, Quaternion.identity, transform);
            }
        }
    }

    public void ApplyFear(float duration)
    {
        _fearTimer = duration;

        if ( _activeFearAura == null )
        {
            if ( _fearAuraPrefab != null )
            {
                _activeFearAura = Instantiate(_fearAuraPrefab, transform.position, Quaternion.identity, transform);
            }
        }
    }

    public void ClearCrowdControl()
    {
        _stunTimer = 0f;
        _slowTimer = 0f;
        _fearTimer = 0f;

        if ( _activeStunAura != null )
        {
            Destroy(_activeStunAura.gameObject);
        }

        if ( _activeSlowAura != null )
        {
            Destroy(_activeSlowAura.gameObject);
        }

        if ( _activeFearAura != null )
        {
            Destroy(_activeFearAura.gameObject);
        }
    }

    public virtual void Die()
    {
        if ( !gameObject.activeInHierarchy )
        {
            return;
        }

        if ( _isDying )
        {
            return;
        }

        _isDying = true;
        IsTargetable = false;

        if ( LevelManager.Instance != null )
        {
            LevelManager.Instance.AddEnergy(EnergyForDeath, ScoreForDeath);
        }

        Collider col = GetComponent<Collider>();
        if ( col != null )
        {
            col.enabled = false;
        }

        if ( DeathSound != null )
        {
            AudioSource.PlayClipAtPoint(DeathSound, transform.position, Volume);
        }

        StartCoroutine(DeathRoutine());
    }

    // Prevents material cloning and keeps performance stable during mass destruction
    private System.Collections.IEnumerator DeathRoutine()
    {
        if ( _deathEffectPrefab != null )
        {
            Vector3 effectPos = transform.position + new Vector3(0, 0.5f, 0);
            ParticleSystem effect = Instantiate(_deathEffectPrefab, effectPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        if ( allRenderers.Length > 0 )
        {
            if ( _dissolveMaterialTemplate != null )
            {
                for ( int i = 0; i < allRenderers.Length; i++ )
                {
                    Renderer rend = allRenderers[i];
                    Material[] sharedDyingMats = new Material[rend.sharedMaterials.Length];
                    
                    for ( int j = 0; j < sharedDyingMats.Length; j++ )
                    {
                        sharedDyingMats[j] = _dissolveMaterialTemplate;
                    }
                    
                    rend.sharedMaterials = sharedDyingMats;
                }

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                float elapsedTimer = 0f;

                while (elapsedTimer < _deathAnimationTime)
                {
                    elapsedTimer += Time.deltaTime;
                    float currentProgress = elapsedTimer / _deathAnimationTime;

                    foreach (Renderer rend in allRenderers)
                    {
                        if (rend != null)
                        {
                            rend.GetPropertyBlock(propertyBlock);
                            propertyBlock.SetFloat(_shaderDissolveParameter, currentProgress);
                            rend.SetPropertyBlock(propertyBlock);
                        }
                    }

                    yield return null;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(_deathAnimationTime);
        }

        _isDying = false;
        Recycle();
    }

    protected virtual void ReachDestination()
    {
        StartCoroutine(DestinationRoutine());
    }

    // We utilise MaterialPropertyBlock to prevent excessive garbage collection
    protected virtual System.Collections.IEnumerator DestinationRoutine()
    {
        Speed = 0f;
        IsTargetable = false;
        _isDying = true;

        Collider col = GetComponent<Collider>();
        if ( col != null )
        {
            col.enabled = false;
        }

        if ( LevelManager.Instance != null )
        {
            LevelManager.Instance.OnEnemyReached(Strength);
        }

        MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>();

        if ( allRenderers.Length > 0 )
        {
            if ( _dissolveMaterialTemplate != null )
            {
                for ( int i = 0; i < allRenderers.Length; i++ )
                {
                    MeshRenderer rend = allRenderers[i];
                    Material[] sharedDyingMats = new Material[rend.sharedMaterials.Length];
                    
                    for ( int j = 0; j < sharedDyingMats.Length; j++ )
                    {
                        sharedDyingMats[j] = _dissolveMaterialTemplate;
                    }
                    
                    rend.sharedMaterials = sharedDyingMats;
                }

                if ( _destinationTeleportPrefab != null )
                {
                    Instantiate(_destinationTeleportPrefab, transform.position, Quaternion.identity);
                }

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                float elapsedTimer = 0f;

                while ( elapsedTimer < _destinationAnimationTime )
                {
                    elapsedTimer += Time.deltaTime;
                    float currentProgress = elapsedTimer / _destinationAnimationTime;

                    foreach ( MeshRenderer rend in allRenderers )
                    {
                        rend.GetPropertyBlock(propertyBlock);
                        propertyBlock.SetFloat(_shaderDissolveParameter, currentProgress);
                        rend.SetPropertyBlock(propertyBlock);
                    }

                    yield return null;
                }
            }
        }
        else
        {
            if ( _destinationTeleportPrefab != null )
            {
                Instantiate(_destinationTeleportPrefab, transform.position, Quaternion.identity);
            }
            
            yield return new WaitForSeconds(_destinationAnimationTime);
        }

        _isDying = false;

        Recycle();
    }

    public void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public float GetPathProgress()
    {
        return PathProgress;
    }

    public void SetDestination(Vector2Int endPoint)
    {
        _destinationCoordinates = endPoint;
    }

    public float GetHealth()
    {
        return CurrentHealth;
    }

    public float GetBaseSpeed()
    {
        return _baseSpeed;
    }

    public float GetStrength()
    {
        return Strength;
    }

    public void SetStrength(float damage)
    {
        Strength = damage;
    }
}