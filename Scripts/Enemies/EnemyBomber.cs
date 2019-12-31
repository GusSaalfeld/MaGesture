using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBomber : Enemy
{
#pragma warning disable 0649
    [SerializeField] private GameObject explosionPrefab;
    private bool killedSelf;
#pragma warning restore 0649

    private AudioSource audioSource;

    protected override void OnDeath()
    {
        //Dead enemies can no longer move
        navMeshAgent.enabled = false;

        //Ragdoll self
        animator.enabled = false;
        gameObject.layer = 9;

        //TODO: Make killed self explosion based off their affinity.
        //  So each affinity that spell type. (Ice explosion vs. fire explosion)
        if (killedSelf)
        {
            Explode();
            //TODO: Maybe just autoexplode him for looks.
        }
        else
        {
            if (lastSpellType != SpellElement.None)
            {
                Explode();
            }
        }
        //Clean up corpse after set amount of time
        Destroy(gameObject, 5f);
    }


    protected override IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!CanAttack())
            {
                yield return new WaitUntil(CanAttack);
            }

            killedSelf = true;
            damageable.KillSelf();

            break;
        }
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        GameManager.S.Audio.EnemyBomberExplosion(audioSource);
    }

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("Couldn't find AudioSource component.");
        }
    }
}
