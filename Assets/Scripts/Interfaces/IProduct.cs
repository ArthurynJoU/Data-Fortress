using UnityEngine;

/// <summary>
/// Basic contract for working with Object Pooling via GameContentFactory.
/// Ensures that the factory can uniformly manage the life cycle of both towers and enemies.
/// </summary>
public interface IProduct
{
    GameObject gameObject { get; }
    /// <summary>
    /// Called by the factory immediately after an object is retrieved from the pool. 
    /// Used to reset the state (restore HP, reset path progress) before activation on stage.
    /// </summary>
    void Initialize();
    /// <summary>
    /// Prepares the object for return to the inactive objects warehouse.
    /// Required: clear all active effects and unsubscribe from events to avoid memory leaks.
    /// </summary>
    void Recycle();
}