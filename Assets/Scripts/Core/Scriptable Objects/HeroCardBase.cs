using UnityEngine;

[CreateAssetMenu(fileName = "NewHero", menuName = "Cards/Heroes")]
public class HeroCardBase: ScriptableObject
{
    public int Cost;
    public GameEntity PrefabToSpawn;
    public Sprite CardArt;
}