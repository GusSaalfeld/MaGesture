using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A component that deals damage to <see cref="IDamageable"/> objects.
/// </summary>
public class Damager : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("Damage")]
    [SerializeField] private DamageableGroup targetGroup;
    [SerializeField] private SpellElement damageType;
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageFrequency = 1f;

    [Header("Explosion")]
    [SerializeField] private bool appliesExplosion = false;
    [SerializeField] private float explosionForce = 0.01f;
    [SerializeField] private float explosionRadius = 0.01f;
    [SerializeField] private float explosionUpwardsModifier = 0.01f;
#pragma warning restore 0649
    #endregion

    public void AddOnHitListener(UnityAction listener)
    {
        onHit.AddListener(listener);
    }

    #region Core
    private readonly ISet<IDamageable> enteringTargets = new HashSet<IDamageable>();
    private readonly ISet<IDamageable> targets = new HashSet<IDamageable>();

    private void Start()
    {
        Collider[] targets = Physics.OverlapSphere(this.transform.position, explosionRadius);
        foreach (Collider target in targets)
        {
            IDamageable t = target.GetComponent<IDamageable>();
            if (t == null)
            {
                continue;
            }
            AddTarget(t);
        }
    }
    private readonly UnityEvent onHit = new UnityEvent();

    private IEnumerator DamageRoutine()
    {
        while (true)
        {
            foreach (IDamageable t in targets)
            {
                t.TakeDamage(damage, damageType);
            }

            foreach (IDamageable t in enteringTargets)
            {
                targets.Add(t);
            }
            enteringTargets.Clear();

            yield return new WaitForSeconds(damageFrequency);
        }
    }

    private void AddTarget(IDamageable t)
    {
        if (t.Group != targetGroup)
        {
            return;
        }
        onHit.Invoke();
        enteringTargets.Add(t);
        DamageTarget(t);
    }

    private void RemoveTarget(IDamageable t)
    {
        enteringTargets.Remove(t);
        targets.Remove(t);
    }

    private void DamageTarget(IDamageable t)
    {
        t.TakeDamage(damage, damageType);
        if (appliesExplosion)
        {
            t.ApplyExplosion(explosionForce, transform.position, explosionRadius, explosionUpwardsModifier, ForceMode.VelocityChange);
        }
    }
    #endregion

    #region Unity Events
    private void OnEnable()
    {
        StartCoroutine(DamageRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable t = collision.collider.gameObject.GetComponent<IDamageable>();
        if (t == null)
        {
            return;
        }
        AddTarget(t);
    }

    private void OnCollisionExit(Collision collision)
    {
        IDamageable t = collision.collider.gameObject.GetComponent<IDamageable>();
        if (t == null)
        {
            return;
        }
        RemoveTarget(t);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable t = other.gameObject.GetComponent<IDamageable>();
        if (t == null)
        {
            return;
        }
        AddTarget(t);
    }

    private void OnTriggerExit(Collider other)
    {
        IDamageable t = other.gameObject.GetComponent<IDamageable>();
        if (t == null)
        {
            return;
        }
        RemoveTarget(t);
    }

    private void OnValidate()
    {
        damage = Mathf.Max(1, damage);
        damageFrequency = Mathf.Clamp(damageFrequency, 0.01f, 60f);

        explosionForce = Mathf.Max(0.01f, explosionForce);
        explosionRadius = Mathf.Max(0.01f, explosionRadius);
        explosionUpwardsModifier = Mathf.Max(0.01f, explosionUpwardsModifier);
    }
    #endregion
}
