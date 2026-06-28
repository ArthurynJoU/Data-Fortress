using UnityEngine;

/// <summary>
/// An enemy that accelerates as it sustains damage, reaching maximum speed when its health is critically low.
/// </summary>
public class DDoS: EnemyBase
{
    [Header("Mechanics")]
    [SerializeField]
    private float coefficient = 1.0f;
    public override void Tick(float deltaTime)
    {
        if ( CurrentHealth > 0 )
        {
            float healthPercent = CurrentHealth / MaxHealth;
            float increaseSpeed = coefficient + (1f - healthPercent);
            Speed = _baseSpeed * increaseSpeed;
        }
        base.Tick(deltaTime);
    }
}