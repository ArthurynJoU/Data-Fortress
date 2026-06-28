using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base entity for protection server.
/// </summary>
public abstract class TowerBase : GameEntity, IDamageable, ITickable
{
    [Header("Tower Combat Parameters")]
    [Tooltip("A radius of the invisible circle around the tower.")]
    [SerializeField]
    protected float DetectionRadius = 3f;
    [Tooltip("A delay between shots in seconds.")]
    [SerializeField]
    protected float BreakTime = 1f;
    [Tooltip("With each shot, or simply over time, the tower loses its \"stability\".")]
    [SerializeField]
    protected float Attrition = 1f;

    [Header("Visuals")]
    [SerializeField]
    protected Transform Turret;
    [SerializeField]
    protected Transform Laser;

    [Header("Targeting Priorities")]
    [Tooltip("List of enemies types that this tower is capable of attacking")]
    [SerializeField]
    protected List<EnemyType> TargetableCategories;
    [SerializeField]
    protected LayerMask EnemyLayerMask;
    [SerializeField]
    protected int MaxTargets = 1;

    protected List<EnemyBase> CurrentTargets = new List<EnemyBase>();
    // optimisation of Garbage Collector
    private Collider[] _hitsEnemies = new Collider[10];
    private List<EnemyBase> _validEnemies = new List<EnemyBase>();

    [Header("Death Visuals")]
    [SerializeField]
    protected ParticleSystem _deathEffectPrefab;
    [SerializeField]
    protected float _deathAnimationTime = 1f;
    [SerializeField]
    protected string _shaderDissolveParameter = "_DissolveAmount";
    [SerializeField]
    protected Material _dissolveMaterialTemplate;

    [Header("Audio")]
    [SerializeField]
    protected AudioClip ShootSound;
    [SerializeField]
    protected AudioClip DeathSound;
    [SerializeField]
    protected AudioClip BirthSound;
    [Range(0f, 1f)]
    [SerializeField]
    protected float Volume = 0.6f;

    // States
    protected float _currentReloadTimer;
    protected float _freezeTimer;
    protected bool _isDying = false;

    // Settings for material, rotation and laser
    protected Vector3 _laserOriginalLocalPos;
    protected Transform[] _laserPool;
    protected Quaternion _defaultTurretRotation = Quaternion.identity;
    private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool _materialsSaved = false;
    private bool _lasersInitialized = false;
    private bool _rotationInitialized = false;

