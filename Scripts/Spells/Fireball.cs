using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fireballs are launched by the FireFlower spell.
/// </summary>
public class Fireball : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Explosion explosionPrefab;
    [SerializeField] private float autoAimRadius = 1;
    [SerializeField] private bool autoFire;
#pragma warning restore 0649

    private AudioSource audioSource;
    private Damager damager;

    public int movement_speed;

    private Transform target;
    private Vector3 centerOffset;
    private Enemy enemy;

    private void Start()
    {
        if (autoFire)
        {
            ShootFireball();
        }
    }

    public void FireFrom(Ray aim)
    {
        transform.position = aim.origin;
        transform.forward = aim.direction;
        ShootFireball();
    }

    public void ShootFireball()
    {
        RaycastHit ray;
        if (Physics.SphereCast(transform.position, autoAimRadius, transform.forward, out ray, 1000f, LayerMask.GetMask("Enemy")))
        {
            target = ray.transform;
            centerOffset = target.InverseTransformPoint(ray.collider.bounds.center);
        }

        GameManager.S.Audio.CastFire(audioSource);
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        float timePassed = 0f;
        while (timePassed < 10f)
        {
            yield return new WaitForFixedUpdate();
            if (target != null && target.gameObject.layer == 11)
            {
                Vector3 targetPos = target.position + centerOffset;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * movement_speed);
                if((targetPos -  transform.position).magnitude > 0)
                    transform.forward = targetPos - transform.position;
            }
            else transform.position += transform.forward * Time.fixedDeltaTime * movement_speed;
            timePassed += Time.fixedDeltaTime;
        }
        Destroy(gameObject);
    }

    private void Detonate()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    #region Unity Events
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("Couldn't find AudioSource component.");
        }
        damager = GetComponent<Damager>();
        damager.AddOnHitListener(() => Detonate());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Environment"))
        {
            Detonate();
        }
    }
    #endregion
}
