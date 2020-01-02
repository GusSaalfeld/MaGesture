using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBig : Enemy
{
#pragma warning disable 0649
    [SerializeField] private GameObject explosionPrefab;
    private bool killedSelf;
#pragma warning restore 0649

    private AudioSource audioSource;

    //This override ensures they don't ragdoll like smaller enemies do in response to being hit
    public override void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {

    }
}
