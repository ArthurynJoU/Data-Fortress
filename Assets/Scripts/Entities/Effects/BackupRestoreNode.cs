using UnityEngine;

/// <summary>
/// Intercepts and reverses malicious payload logic, temporarily inverting the damage output of the affected entity.
/// </summary>
public class BackupRestoreNode : EffectBase
{
    public override void ApplyEffect(EnemyBase target)
    {
        Debug.Log($"[BackupRestoreNode] {target.name}'s damage has inverted!");
        target.SetStrength(-1 * UnityEngine.Mathf.Abs(target.GetStrength()));
    }

    public override void RemoveEffect(EnemyBase target)
    {
        Debug.Log($"[BackupRestoreNode] {target.name}'s damage has returned!");
        target.SetStrength(UnityEngine.Mathf.Abs(target.GetStrength()));
    }
}