    public override void Initialize()
    {
        // Once it has been taken down, the tower must be rebuilt using the same materials
        SaveAndRestoreMaterials();
        _isDying = false;

        base.Initialize();

        _currentReloadTimer = BreakTime;
        _freezeTimer = 0f;
        CurrentTargets.Clear();

        GameSessionController.Instance.RegisterTower(this);

        // If the tower is placed on a path that enemies can use
        if ( EnemyCollection.Instance != null )
        {
            EnemyCollection.Instance.UpdateAllRoutes();
        }

        if (Turret != null)
        {
            if ( !_rotationInitialized )
            {
                _defaultTurretRotation = Turret.localRotation;
                _rotationInitialized = true;
            }
            else
            {
                Turret.localRotation = _defaultTurretRotation;
            }
        }

        // The laser array is installed once, for the entire lifetime of the tower
        if ( Laser != null )
        {
            if ( !_lasersInitialized )
            {
                _laserOriginalLocalPos = Laser.localPosition;
                _laserPool = new Transform[MaxTargets];
                _laserPool[0] = Laser;
                _laserPool[0].gameObject.SetActive(false);

                for ( int i = 1; i < MaxTargets; i++ )
                {
                    GameObject laserCopy = Instantiate(Laser.gameObject, Laser.parent);
                    _laserPool[i] = laserCopy.transform;
                    _laserPool[i].gameObject.SetActive(false);
                }
                _lasersInitialized = true;
            }
            else
            {
                // When reused, all lasers are switched off to allow it to recharge, as shown on the cover
                foreach ( Transform laser in _laserPool )
                {
                    if ( laser != null )
                    {
                        laser.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (BirthSound != null)
        {
            AudioSource.PlayClipAtPoint(BirthSound, transform.position, Volume);
        }
    }

    public override void Recycle()
    {
        ReleaseAllTargets();

        GameSessionController.Instance.UnregisterTower(this);

        if ( GameBoard.Instance != null )
        {
            Tile tile = GameBoard.Instance.GetTileFromPosition(transform.position);

            if ( tile != null && tile.Type == Tile.TileType.Tower )
            {
                tile.isTowerFree = true;
            }
        }

        if ( EnemyCollection.Instance != null )
        {
            EnemyCollection.Instance.UpdateAllRoutes();
        }

        base.Recycle();
    }

    public virtual void Tick(float deltaTime)
    {
        // The tower is frozen and can't do anything
        if ( _freezeTimer > 0f )
        {
            _freezeTimer -= deltaTime;
            return;
        }

        // The tower is waiting for the right moment to fire
        _currentReloadTimer -= deltaTime;

        // Search and fire at the target
        ValidateTargets();
        if ( CurrentTargets.Count < MaxTargets )
        {
            SearchTarget();
        }
        AimAtTarget(deltaTime);
        if ( CurrentTargets.Count > 0 && _currentReloadTimer <= 0f )
        {
            Attack();
            _currentReloadTimer = BreakTime;
        }

        UpdateLaserVisuals();
    }

    /// <summary>
    /// Checks whether current targets are still active. Removes enemies that are dead, disabled or out of range.
    /// </summary>
    private void ValidateTargets()
    {
        // The reverse loop is essential for safely removing elements from the list "on the fly".
        int currentIndex = CurrentTargets.Count - 1;
        while ( currentIndex >= 0 )
        {
            EnemyBase target = CurrentTargets[currentIndex];

            if ( target == null ||
                 !target.gameObject.activeInHierarchy ||
                 target.CurrentHealth <= 0f ||
                 Vector3.Distance(transform.position, target.transform.position) > DetectionRadius )
            {
                if (target != null)
                {
                    target.ClearCrowdControl();
                }
                CurrentTargets.RemoveAt(currentIndex);
            }
            currentIndex--;
        }
    }

    /// <summary>
    /// Searching for new targets.
    /// </summary>
    protected virtual void SearchTarget()
    {
        CurrentTargets.Clear();
        _validEnemies.Clear();

        int hitsEnemiesCount = Physics.OverlapSphereNonAlloc(transform.position, DetectionRadius, _hitsEnemies, EnemyLayerMask);
        int n = 0;
        while ( n < hitsEnemiesCount )
        {
            EnemyBase enemy = _hitsEnemies[n].GetComponentInParent<EnemyBase>();

            if ( enemy != null && enemy.CurrentHealth > 0 && 
                 TargetableCategories.Contains(enemy.Category) )
            {
                // Trojan moment
                if ( !enemy.IsTargetable )
                {
                    n++;
                    continue;
                }

                if ( !_validEnemies.Contains(enemy) )
                {
                    _validEnemies.Add(enemy);
                }
            }
            n++;
        }

        // Sorting enemies by proximity to the server
        if ( _validEnemies.Count > 0 )
        {
            _validEnemies.Sort((a, b) => b.GetPathProgress().CompareTo(a.GetPathProgress()));

            int targetsToTake = Mathf.Min(MaxTargets, _validEnemies.Count);
            for (int i = 0; i < targetsToTake; i++)
            {
                CurrentTargets.Add(_validEnemies[i]);
            }
        }
    }

    private void AimAtTarget(float deltaTime)
    {
        if ( Turret == null )
        {
            return;
        }

        // Rotation lock for multi-target towers
        if ( MaxTargets > 1 )
        {
            Turret.localRotation = Quaternion.Slerp(Turret.localRotation, _defaultTurretRotation, deltaTime * 5f);
            return;
        }

        if ( CurrentTargets.Count > 0 && CurrentTargets[0] != null )
        {
            EnemyBase mainTarget = CurrentTargets[0];
            Collider targetCollider = mainTarget.GetComponentInChildren<Collider>();
            
            Vector3 targetCenter;
            if (targetCollider != null)
            {
                targetCenter = targetCollider.bounds.center;
            }
            else
            {
                targetCenter = mainTarget.transform.position;
            }

            Vector3 directionToTarget = targetCenter - Turret.position;
            if ( directionToTarget != Vector3.zero )
            {
                Quaternion offset = Quaternion.Euler(0, 0f, 0);
                Turret.rotation = Quaternion.LookRotation(directionToTarget) * offset;
            }
        }
        else
        {
            Turret.localRotation = Quaternion.Slerp(Turret.localRotation, _defaultTurretRotation, deltaTime * 5f);
        }
    }

    protected virtual void Attack()
    {
        bool hasAttacked = false;

        foreach ( var target in CurrentTargets )
        {
            if ( target != null && target.CurrentHealth > 0 )
            {
                target.TakeDamage(Strength);
                hasAttacked = true;
            }
        }

        if ( hasAttacked )
        {
            if (ShootSound != null)
            {
                AudioSource.PlayClipAtPoint(ShootSound, transform.position, Volume);
            }
            TakeDamage(Attrition);
        }
    }

    protected virtual void UpdateLaserVisuals()
    {
        if ( _laserPool == null || _laserPool.Length == 0 )
        {
            return;
        }

        Vector3 startPos = Laser.parent.TransformPoint(_laserOriginalLocalPos);

        int n = 0;
        while ( n < MaxTargets )
        {
            Transform laser = _laserPool[n];
            if ( laser == null )
            {
                n++;
                continue;
            }

            if ( n < CurrentTargets.Count && CurrentTargets[n] != null && 
                 CurrentTargets[n].CurrentHealth > 0 )
            {
                laser.gameObject.SetActive(true);

                EnemyBase target = CurrentTargets[n];
                Collider targetCollider = target.GetComponentInChildren<Collider>();
                Vector3 endPos;
                if ( targetCollider != null )
                {
                    endPos = targetCollider.bounds.center;
                }
                else
                {
                    endPos = target.transform.position;
                }

                // Position the laser in the middle between the tower and the target, then scale its length
                laser.position = (startPos + endPos) / 2f;
                laser.LookAt(endPos);

                float distance = Vector3.Distance(startPos, endPos);
                Vector3 currentScale = laser.localScale;
                currentScale.z = distance / laser.parent.lossyScale.z;
                laser.localScale = currentScale;
            }
            else
            {
                laser.gameObject.SetActive(false);
            }

            n++;
        }
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
            if (mat.Key != null)
            {
                mat.Key.materials = mat.Value;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }

    public virtual void TakeDamage(float amount)
    {
        if (!gameObject.activeInHierarchy || CurrentHealth <= 0 || _isDying)
        {
            return;
        }

        CurrentHealth -= amount;
        if ( CurrentHealth <= 0 )
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if ( !gameObject.activeInHierarchy || _isDying )
        {
            return;
        }

        // A flag indicating that the tower is not operational
        _isDying = true;

        Collider col = GetComponent<Collider>();
        if ( col != null )
        {
            col.enabled = false;
        }

        if ( Laser != null )
        {
            Laser.gameObject.SetActive(false);
        }

        if ( DeathSound != null )
        {
            AudioSource.PlayClipAtPoint(DeathSound, transform.position, Volume);
        }

        StartCoroutine(DeathRoutine());
    }

    protected virtual System.Collections.IEnumerator DeathRoutine()
    {
        TriggerDeathVFX();

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        if ( renderers.Length > 0 && _dissolveMaterialTemplate != null )
        {
            foreach ( MeshRenderer rend in renderers )
            {
                Material[] sharedDyingMats = new Material[rend.sharedMaterials.Length];
                for ( int i = 0; i < sharedDyingMats.Length; i++ )
                {
                    sharedDyingMats[i] = _dissolveMaterialTemplate;
                }
                rend.sharedMaterials = sharedDyingMats;
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            float elapsedTimer = 0f;

            while ( elapsedTimer < _deathAnimationTime )
            {
                elapsedTimer += Time.deltaTime;
                float currentProgress = elapsedTimer / _deathAnimationTime;

                foreach ( MeshRenderer rend in renderers )
                {
                    rend.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetFloat(_shaderDissolveParameter, currentProgress);
                    rend.SetPropertyBlock(propertyBlock);
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(_deathAnimationTime);
        }

        Recycle();
    }

    private void TriggerDeathVFX()
    {
        if ( _deathEffectPrefab != null )
        {
            Vector3 spawnPosition = transform.position + (Vector3.up * 0.5f);
            Instantiate(_deathEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // Zero Day Exploit moment
    public virtual void ApplyFreeze(float duration)
    {
        _freezeTimer = Mathf.Max(_freezeTimer, duration);

        if ( _laserPool != null )
        {
            foreach ( Transform laser in _laserPool )
            {
                if (laser != null)
                {
                    laser.gameObject.SetActive(false);
                }
            }
        }

        ReleaseAllTargets();
    }

    private void ReleaseAllTargets()
    {
        foreach (var target in CurrentTargets)
        {
            if (target != null) target.ClearCrowdControl();
        }
        CurrentTargets.Clear();
    }
}