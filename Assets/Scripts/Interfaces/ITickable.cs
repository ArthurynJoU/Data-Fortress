/// <summary>
/// Interface for entities and managers requiring synchronous updates.
/// </summary>
public interface ITickable
{
    void Tick(float deltaTime);
}