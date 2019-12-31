using UnityEngine;
using System.Collections;

public interface ISpell
{
    string Name { get; }

    /// <summary>
    /// Element of the spell.
    /// </summary>
    SpellElement Element { get; }

    /// <summary>
    /// Tier of the spell.
    /// </summary>
    SpellTier Tier { get; }

    /// <summary>
    /// Whether this spell is channeled.
    /// A channeled spell, once Activate is called, continues casting until 
    /// Deactivate is called.
    /// </summary>
    bool IsChanneled { get; }

    /// <summary>
    /// Whether the spell is active.
    /// 
    /// A spell is active if it has been successfully activated and has not
    /// ended.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Adjusting a spell with a SpellAttribute changes its behaviors.
    /// Different SpellAttributes have different effects on different spells.
    /// Power is a value within [0, 100] that influences the magnitude of the
    /// effect.
    /// </summary>
    /// <param name="attribute">The attribute to adjust</param>
    /// <param name="power">Value within [0, 100] that influences the change in the spell</param>
    /// <returns>Whether the specified attribute could be adjusted</returns>
    bool AdjustAttribute(SpellAttribute attribute, int power);

    /// <summary>
    /// Adjust the aim of the spell based on the specified gesture.
    /// </summary>
    /// <param name="gesture">The gesture to determine the aim from.</param>
    void AdjustAim(ISpellGesture gesture);

    /// <summary>
    /// Attempts to activate the spell.  Returns false if activation failed.
    /// </summary>
    /// <returns>Whether the activation succeeded.</returns>
    bool Activate();

    /// <summary>
    /// Attempts to deactivate the spell.  Returns false if deactivation failed.
    /// </summary>
    /// <returns>Whether the deactivation succeeded.</returns>
    bool Deactivate();
}

public abstract class Spell : MonoBehaviour, ISpell
{
    public abstract string Name { get; }
    public abstract SpellElement Element { get; }
    public abstract SpellTier Tier { get; }
    public abstract bool IsChanneled { get; }
    public abstract bool IsActive { get; }

    public Ray Aim { get; protected set; }

    public bool AdjustAttribute(SpellAttribute attribute, int power)
    {
        if (attribute == SpellAttribute.None)
        {
            throw new System.ArgumentException("Adjusted attribute cannot be 'None'.");
        }

        return AdjustAttributeCore(attribute, power);
    }
    public void AdjustAim(ISpellGesture gesture)
    {
        Aim = gesture.Aim;
    }
    public abstract bool Activate();
    public abstract bool Deactivate();
    protected abstract bool AdjustAttributeCore(SpellAttribute attribute, int power);
}

public enum SpellElement
{
    None,
    Fire,
    Ice,
    Almighty,
}

public enum SpellTier
{
    None,
    Tier1,
    Tier2,
}

public enum SpellAttribute
{
    None,
    Force,
    Skill,
}
