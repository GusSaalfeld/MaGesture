using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gesture : ISpellGesture
{
    #region TODO: ISpellGesture Implementation
    public SpellElement? Element => this.ToSpellElement();
    public SpellTier? Tier => this.ToSpellTier();
    public SpellAttribute? Attribute => this.ToSpellAttribute();
    public bool IsCast => this.IsCast();
    public bool IsCancel => this.IsCancel();

    public Transform AttachmentPoint { get; }

    //TODO: Initialize these based on actual Gesture information
    public Ray Aim { get; set; } = new Ray(Vector3.zero, Vector3.forward);
    public int AttributePower { get; } = 10;
    #endregion

    public readonly GestureType gestureType = GestureType.None;

    public Gesture(GestureType type, Transform attachmentPoint)
    {
        gestureType = type;
        AttachmentPoint = attachmentPoint;
    }
}

public enum GestureType
{
    None,
    ClosedFist,
    OpenPalm,

    //Debug gestures
    DebugAlmighty,
    DebugFire,
    DebugIce,
    DebugTier1,
    DebugTier2,
    DebugForce,
    DebugSkill,
    DebugCast,
    DebugCancel,
    DebugFire1,
    DebugIce1,
    DebugGrav1
}

/// <summary>
/// Utility methods for extracting spell info (attribute, tier, etc.) from
/// Gesture objects
/// </summary>
public static class GestureToSpellInfoExtensions
{
    #region Gesture Mappings
    private static Dictionary<GestureType, SpellElement> gToElement = new Dictionary<GestureType, SpellElement>()
    {
        { GestureType.DebugAlmighty, SpellElement.Almighty },
        { GestureType.DebugFire, SpellElement.Fire },
        { GestureType.DebugIce, SpellElement.Ice },
        { GestureType.DebugFire1, SpellElement.Fire },
        { GestureType.DebugIce1, SpellElement.Ice },
        { GestureType.DebugGrav1, SpellElement.Almighty }
    };

    private static Dictionary<GestureType, SpellTier> gToTier = new Dictionary<GestureType, SpellTier>()
    {
        { GestureType.DebugTier1, SpellTier.Tier1 },
        { GestureType.DebugTier2, SpellTier.Tier2 },
        { GestureType.DebugFire1, SpellTier.Tier1 },
        { GestureType.DebugIce1, SpellTier.Tier1 },
        { GestureType.DebugGrav1, SpellTier.Tier1 }
    };

    private static Dictionary<GestureType, SpellAttribute> gToAttr = new Dictionary<GestureType, SpellAttribute>()
    {
        { GestureType.DebugForce, SpellAttribute.Force },
        { GestureType.DebugSkill, SpellAttribute.Skill },
    };

    private static HashSet<GestureType> gIsCast = new HashSet<GestureType>()
    {
        GestureType.DebugCast,
        GestureType.DebugFire1,
        GestureType.DebugIce1,
        GestureType.DebugGrav1
    };

    private static HashSet<GestureType> gIsCancel = new HashSet<GestureType>()
    {
        GestureType.DebugCancel,
        GestureType.ClosedFist
    };
    #endregion

    #region Extension Methods
    public static SpellElement? ToSpellElement(this Gesture g)
    {
        if (gToElement.TryGetValue(g.gestureType, out SpellElement result)) return result;
        else return null;
    }

    public static SpellTier? ToSpellTier(this Gesture g)
    {
        if (gToTier.TryGetValue(g.gestureType, out SpellTier result)) return result;
        else return null;
    }

    public static SpellAttribute? ToSpellAttribute(this Gesture g)
    {
        if (gToAttr.TryGetValue(g.gestureType, out SpellAttribute result)) return result;
        else return null;
    }

    public static bool IsCast(this Gesture g)
    {
        return gIsCast.Contains(g.gestureType);
    }

    public static bool IsCancel(this Gesture g)
    {
        return gIsCancel.Contains(g.gestureType);
    }
    #endregion
}
