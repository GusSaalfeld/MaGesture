using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyForceField : MonoBehaviour, IDamageable
{
#pragma warning disable 0649
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private SpellElement affinity;
    [SerializeField] private float resistanceMultiplier;
#pragma warning restore 0649

    #region Unity Events
    private void Awake()
    {
        damageable = new Damageable(maxHealth);
        damageable.AddDeathEffect(OnDeath);
    }

    private void Start() {
        if(affinity == SpellElement.None) {
            int i = Random.Range(0, 2);
            if(i == 0) {
                affinity = SpellElement.Fire;
                Color col = Color.red;
                col.a = 0.7f;
                GetComponent<Renderer>().material.SetColor("_MainColor", col);
            } else if (i == 1) {
                affinity = SpellElement.Ice;
                Color col = Color.blue;
                col.a = 0.7f;
                GetComponent<Renderer>().material.SetColor("_MainColor", col);
                
            }
            
        }
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        if (damageable != null && damageable.MaxHealth != maxHealth)
        {
            damageable.ChangeMaxHealth(maxHealth);
        }
    }
    #endregion

    #region IDamageable Implementation
    private Damageable damageable;

    public DamageableGroup Group => DamageableGroup.Enemy;
    public int MaxHealth => damageable.MaxHealth;
    public int Health => damageable.Health;
    public bool IsAlive => damageable.IsAlive;

    public void TakeDamage(int baseAmount, SpellElement damageType)
    {
        damageable.TakeDamage(ComputeDamage(baseAmount, damageType));
    }

    public void AddObeliskDeathEffect(UnityAction effect)
    {
        damageable.AddDeathEffect(effect);
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

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    public void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
        //DO NOTHING!
    }
    #endregion
}
