/// <summary>
/// Grants the object the ability to imbue other items with special properties.
/// </summary>
public interface IEffector
{
    void ApplyEffect(EnemyBase target);

    void RemoveEffect(EnemyBase target);
}