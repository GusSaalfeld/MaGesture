using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private DamageableGroup targetGroup;
    [SerializeField] private SpellElement damageType;
    [SerializeField] private int damage;
    [SerializeField] private float duration;

    [SerializeField] private bool friendlyFireOn;
    [SerializeField] private int friendlyFireDmg;

    [Header("Explosion Physics")]
    [SerializeField] private float force;
    [SerializeField] private float upwardsModifier;
#pragma warning restore 0649

    private new SphereCollider collider;

    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        Collider[] others = Physics.OverlapSphere(collider.center, collider.bounds.extents.x);
        foreach (Collider other in others)
        {
            IDamageable d = other.gameObject.GetComponent<IDamageable>();
            if (d == null || (targetGroup != d.Group && !friendlyFireOn))
            {
                continue;
            }
            Damage(d);
        }

        Destroy(gameObject, duration);
    }

    private void Damage(IDamageable d)
    {
        if(d.Group == targetGroup && friendlyFireOn) {
            d.TakeDamage(friendlyFireDmg, damageType);
        } else {
            d.TakeDamage(damage, damageType);
        }
        d.ApplyExplosion(force, transform.position, collider.bounds.extents.x, upwardsModifier, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable d = other.gameObject.GetComponent<IDamageable>();
        if (d == null) return;
        Damage(d);
    }

    private void OnValidate()
    {
        damage = Mathf.Max(0, damage);
        duration = Mathf.Max(0, duration);

        force = Mathf.Max(0, force);
        upwardsModifier = Mathf.Max(upwardsModifier);
    }
}
