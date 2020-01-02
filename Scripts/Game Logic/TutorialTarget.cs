using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialTarget : MonoBehaviour, IDamageable, IGravityBallTargetable
{
    public UnityEvent onHit;

    public DamageableGroup Group => DamageableGroup.Enemy;

    public int MaxHealth => 999999999;

    public int Health => 999999999;

    public void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
        //Do nothing
    }

    public void TakeDamage(int baseDamage, SpellElement damageType)
    {
        onHit.Invoke();
    }

    public bool IsBeingTargeted { get; private set; }

    public void OnBeginPull()
    {
        IsBeingTargeted = true;
        onHit.Invoke();
    }

    public void Pull(Vector3 location, float pullSpeed)
    {
        //Do nothing
    }

    public void OnEndPull()
    {
        IsBeingTargeted = false;
    }
}
