using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public IDamageable Target { get; private set; }

    //When enemies' hitboxes collide with undestroyed defences, set it as their target
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Defence"))
        {
            Obelisk obelisk = col.gameObject.GetComponent<Obelisk>();
            if (obelisk != null && obelisk.IsAlive)
            {
                Target = obelisk;
                return;
            }

            ForceField forceField = col.gameObject.GetComponent<ForceField>();
            if (forceField != null && forceField.IsAlive)
            {
                Target = forceField;
                return;
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Defence"))
        {
            Target = null;
        }
    }
}
