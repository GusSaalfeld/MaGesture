using UnityEngine;
using System.Collections;

/// <summary>
/// <para>
/// Ice Mist is the <see cref="SpellTier.Tier1"/> 
/// <see cref="SpellElement.Ice"/> spell.  It launches a cloud of freezing mist
/// that moves a distance in a direction along the xz-plane and then lingers.
/// The freezing mist continuously deals damage to enemies it is touching.
/// </para>
/// <para>
/// Adjusting its <see cref="SpellAttribute.Force"/> influences the size and 
/// linger time of the mist (positive correlation).
/// </para>
/// <para>
/// Has an interaction with <see cref="GravityBall"/>.  If, while traveling,
/// the ice mist comes within range of a <see cref="GravityBall"/>, its 
/// target will change accordingly.
/// </para>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class IceMist : Spell, IGravityBallTargetable
{
    #region Inspector
#pragma warning disable 0649
    [Header("Size")]
    [SerializeField] private float heldScale;
    [SerializeField] private float releaseScale;
    [SerializeField] private float growSpeed;

    [Header("Stats")]

    [SerializeField] private float speed = 1f;

    [SerializeField] private float baseLingerScale = 1f;

    [SerializeField,
     Tooltip("How long the mist lingers after it stops moving (seconds)")]
    private float baseLinger = 1f;

    [SerializeField,
     Tooltip("How far the mist travels")]
    private float baseTravelDistance = 0.1f;
#pragma warning restore 0649
    #endregion

    private AudioSource audioSource;

    #region Spell Implementation
    public override string Name => "Ice Mist";
    public override SpellElement Element => SpellElement.Ice;
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
        GameManager.S.Audio.CastIce(audioSource);

        transform.SetParent(null, true);
        StartCoroutine(AttackRoutine(Aim));
        return true;
    }

    public override bool Deactivate()
    {
        if (!isActive)
        {
            GameManager.S.Audio.DismissIce(audioSource);
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    protected override bool AdjustAttributeCore(SpellAttribute attribute, int power)
    {
        switch (attribute)
        {
            case SpellAttribute.Force:
                splash.Value += power;
                chill.Value += power;
                break;
            default:
                return false;
        }

        return true;
    }
    #endregion

    #region IGravityBallTargetable Implementation
    private float pullSpeed;

    public bool IsBeingTargeted { get; private set; } = false;

    public void OnBeginPull()
    {
        IsBeingTargeted = true;
        pullSpeed = 0f;
    }

    public void Pull(Vector3 location, float pullSpeed)
    {
        this.pullSpeed = Mathf.Max(pullSpeed, this.pullSpeed + pullSpeed * Time.fixedDeltaTime);
        xzTarget = location;
        xzTarget.y = 0;
    }

    public void OnEndPull()
    {
        IsBeingTargeted = true;
        pullSpeed = 0f;
    }
    #endregion

    #region IceMist Core
    /// <summary>
    /// Influences the size of the mist (higher = larger).
    /// Falls within [0, 200].
    /// </summary>
    public int Splash => splash.Value;
    private readonly SpellStat splash = new SpellStat();

    /// <summary>
    /// Influences the linger time of the mist (higher = longer).
    /// Falls within [0, 200].
    /// </summary>
    public int Chill => chill.Value;
    private readonly SpellStat chill = new SpellStat();

    public float Speed => speed;
    public float Radius => splash.TwoSidedLerp(baseLingerScale, 0.5f, 2f);
    public float Linger => chill.TwoSidedLerp(baseLinger, 0.5f, 2f);

    private new Rigidbody rigidbody;
    private new SphereCollider collider;
    private Vector3 xzTarget;

    private IEnumerator AttackRoutine(Ray aim)
    {
        transform.localScale = Vector3.one * releaseScale;

        //Origin in xz-plane
        Vector3 xzOrigin = aim.origin;
        xzOrigin.y = 0;

        //Travel direction in xz-plane
        Vector3 xzDirection = aim.direction;
        xzDirection.y = 0;
        xzDirection.Normalize();

        //Target in xz-plane
        xzTarget = xzOrigin + xzDirection * baseTravelDistance;
        xzTarget.y = 0;

        //Travel a set distance along the xz-plane before lingering.
        Vector3 xzPosition = rigidbody.position;
        xzPosition.y = 0;
        while (xzPosition != xzTarget)
        {
            yield return new WaitForFixedUpdate();

            //Want to travel along the xz-plane
            Vector3 target = Vector3.MoveTowards(xzPosition, xzTarget, Speed * Time.fixedDeltaTime);
            target.y = rigidbody.position.y;

            //Make sure the sphere stays on the ground as it travels to target
            //FIXME: SphereCast assumes that there's "enough" space above the spell
            float radius = collider.bounds.extents.x;
            Ray sphereRay = new Ray(target + Vector3.up * (radius + 1f), Vector3.down);
            bool isHit = Physics.SphereCast(
                sphereRay,
                radius,
                out RaycastHit hitInfo,
                Mathf.Infinity,
                LayerMask.GetMask("Environment")
            );

            //Stop moving if the sphere couldn't travel to the target
            if (!isHit)
            {
                break;
            }

            //Move to the position of the sphere's center after the SphereCast hit
            Vector3 newPosition = sphereRay.origin + sphereRay.direction * hitInfo.distance;
            rigidbody.MovePosition(newPosition);

            xzPosition = rigidbody.position;
            xzPosition.y = 0;

            //Grow in size
            if (transform.localScale.x < Radius)
            {
                transform.localScale += Vector3.one * growSpeed * Time.fixedDeltaTime;
            }
        }

        //Linger in place
        GameManager.S.Audio.IceHits(audioSource);
        yield return GrowToSize(Radius, 0.2f);
        yield return new WaitForSeconds(Linger);
        isActive = false;
        Destroy(gameObject);
    }

    private IEnumerator GrowToSize(float radius, float time)
    {
        float initialRadius = transform.localScale.x;
        if (true || initialRadius >= radius)
        {
            yield break;
        }
        float growSpeed = (radius - initialRadius) / time;

        while (transform.localScale.x < radius)
        {
            yield return new WaitForFixedUpdate();
            transform.localScale += Vector3.one * growSpeed * Time.fixedDeltaTime;
        }
        transform.localScale = Vector3.one * radius;
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
    }

    private void Start()
    {
        transform.localScale = Vector3.one * heldScale;
    }

    private void OnValidate()
    {
        baseTravelDistance = Mathf.Max(0.1f, baseTravelDistance);
        baseLingerScale = Mathf.Max(0.01f, baseLingerScale);
        speed = Mathf.Max(0.01f, speed);
        baseLinger = Mathf.Max(0.1f, baseLinger);

        heldScale = Mathf.Clamp01(heldScale);
        releaseScale = Mathf.Clamp01(releaseScale);
        growSpeed = Mathf.Max(0.01f, growSpeed);
    }
    #endregion
}
