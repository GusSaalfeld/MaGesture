using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IGravityBallTargetable
{
    /// <summary>
    /// Whether the object is currently targeted by a GravityBall.
    /// </summary>
    bool IsBeingTargeted { get; }

    /// <summary>
    /// Called when initially targeted.
    /// </summary>
    void OnBeginPull();

    /// <summary>
    /// Called every fixed update while being targeted.
    /// </summary>
    /// <param name="location">The position to be pulled to</param>
    /// <param name="pullSpeed">The strength of the pull</param>
    void Pull(Vector3 location, float pullSpeed);

    /// <summary>
    /// Called when no longer being targeted.
    /// </summary>
    void OnEndPull();
}

/// <summary>
/// <para>
/// GravityBall is the <see cref="SpellTier.Tier1"/>
/// <see cref="SpellElement.Almighty"/> spell.  It launches a bouncing 
/// projectile that detonates after a set time, causing nearby enemies to be
/// dragged towards it.
/// </para>
/// </summary>
public class GravityBall : Spell
{
    #region Inspector
#pragma warning disable 0649
    [SerializeField] private float baseLaunchSpeed;
    [SerializeField] private float detonationDelay;
    [SerializeField] private float baseDuration;
    [SerializeField] private float basePullRadius;
    [SerializeField] private float basePullSpeed;
    [SerializeField] private GameObject activationVfx;
#pragma warning restore 0649
    #endregion

    private AudioSource audioSource;

    #region Spell Implementation
    public override string Name => "Gravity Ball";
    public override SpellElement Element => SpellElement.Almighty;
    public override SpellTier Tier => SpellTier.Tier1;
    public override bool IsChanneled => false;
    public override bool IsActive => isActive;

    private bool isActive = false;

    public override bool Activate()
    {
        //Can only activate once
        if (isActive)
        {
            return false;
        }
        isActive = true;
        GameManager.S.Audio.CastGrav(audioSource);

        transform.SetParent(null, true);
        rigidbody.AddForce(Aim.direction * baseLaunchSpeed, ForceMode.VelocityChange);
        StartCoroutine(PullRoutine(detonationDelay));
        return true;
    }

    public override bool Deactivate()
    {
        if (!isActive)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    protected override bool AdjustAttributeCore(SpellAttribute attribute, int power)
    {
        Debug.LogWarning("TODO: GravityBall spell does not have adjustable attributes.");
        return false;
    }
    #endregion

    #region Gravity Ball Core
    private readonly ISet<IGravityBallTargetable> targetedByOther = new HashSet<IGravityBallTargetable>();
    private readonly ISet<IGravityBallTargetable> targets = new HashSet<IGravityBallTargetable>();
    private SphereCollider gravityHitbox;
    private new SphereCollider collider;
    private new Rigidbody rigidbody;

    private IEnumerator PullRoutine(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        GameManager.S.Audio.GravHits(audioSource);
        activationVfx.SetActive(true);
        activationVfx.transform.localScale = Vector3.one * 2 * basePullRadius;
        rigidbody.isKinematic = true;
        collider.enabled = false;

        //Create gravity hitbox if necessary
        if (gravityHitbox == null)
        {
            gravityHitbox = gameObject.AddComponent<SphereCollider>();
        }
        gravityHitbox.enabled = true;
        gravityHitbox.radius = basePullRadius;
        gravityHitbox.isTrigger = true;

        //OnTriggerEnter won't detect colliders already within gravityHitbox
        //So, get them via Physics.OverlapSphere
        Collider[] initialTargets = Physics.OverlapSphere(gravityHitbox.bounds.center, gravityHitbox.bounds.extents.x);
        foreach (Collider initialTarget in initialTargets)
        {
            IGravityBallTargetable t = initialTarget.GetComponent<IGravityBallTargetable>();
            if (t == null)
            {
                continue;
            }
            AddTarget(t);
        }

        //Pull targets in over duration
        for (float timePassed = 0; timePassed < baseDuration; timePassed += Time.fixedDeltaTime)
        {
            UpdateTargetedByOther();

            //Remove any destroyed targets
            targets.ExceptWith(targets.Where(t => t.Equals(null)).ToList());

            //Pull targets to center
            foreach (IGravityBallTargetable target in targets)
            {
                target.Pull(gravityHitbox.bounds.center, basePullSpeed);
            }
            yield return new WaitForFixedUpdate();
        }
        isActive = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// <para>
    /// Targets that were already targeted get stored in targetedByOther.
    /// </para>
    /// <para>
    /// This method starts targeting these "pending targets" if they are no
    /// no longer being targeted by another GravityBall.
    /// </para>
    /// </summary>
    private void UpdateTargetedByOther()
    {
        foreach (IGravityBallTargetable target in targetedByOther)
        {
            AddTarget(target);
        }
        targetedByOther.ExceptWith(targets);
    }

    private void AddTarget(IGravityBallTargetable target)
    {
        if (!target.IsBeingTargeted)
        {
            targets.Add(target);
            target.OnBeginPull();
        }
        else
        {
            targetedByOther.Add(target);
        }
    }

    private void RemoveTarget(IGravityBallTargetable target)
    {
        bool wasTargeted = targets.Remove(target);
        if (wasTargeted)
        {
            target.OnEndPull();
        }
        targetedByOther.Remove(target);
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
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<SphereCollider>();
        activationVfx.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        IGravityBallTargetable target = other.GetComponent<IGravityBallTargetable>();
        if (target == null)
        {
            return;
        }
        AddTarget(target);
    }

    private void OnTriggerExit(Collider other)
    {
        IGravityBallTargetable target = other.GetComponent<IGravityBallTargetable>();
        if (target == null)
        {
            return;
        }
        RemoveTarget(target);
    }

    private void OnDestroy()
    {
        while (targets.Count > 0)
        {
            RemoveTarget(targets.First());
        }
        targetedByOther.Clear();
    }

    private void OnValidate()
    {
        baseLaunchSpeed = Mathf.Max(0, baseLaunchSpeed);
        basePullRadius = Mathf.Max(0, basePullRadius);
        baseDuration = Mathf.Max(0, baseDuration);
        detonationDelay = Mathf.Max(0, detonationDelay);
        basePullSpeed = Mathf.Max(0, basePullSpeed);

        if (gravityHitbox != null)
        {
            gravityHitbox.radius = basePullRadius;
        }
    }
    #endregion
}
