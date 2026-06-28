/// <summary>
/// Allows the object to react to external status modifiers (invisibility for Trojan).
/// </summary>
public interface IEffectible
{
    void ApplyEffect(EffectBase trigger);
}