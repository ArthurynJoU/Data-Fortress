/// <summary>
/// Isolates the targeted threat in a secure environment, completely halting its progression across the network.
/// </summary>
public class Sandboxing : EffectBase
{
    public override void ApplyEffect(EnemyBase target)
    {
        target.SetSpeed(0f);
    }

    public override void RemoveEffect(EnemyBase target)
    {
        target.SetSpeed(target.GetBaseSpeed());
    }
}