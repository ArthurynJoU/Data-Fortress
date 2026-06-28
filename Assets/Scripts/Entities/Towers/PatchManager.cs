/// <summary>
/// A control-oriented tower that applies a "stun" effect to its targets, 
/// completely halting their movement and rendering them temporarily inactive.
/// </summary>
public class PatchManager: TowerBase
{
    protected override void Attack()
    {
        base.Attack();

        foreach ( var target in CurrentTargets )
        {
            if ( target != null )
            {
                target.ApplyStun(BreakTime + 0.1f);
            }
        }
    }
}