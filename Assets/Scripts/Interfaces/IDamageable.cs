/// <summary>
/// A contract for any game entities that have health points and can be destroyed.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
    void Die();
}