using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gradually degrades the integrity of the target over time, simulating a continuous toxic injection or memory leak.
/// </summary>
public class BehavioralAnalysis : EffectBase
{
    [Header("Poison Settings")]
    [Tooltip("How many seconds will the enemy take damage?")]
    public float PoisonDuration = 5f;

    [Header("Poison Visuals")]
    [SerializeField] private GameObject _purpleAuraPrefab;

    private Dictionary<EnemyBase, float> _poisonTimers = new Dictionary<EnemyBase, float>();
    private Dictionary<EnemyBase, GameObject> _activeAuras = new Dictionary<EnemyBase, GameObject>();

    public override void ApplyEffect(EnemyBase target)
    {
        _poisonTimers[target] = PoisonDuration;

        if ( _purpleAuraPrefab != null && !_activeAuras.ContainsKey(target) )
        {
            GameObject aura = Instantiate(_purpleAuraPrefab, target.transform.position, Quaternion.identity);
            aura.transform.SetParent(target.transform);
            _activeAuras[target] = aura;
        }
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        List<EnemyBase> curedEntities = new List<EnemyBase>();

        List<EnemyBase> activeEnemies = new List<EnemyBase>(_poisonTimers.Keys);

        foreach ( EnemyBase enemy in activeEnemies )
        {
            float timeLeft = _poisonTimers[enemy];

            if ( enemy != null && enemy.gameObject.activeInHierarchy && enemy.GetHealth() > 0 )
            {
                enemy.TakeDamage(Strength * deltaTime);
                _poisonTimers[enemy] = timeLeft - deltaTime;

                if ( _poisonTimers[enemy] <= 0 )
                {
                    curedEntities.Add(enemy);
                }
            }
            else
            {
                curedEntities.Add(enemy);
            }
        }

        foreach ( EnemyBase enemy in curedEntities )
        {
            RemoveEffect(enemy);
        }
    }

    public override void RemoveEffect(EnemyBase target)
    {
        if ( _poisonTimers.ContainsKey(target) )
        {
            if ( _activeAuras.ContainsKey(target) )
            {
                if ( _activeAuras[target] != null )
                {
                    Destroy(_activeAuras[target]);
                }
                _activeAuras.Remove(target);
            }
            _poisonTimers.Remove(target);
        }
    }
}