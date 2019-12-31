using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ForceField : MonoBehaviour, IDamageable
{
#pragma warning disable 0649
    [SerializeField] private int maxHealth = 1;
#pragma warning restore 0649

    private AudioSource audioSource;

    #region Unity Events
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("Couldn't find AudioSource component.");
        }
        damageable = new Damageable(maxHealth);
        damageable.AddDeathEffect(OnDeath);
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

    public DamageableGroup Group => DamageableGroup.Player;
    public int MaxHealth => damageable.MaxHealth;
    public int Health => damageable.Health;
    public bool IsAlive => damageable.IsAlive;

    public void TakeDamage(int baseAmount, SpellElement damageType)
    {
        int damage = ComputeDamage(baseAmount, damageType);
        if (damage > 0)
        {
            GameManager.S.Audio.AttackShield(audioSource);
            damageable.TakeDamage(damage);
        }
    }

    public void AddObeliskDeathEffect(UnityAction effect)
    {
        damageable.AddDeathEffect(effect);
    }

    private int ComputeDamage(int baseAmount, SpellElement damageType)
    {
        int amount = baseAmount;
        return amount;
    }

    private void OnDeath()
    {
        //Disable the child gameobject of the obelisk containing the tower particle effects
        Destroy(gameObject);

    }

    public void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
        //DO NOTHING!
    }
    #endregion
}
