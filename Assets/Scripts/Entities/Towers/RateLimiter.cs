using UnityEngine;
/// <summary>
/// A traffic-throttling module that applies a lingering slow effect to targeted entities, 
/// drastically reducing their movement speed to minimise immediate threats.
/// </summary>
public class RateLimiter: TowerBase
{
    [Header("Mechanics")]
    [SerializeField] 
    private float _slowFactor = 0.01f;

    protected override void Attack()
    {
        base.Attack();

        foreach ( var target in CurrentTargets )
        {
            if ( target != null )
            {
                target.ApplySlow(BreakTime + 0.1f, _slowFactor);
            }
        }
    }
}