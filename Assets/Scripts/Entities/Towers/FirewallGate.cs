/// <summary>
/// Upon attacking, it applies a "fear" effect, forcing the targeted entity to temporarily retreat along its path.
/// </summary>
public class FirewallGate : TowerBase
{
    protected override void Attack()
    {
        base.Attack();

        foreach ( var target in CurrentTargets )
        {
            if ( target != null )
            {
                target.ApplyFear(BreakTime + 0.1f);
            }
        }
    }
}