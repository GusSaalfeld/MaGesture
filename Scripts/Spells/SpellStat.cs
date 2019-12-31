using UnityEngine;
using System.Collections;

/// <summary>
/// Helper class representing a spell stat.  Takes values between 0 and 200.
/// Contains various interpolation methods for utility.
/// </summary>
public class SpellStat
{
    private int _value = 100;
    public int Value
    {
        get => _value;
        set => _value = Mathf.Clamp(value, 0, 200);
    }

    /// <summary>
    /// REQUIRES minMultiplier < 1 and maxMultiplier > 1.  Behavior is undefined
    /// if this isn't the case.
    /// 
    /// If Value == 100, returns baseValue.
    /// If Value < 100, lerps between minMultiplier and 1.
    /// If Value > 100, lerps between 1 and maxMultiplier.
    /// </summary>
    public float TwoSidedLerp(float baseValue, float minMultiplier, float maxMultiplier)
    {
        if (Value == 100) return baseValue;
        if (Value < 100) return baseValue * Mathf.Lerp(minMultiplier, 1, 0.01f * Value / 2);
        if (Value > 100) return baseValue * Mathf.Lerp(1, maxMultiplier, 0.01f * (Value - 100));

        throw new System.Exception("Illegal program state");
    }
}
