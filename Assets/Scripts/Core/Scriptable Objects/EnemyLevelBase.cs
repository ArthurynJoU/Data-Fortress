using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Cards/Enemies")]
public class EnemyLevelBase: ScriptableObject
{  
    public GameEntity Prefab;

    [Header("Parametric Randomization")]
    [FloatRangeSlider(0.5f, 6f)]
    public FloatRange ScaleRange = new FloatRange(1f);
    [FloatRangeSlider(0.5f, 5f)]
    public FloatRange SpeedRange = new FloatRange(1f);
    [FloatRangeSlider(-0.5f, 0.5f)]
    public FloatRange PathOffSetRange = new FloatRange(0f);
}