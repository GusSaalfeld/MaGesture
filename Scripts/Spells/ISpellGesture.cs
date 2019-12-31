using UnityEngine;
using System.Collections;

public interface ISpellGesture
{
    SpellElement? Element { get; }
    SpellTier? Tier { get; }
    SpellAttribute? Attribute { get; }
    bool IsCast { get; }
    bool IsCancel { get; }

    Transform AttachmentPoint { get; }

    /// <summary>
    /// The ray along which to cast the spell
    /// </summary>
    Ray Aim { get; }

    /// <summary>
    /// A value within [0, 100] representing the power of this gesture.
    /// 
    /// This is used by SpellCaster when adjusting SpellAttributes.
    /// 
    /// It is never used if ToAttribute returns null.
    /// </summary>
    int AttributePower { get; }
}
