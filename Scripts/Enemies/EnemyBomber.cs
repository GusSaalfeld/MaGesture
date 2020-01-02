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

    //Override default OnDeath() so inherited Enemy code auto-implements it
    protected override void OnDeath()
    {
        //Dead enemies can no longer move
        navMeshAgent.enabled = false;

        //Ragdoll self
        animator.enabled = false;
        gameObject.layer = 9;

        //If he reached a defense or was killed by a player spell, explode 
        if (killedSelf)
        {
            Explode();
        }
        else if (lastSpellType != SpellElement.None)
        {
            Explode();
        }
        
        //Clean up corpse after set amount of time
        Destroy(gameObject, 5f);
    }

    //Override AttackRoutine so bombers don't punch
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

    #region Unity Events
    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("Couldn't find AudioSource component.");
        }
    }
    #endregion
}
