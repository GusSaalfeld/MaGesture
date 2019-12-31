using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable, IGravityBallTargetable
{
#pragma warning disable 0649
    [SerializeField] private SpellElement affinity;
    [SerializeField] private float resistanceMultiplier;

    [SerializeField] private int maxHealth = 1;
    [SerializeField] private float minAttackCooldown;
    [SerializeField] private float maxAttackCooldown;
    [SerializeField] private int _strength;
    [SerializeField] protected SpellElement lastSpellType;

    [SerializeField] private GameObject currDest;
#pragma warning restore 0649

    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    private new Rigidbody rigidbody;
    private new Collider collider;
    private EnemyHitbox hitbox;

    public int Strength => _strength;

    public void SetDestination(GameObject destination)
    {
        currDest = destination;
        if (!navMeshAgent.isOnNavMesh)
        {
            Destroy(gameObject);
        }
        else
        {
            navMeshAgent.SetDestination(currDest.transform.position);
        }
    }

    //Possible error: update might mess up attacking if one is attacking obelisk that wasn't destroyed
    public void UpdateDestination(GameObject destination)
    {
        if (currDest != destination)
        {
            currDest = destination;
            UpdateCurrentAnimation(0, false);
            navMeshAgent.SetDestination(currDest.transform.position);
        }
    }

    protected virtual IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!CanAttack())
            {
                //Enable walking animation and wait until it's possible to attack
                UpdateCurrentAnimation(0, false);
                if (!IsBeingTargeted)
                {
                    navMeshAgent.isStopped = false;
                }
                yield return new WaitUntil(CanAttack);
            }

            //Enable attack animation and deal damage to target
            navMeshAgent.isStopped = true;

            if (animator.GetInteger("currentAnim") != 3 && animator.GetInteger("currentAnim") != 4)
            {
                UpdateCurrentAnimation(3 + Random.Range(0, 2), true);
            }
            hitbox.Target.TakeDamage(Strength, SpellElement.None);
            yield return new WaitForSeconds(Random.Range(minAttackCooldown, maxAttackCooldown));
        }
    }

    protected bool CanAttack()
    {
        return !IsBeingTargeted && hitbox.Target != null && !hitbox.Target.Equals(null) && hitbox.Target.Health > 0;
    }

    #region AI: Sheltering Under EnemyMage's shield
    private IEnumerator ShelterUnderEnemyMageRoutine()
    {
        if (this is EnemyMage) {
            yield break;
        }
        EnemyMage toShelterUnder = null;
        while (true)
        {
            toShelterUnder = FindBestEnemyMage();
            if (toShelterUnder != null)
            {
                
                GameObject shelterDestination = toShelterUnder.gameObject;
                GameObject primaryDestination = currDest;

                //Take shelter under the EnemyMage until it's destroyed/dies
                while (toShelterUnder == null || toShelterUnder.Equals(null) || !toShelterUnder.IsAlive)
                {
                    if (currDest != shelterDestination)
                    {
                        primaryDestination = currDest;
                    }
                    SetDestination(toShelterUnder.gameObject);
                    yield return new WaitForSeconds(0.1f);
                }

                //Revert to primary destination
                SetDestination(primaryDestination);
            }
            yield return new WaitForSeconds(10f);
        }
    }

    /// <summary>
    /// Find the best EnemyMage to take shelter under.
    /// </summary>
    private EnemyMage FindBestEnemyMage()
    {
        //Get nearby Colliders
        Collider[] enemyMages = Physics.OverlapSphere(transform.position, 15f);

        //Find the best EnemyMage to shelter under
        float lowestScore = Mathf.Infinity;
        EnemyMage bestShieldEnemy = null;
        foreach (Collider c in enemyMages)
        {
            EnemyMage enemyMage = c.GetComponent<EnemyMage>();
            if (enemyMage == null)
            {
                continue;
            }

            float score =
              Vector3.Distance(enemyMage.transform.position, transform.position) +
              Vector3.Distance(enemyMage.transform.position, navMeshAgent.destination);
            if (score < lowestScore)
            {
                lowestScore = score;
                bestShieldEnemy = enemyMage;
            }
        }

        if (bestShieldEnemy == null)
        {
            return null;
        }

        float distToDestination = Vector3.Distance(transform.position, navMeshAgent.destination);
        float bestShieldEnemyDistToDestination = Vector3.Distance(bestShieldEnemy.transform.position, navMeshAgent.destination);
        if (bestShieldEnemyDistToDestination > distToDestination + 5f)
        {
            return null;
        }

        return bestShieldEnemy;
    }
    #endregion

    #region IGravityBallTargetable
    public bool IsBeingTargeted { get; private set; } = false;

    public void Pull(Vector3 location, float pullSpeed)
    {
        //If close enough to location, set velocity to 0 and terminate
        Vector3 toGravityBall = location - transform.position;
        bool closeEnough = (collider.ClosestPointOnBounds(location) - location).sqrMagnitude < 0.01f;
        if (closeEnough)
        {
            rigidbody.velocity = Vector3.zero;
            return;
        }

        toGravityBall.Normalize();

        //Turn away from gravity ball
        transform.forward = Vector3.RotateTowards(
            transform.forward,
            -toGravityBall,
            Time.fixedDeltaTime * navMeshAgent.angularSpeed * Mathf.Deg2Rad,
            0f
        );

        //Pull toward gravity ball
        rigidbody.velocity = toGravityBall * rigidbody.velocity.magnitude;
        if (rigidbody.velocity.sqrMagnitude < pullSpeed * pullSpeed)
        {
            rigidbody.AddForce(toGravityBall * pullSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    public void OnBeginPull()
    {
        IsBeingTargeted = true;

        if (endPullRoutine != null)
        {
            StopCoroutine(endPullRoutine);
            endPullRoutine = null;
        }

        //Take control away from navMeshAgent
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        rigidbody.isKinematic = false;
        rigidbody.freezeRotation = true;

        //Disable model physics
        Collider[] modelColliders = animator.GetComponentsInChildren<Collider>();
        foreach (Collider c in modelColliders)
        {
            c.enabled = false;
        }
    }

    public void OnEndPull()
    {
        IsBeingTargeted = false;
        endPullRoutine = StartCoroutine(EndPullRoutine());
    }

    private Coroutine endPullRoutine;
    private IEnumerator EndPullRoutine()
    {
        //Enable model physics
        Collider[] modelColliders = animator.GetComponentsInChildren<Collider>();
        foreach (Collider c in modelColliders)
        {
            c.enabled = true;
        }

        //Give control back to navMeshAgent after 3 seconds
        yield return new WaitForSeconds(3f);
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
        rigidbody.isKinematic = true;
        rigidbody.freezeRotation = false;
        endPullRoutine = null;
    }
    #endregion

    #region IDamageable Implementation
    protected Damageable damageable;

    public DamageableGroup Group => DamageableGroup.Enemy;
    public int MaxHealth => damageable.MaxHealth;
    public int Health => damageable.Health;
    public bool IsAlive => damageable.IsAlive;

    public void TakeDamage(int baseAmount, SpellElement damageType)
    {
        damageable.TakeDamage(ComputeDamage(baseAmount, damageType));
        if (damageType == SpellElement.Ice)
        {
            remainingSlowSeconds = 5f;
        }
    }

    public virtual void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
        EnableRagdoll();
        rigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, mode);
    }

    private float remainingSlowSeconds = 0f;
    private IEnumerator SlowRoutine()
    {
        float originalSpeed = navMeshAgent.speed;
        while (true)
        {
            if (remainingSlowSeconds <= 0)
            {
                navMeshAgent.speed = originalSpeed;
                remainingSlowSeconds = 0;
                yield return new WaitUntil(() => remainingSlowSeconds > 0);
                navMeshAgent.speed = 0.5f * originalSpeed;
            }
            remainingSlowSeconds -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void EnableRagdoll()
    {
        animator.enabled = false;
        rigidbody.isKinematic = false;
        navMeshAgent.enabled = false;
        GetComponent<Collider>().enabled = false;
    }

    private int ComputeDamage(int baseAmount, SpellElement damageType)
    {
        int amount = baseAmount;
        if (affinity != SpellElement.None && damageType == affinity)
        {
            amount = Mathf.RoundToInt(amount * resistanceMultiplier);
        }
        return amount;
    }

    protected virtual void OnDeath()
    {
        EnableRagdoll();

        //Dead enemies can no longer move
        navMeshAgent.enabled = false;
        gameObject.layer = 9;

        //Clean up corpse after set amount of time
        Destroy(gameObject, 5f);

        animator.SetInteger("currentAnim", 5);
    }
    #endregion

    #region Unity Events
    private static bool todosAreLogged = false;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        damageable = new Damageable(maxHealth);
        damageable.AddDeathEffect(OnDeath);

        hitbox = GetComponentInChildren<EnemyHitbox>();

        StartCoroutine(AttackRoutine());
        StartCoroutine(SlowRoutine());
        //StartCoroutine(ShelterUnderEnemyMageRoutine());

        if (!todosAreLogged)
        {
            todosAreLogged = true;
            Debug.LogWarning("TODO: Enemy.ShelterUnderEnemyMageRoutine() is really hacky.  Consider refactoring.");
        }
    }


    private void OnValidate()
    {
        resistanceMultiplier = Mathf.Clamp(resistanceMultiplier, 0f, 1f);
        maxHealth = Mathf.Max(1, maxHealth);
        _strength = Mathf.Max(0, _strength);

        if (damageable != null && damageable.MaxHealth != maxHealth)
        {
            damageable.ChangeMaxHealth(maxHealth);
        }
    }
    #endregion

    public void UpdateCurrentAnimation(bool change_animNum, int animNum, bool change_attacking, bool attacking)
    {
        if (change_animNum)
            animator.SetInteger("currentAnim", animNum);
        if (change_attacking)
            animator.SetBool("attacking", attacking);
    }

    public void UpdateCurrentAnimation(int animNum, bool attacking)
    {
        UpdateCurrentAnimation(true, animNum, true, attacking);
    }

    public void UpdateCurrentAnimation(bool attacking)
    {
        UpdateCurrentAnimation(false, 0, true, attacking);
    }

    public void UpdateCurrentAnimation(int animNum)
    {
        UpdateCurrentAnimation(true, animNum, false, false);
    }
}
