using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DamageableGroup
{
    Enemy,
    Player,
}

/// <summary>
/// Implement this to be affected by <see cref="Damager"/>.
/// </summary>
public interface IDamageable
{
    DamageableGroup Group { get; }
    int MaxHealth { get; }
    int Health { get; }
    void TakeDamage(int baseDamage, SpellElement damageType);
    void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode);
}

/// <summary>
/// A helper class for handling Health, MaxHealth, and death for objects
/// implementing <see cref="IDamageable"/>.
/// </summary>
public class Damageable
{
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

    private readonly ISet<UnityAction> deathEffects = new HashSet<UnityAction>();

    public Damageable(int maxHealth)
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
    }

    public void ChangeMaxHealth(int newMaxHealth)
    {
        if (newMaxHealth < 1)
        {
            throw new System.ArgumentException("Tried to set Damageable max health to < 1");
        }
        MaxHealth = newMaxHealth;
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (Health == 0)
        {
            return;
        }

        Health = Mathf.Max(0, Health - amount);
        if (Health == 0)
        {
            foreach (UnityAction deathEffect in deathEffects)
            {
                deathEffect();
            }
        }
    }

    public void KillSelf() {
        foreach (UnityAction deathEffect in deathEffects)
        {
                deathEffect();
        }
    }

    public void AddDeathEffect(UnityAction effect)
    {
        deathEffects.Add(effect);
    }
}
