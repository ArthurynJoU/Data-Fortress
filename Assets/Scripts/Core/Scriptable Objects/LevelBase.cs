using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Levels")]


public class LevelBase: ScriptableObject
{
    public string Name;
    public Vector2Int GridSize = new Vector2Int(10, 10);

    [Header("Visuals")]
    public GameObject EnvironmentPrefab;

    public List<WaveDefinition> Waves;

    [Header("Deck Settings")]
    [SerializeField]
    private int _handSize = 4;
    [SerializeField]
    public List<HeroCardBase> DeckList = new List<HeroCardBase>();

    public int GetHandSize()
    {
        return _handSize;
    }
}

[Serializable]
public struct WaveStep
{
    public EnemyLevelBase Config;
    public int Count;
    public float Interval;
}

[Serializable]
public class WaveDefinition
{
    public string Name;
    public float DelayBeforeWave;
    public List<WaveStep> Steps;
}