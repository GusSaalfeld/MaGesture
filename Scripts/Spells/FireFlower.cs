using UnityEngine;
using System.Collections;

/// <summary>
/// <para>
/// Fire Flower is the <see cref="SpellTier.Tier1"/>
/// <see cref="SpellElement.Fire"/> spell.  It repeatedly launches exploding
/// fireballs.
/// </para>
/// <para>
/// Adjusting its <see cref="SpellAttribute.Force"/> influences the maximum
/// number of chains and also the chain radius (positive correlation).
/// </para>
/// <para>
/// Adjusting its <see cref="SpellAttribute.Skill"/> influences the frequency 
/// at which fireballs are launched (positive correlation).
/// </para>
/// </summary>
public class FireFlower : Spell
{
    #region Inspector
#pragma warning disable 0649
    [SerializeField] private Fireball fireballPrefab;

    [SerializeField,
     Tooltip("Max chain count is [0.5, 2] times the base value.")]
    private int baseChainCount = 1;

    [SerializeField,
     Tooltip("Firing cooldown (seconds) is [0.5, 1.5] times the base value.")]
    private float baseCooldown = 1f;
#pragma warning restore 0649
    #endregion

    private AudioSource audioSource;

    #region Spell Implementation
    public override string Name => "Fire Flower";
    public override SpellElement Element => SpellElement.Fire;
    public override SpellTier Tier => SpellTier.Tier1;
    public override bool IsChanneled => true;
    public override bool IsActive => activatedRoutine != null;

    public override bool Activate()
    {
        if (activatedRoutine != null)
        {
            return false;
        }
        GameManager.S.Audio.FireCharged(audioSource);
        activatedRoutine = StartCoroutine(ActivatedRoutine());
        return true;
    }

    public override bool Deactivate()
    {
        if (activatedRoutine != null)
        {
            StopCoroutine(activatedRoutine);
        }
        GameManager.S.Audio.DismissFire(audioSource);
        Destroy(gameObject);
        return true;
    }

    protected override bool AdjustAttributeCore(SpellAttribute attribute, int power)
    {
        //Adjust SpellStats based on attribute and power.
        switch (attribute)
        {
            case SpellAttribute.Force:
                chain.Value += power;
                break;
            case SpellAttribute.Skill:
                cooldown.Value -= power;
                break;
            default:
                return false;
        }
        return true;
    }
    #endregion

    #region FireFlower Core
    /// <summary>
    /// Influences the maximum number of chains and the time between each
    /// chain.  Falls within [0, 200].
    /// </summary>
    public int Chain => chain.Value;
    private readonly SpellStat chain = new SpellStat();

    /// <summary>
    /// Influences the cooldown time between fireballs.  Falls within [0, 200].
    /// </summary>
    public int Cooldown => cooldown.Value;
    private readonly SpellStat cooldown = new SpellStat();

    public int MaxChainCount =>
        Mathf.CeilToInt(chain.TwoSidedLerp(baseChainCount, 0.5f, 2f));

    public float FireCooldown =>
        Mathf.Max(0.05f, cooldown.TwoSidedLerp(baseCooldown, 0.5f, 1.5f));

    private Coroutine activatedRoutine;
    private IEnumerator ActivatedRoutine()
    {
        int numCasts = MaxChainCount;
        for (int i = 0; i < numCasts; i++)
        {
            Fireball f = Instantiate(fireballPrefab);

            f.FireFrom(Aim);

            if (i == numCasts - 1) Deactivate();

            float cooldown = FireCooldown;
            yield return new WaitForSeconds(cooldown);
        }
        Deactivate();
    }
    #endregion

    #region Unity Events
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("Couldn't find AudioSource component.");
        }
        if (fireballPrefab == null)
        {
            Debug.LogError("Fireball prefab reference not set in " +
                "FireFlower.  Please set it in the inspector.");
        }
    }

    private void OnValidate()
    {
        baseChainCount = Mathf.Max(1, baseChainCount);
        baseCooldown = Mathf.Max(0.05f, baseCooldown);
    }
    #endregion
}
