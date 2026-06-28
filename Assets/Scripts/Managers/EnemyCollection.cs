using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Maintains a central registry of all active enemies currently traversing the network.
/// Responsible for dispatching synchronous update commands to the registered elements.
/// </summary>
public class EnemyCollection : MonoBehaviour, ITickable
{
    public static EnemyCollection Instance { get; private set; }

    private List<EnemyBase> _trackedEnemies = new List<EnemyBase>();

    public int ActiveCount => _trackedEnemies.Count;

    private void Awake()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
    }

    public void Register(EnemyBase enemy)
    {
        if ( !_trackedEnemies.Contains(enemy) )
        {
            _trackedEnemies.Add(enemy);
        }
    }

    public void Unregister(EnemyBase enemy)
    {
        if ( _trackedEnemies.Contains(enemy) )
        {
            _trackedEnemies.Remove(enemy);
        }
    }

    public void Tick(float deltaTime)
    {
        int currentIndex = _trackedEnemies.Count - 1;

        while ( currentIndex >= 0 )
        {
            EnemyBase enemy = _trackedEnemies[currentIndex];

            if ( enemy != null && enemy.GetHealth() > 0 )
            {
                enemy.Tick(deltaTime);
            }

            currentIndex--;
        }
    }

    public void UpdateAllRoutes()
    {
        int currentIndex = _trackedEnemies.Count - 1;

        while ( currentIndex >= 0 )
        {
            EnemyBase enemy = _trackedEnemies[currentIndex];

            if ( enemy != null && enemy.gameObject != null )
            {
                enemy.BuildNewRoute();
            }
            else
            {
                _trackedEnemies.RemoveAt(currentIndex);
            }

            currentIndex--;
        }
    }
}