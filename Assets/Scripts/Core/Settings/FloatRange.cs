using UnityEngine;
using System;
/// <summary>
/// Responsible for digital indicators of speed, deviation from the route and size. 
/// Also responsible for randomisation.
/// </summary>
[Serializable]
public class FloatRange
{
    [SerializeField]
    private float _min, _max;

    public float Min
    {
        get
        {
            return _min;
        }
    }

    public float Max
    {
        get
        {
            return _max;
        }
    }

    public float RandomValueRange
    {
        get
        {
            return UnityEngine.Random.Range(_min, _max);
        }
    }

    public FloatRange(float value)
    {
        _min = _max = value;

    }
}

public class FloatRangeSliderAttribute : PropertyAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public FloatRangeSliderAttribute(float min, float max)
    {
        Min = min;

        if (max < min)
        {
            Max = min;
        }
        else
        {
            Max = max;
        }
    }
}
