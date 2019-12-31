using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An ISpellCaster receives gestures via PublishGesture and transitions
/// through SpellCastingStates in response.
/// </summary>
public interface ISpellCaster
{
    /// <summary>
    /// The current state of the ISpellCaster.
    /// </summary>
    SpellCastingState State { get; }

    /// <summary>
    /// The current spell (null if State is None/Select)
    /// </summary>
    ISpell CurrentSpell { get; }

    /// <summary>
    /// Publish an ISpellGesture to the ISpellCaster.
    /// </summary>
    /// <param name="gesture"></param>
    void PublishGesture(ISpellGesture gesture);
}

/// <summary>
/// Instantiates and controls spells in the game by responding to Gestures.
/// </summary>
public class SpellCaster : MonoBehaviour, ISpellCaster
{
#pragma warning disable 0649
    [SerializeField] private Spell[] spellPrefabs;
#pragma warning restore 0649

    //Exposed for debugging (intentionally not part of ISpellCaster interface)
    public SpellElement SpellElement { get; private set; }
    public SpellTier SpellTier { get; private set; }

    #region ISpellCaster Implementation
    public ISpell CurrentSpell { get; private set; }
    public SpellCastingState State { get; private set; } = SpellCastingState.Select;

    public void PublishGesture(ISpellGesture gesture)
    {
        bool spellWasDestroyed = CurrentSpell != null && CurrentSpell.Equals(null);

        //Process cancel gesture.
        //A spell can be canceled at any time.
        if (gesture.IsCancel || spellWasDestroyed)
        {
            State = SpellCastingState.Cancel;
        }

        switch (State)
        {
            case SpellCastingState.None:
                break;
            case SpellCastingState.Select:
                SelectSpell(gesture);
                break;
            case SpellCastingState.Adjust:
                AdjustSpell(gesture);
                break;
            case SpellCastingState.Cast:
                CastSpell(gesture);
                break;
            case SpellCastingState.Cancel:
                CancelSpell();
                break;
            default:
                throw new System.InvalidOperationException("Illegal enum value.");
        }
    }
    #endregion

    #region State Behaviors
    private void SelectSpell(ISpellGesture gesture)
    {
        //Extract element and tier from the gesture
        SpellElement? elem = gesture.Element;
        SpellTier? tier = gesture.Tier;

        //If elem/tier exist, store their value
        SpellElement = elem.GetValueOrDefault(SpellElement);
        SpellTier = tier.GetValueOrDefault(SpellTier);

        //If an element and tier have been selected, transition to the Adjust state
        if (SpellElement != SpellElement.None && SpellTier != SpellTier.None)
        {
            foreach (Spell spellPrefab in spellPrefabs)
            {
                if (spellPrefab.Element == SpellElement && spellPrefab.Tier == SpellTier)
                {
                    CurrentSpell = Instantiate(spellPrefab, gesture.AttachmentPoint);
                    State = SpellCastingState.Adjust;
                    AdjustSpell(gesture);
                    break;
                }
            }

            if (CurrentSpell == null)
            {
                Debug.LogWarningFormat(
                    "No spell prefab found for (Element, Tier) = ({0}, {1})",
                    SpellElement.ToString(), SpellTier.ToString()
                );

                SpellElement = SpellElement.None;
                SpellTier = SpellTier.None;
            }
        }
    }

    private void AdjustSpell(ISpellGesture gesture)
    {
        //Adjust the spell (attribute and aim)
        SpellAttribute? attr = gesture.Attribute;
        if (attr.HasValue)
        {
            bool success = CurrentSpell.AdjustAttribute(attr.Value, gesture.AttributePower);
            if (!success)
            {
                Debug.LogWarning("TODO: 'Failed to adjust attribute' SFX/VFX");
            }
        }
        CurrentSpell.AdjustAim(gesture);

        //Start casting the spell if appropriate
        if (State == SpellCastingState.Adjust && gesture.IsCast)
        {
            State = SpellCastingState.Cast;
            CastSpell(gesture);
        }
    }

    private void CastSpell(ISpellGesture gesture)
    {
        //If attempting to cast and CurrentSpell isn't activated, activate
        //Otherwise, adjust
        if (gesture.IsCast && !CurrentSpell.IsActive)
        {
            bool success = CurrentSpell.Activate();

            //Failed to activate spell, so return to Adjust state.
            if (!success)
            {
                State = SpellCastingState.Adjust;
                return;
            }

            //If the spell isn't channeled, prepare to cast a new spell
            if (success && !CurrentSpell.IsChanneled)
            {
                CurrentSpell = null;
                SpellElement = SpellElement.None;
                SpellTier = SpellTier.None;
                State = SpellCastingState.Select;
            }
            else
            {
                StartCoroutine(MonitorChanneledSpell());
            }
        }
        else
        {
            AdjustSpell(gesture);
        }
    }

    private void CancelSpell()
    {
        if (CurrentSpell == null || CurrentSpell.Equals(null))
        {
            CurrentSpell = null;
            SpellElement = SpellElement.None;
            SpellTier = SpellTier.None;
            State = SpellCastingState.Select;
        }
        else
        {
            bool success = CurrentSpell.Deactivate();
            if (success)
            {
                CurrentSpell = null;
                SpellElement = SpellElement.None;
                SpellTier = SpellTier.None;
                State = SpellCastingState.Select;
            }
        }
    }

    private IEnumerator MonitorChanneledSpell()
    {
        yield return new WaitUntil(() => CurrentSpell == null || CurrentSpell.Equals(null));
        CancelSpell();
    }
    #endregion

    #region Unity Events
    [HideInInspector, SerializeField] private Spell[] savedSpellPrefabs;
    private void OnValidate()
    {
        //Prevent adding spells with the same Element and Tier
        bool spellPrefabsLegalFlag = true;
        for (int i = 0; i < spellPrefabs.Length; i++)
        {
            for (int j = i+1; j < spellPrefabs.Length; j++)
            {
                Spell s1 = spellPrefabs[i];
                Spell s2 = spellPrefabs[j];
                if (s1.Element == s2.Element && s1.Tier == s2.Tier)
                {
                    spellPrefabs = savedSpellPrefabs;
                    Debug.LogError("Cannot add spells with identical Element and Tier.");
                    spellPrefabsLegalFlag = false;
                }
            }
        }
        if (spellPrefabsLegalFlag)
        {
            savedSpellPrefabs = spellPrefabs;
        }
    }
    #endregion
}

/// <summary>
/// Casting a spell is a multi-step process:
/// <list type="bullet">
/// <item>Select what kind of spell to cast</item>
/// <item>Adjust the spell (charging, aiming, etc.)</item>
/// <item>Activate the spell (may continue adjusting while activated)</item>
/// <item>End the spell (can be done at any time)</item>
/// </list>
/// </summary>
public enum SpellCastingState
{
    None,
    Select,
    Adjust,
    Cast,
    Cancel,
}
