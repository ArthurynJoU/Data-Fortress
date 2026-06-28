using UnityEngine;

/// <summary>
/// A self-replicating threat that, upon taking fatal damage, splits into multiple smaller offspring that inherit its path and continue the assault.
/// </summary>
public class Worm : EnemyBase
{
    [Header("Mechanics")]
    [SerializeField]
    private GameContentFactory _factory;
    [SerializeField]
    private MiniWorm _smallWormPrefab;
    [SerializeField]
    private int _splitCount = 2;
    [SerializeField]
    private float _spawnSpread = 0.5f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem _splitEffectPrefab;
    public bool CanSplit { get; protected set; } = true;

    public override void Initialize()
    {
        base.Initialize();
        CanSplit = true;
    }

    public override void Die()
    {
        try
        {
            if ( CanSplit && _smallWormPrefab != null && _factory != null )
            {
                SpawnMiniWorms();
            }
        }
        finally
        {
            base.Die();
        }
    }

    private void SpawnMiniWorms()
    {
        if ( _splitEffectPrefab != null )
        {
            Instantiate(_splitEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        for ( int i = 0; i < _splitCount; i++ )
        {
            MiniWorm miniWorm = _factory.GetProduct(_smallWormPrefab) as MiniWorm;

            if ( miniWorm == null )
            {
                Debug.LogWarning("The factory couldn't produce any MiniWorms! Is the pool limit reached?");
                continue;
            }

            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnSpread, _spawnSpread),
                0f,
                Random.Range(-_spawnSpread, _spawnSpread)
            );

            miniWorm.transform.position = this.transform.position + randomOffset;
            miniWorm.CopyRouteFrom(this);

            miniWorm.MaxHealth = this.MaxHealth / 2f;
            miniWorm.SetStrength(this.Strength / 2f);
            miniWorm.EnergyForDeath = 5f;

            if ( AttackSound != null )
            {
                AudioSource.PlayClipAtPoint(AttackSound, transform.position, Volume);
            }
        }
    }
}