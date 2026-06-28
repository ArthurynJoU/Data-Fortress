using UnityEngine;

/// <summary>
/// A highly destructive programme. Upon successfully breaching the server, it bypasses local combat and inflicts simultaneous global damage to all active towers on the board.
/// </summary>
public class SupplyChainAttack : EnemyBase
{
    [Header("Attack Visuals")]
    [SerializeField]
    private ParticleSystem _towerHitEffectPrefab;

    protected override void ReachDestination()
    {
        TowerBase[] Towers = FindObjectsByType<TowerBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach ( var tower in Towers )
        {
            if ( tower.TryGetComponent(out IDamageable t) )
            {
                t.TakeDamage(Strength);

                if ( _towerHitEffectPrefab != null )
                {
                    Instantiate(_towerHitEffectPrefab, tower.transform.position + Vector3.up * 1.5f, Quaternion.identity);
                }
            }
        }
         
        base.ReachDestination();
    }
}