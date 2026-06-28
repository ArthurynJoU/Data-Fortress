using UnityEngine;

/// <summary>
/// Accumulates anger as it travels. Once fully enraged, it halts its movement and continuously drains the health of all nearby defense modules until its anger depletes.
/// </summary>
public class Ransomware: EnemyBase
{
    [Header("Mechanics")]
    [SerializeField]
    private float _radius = 2.0f;
    [SerializeField]
    private LayerMask _towerLayerMask;

    [Header("Anger Settings")]
    [SerializeField]
    private float _angerIncreaseRate = 10f;
    [SerializeField]
    private float _angerDecreaseRate = 20f;

    [Header("Attack Visuals")]
    [SerializeField] 
    private ParticleSystem _waveEffectPrefab;
    [SerializeField] 
    private ParticleSystem _towerHitEffectPrefab;

    private ParticleSystem _activeWave;

    private float _angerLevel = 0f; // from 0 to 100
    private bool _isEnraged = false;

    public override void Initialize()
    {
        base.Initialize();
        _angerLevel = 0f;
        _isEnraged = false;
    }

    // to against allocations
    private Collider[] _hitTowers = new Collider[5];
    private void ActivateSpecial(float deltaTime)
    {
        int hitTowersCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, _hitTowers, _towerLayerMask);
        
        for ( int i = 0; i < hitTowersCount; i++ )
        {
            if ( _hitTowers[i].TryGetComponent(out IDamageable tower) )
            {
                tower.TakeDamage(Strength * deltaTime);
                if ( AttackSound != null )
                {
                    AudioSource.PlayClipAtPoint(AttackSound, transform.position, Volume);
                }
            }
        }
    }

    public override void Tick(float deltaTime)
    {
        if ( !_isEnraged )
        {
            _angerLevel += deltaTime * _angerIncreaseRate;
            base.Tick(deltaTime);

            if ( _angerLevel >= 100f )
            {
                _isEnraged = true;
                _angerLevel = 100f;
                Speed = 0f; 

                if ( _waveEffectPrefab != null )
                {
                    _activeWave = Instantiate(_waveEffectPrefab, transform.position, Quaternion.identity);
                    _activeWave.transform.SetParent(this.transform);
                }

                int hitTowersCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, _hitTowers, _towerLayerMask);
                for ( int i = 0; i < hitTowersCount; i++ )
                {
                    if (_towerHitEffectPrefab != null)
                    { 
                        Instantiate(_towerHitEffectPrefab, _hitTowers[i].transform.position, Quaternion.identity); 
                    }
                }
            }
        }
        else
        {
            ActivateSpecial(deltaTime);

            _angerLevel -= deltaTime * _angerDecreaseRate;
            if ( _angerLevel <= 0f )
            {
                _isEnraged = false;
                _angerLevel = 0f;
                Speed = _baseSpeed;

                if ( _activeWave != null )
                {
                    Destroy(_activeWave.gameObject);
                }
            }
        }
    }

    public override void Recycle()
    {
        if ( _activeWave != null )
        {
            Destroy(_activeWave.gameObject);
        }

        base.Recycle();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}