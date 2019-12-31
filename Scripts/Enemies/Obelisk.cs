using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Obelisk : MonoBehaviour, IDamageable
{

#pragma warning disable 0649
    [SerializeField] private SpellElement affinity;
    [SerializeField] private float resistanceMultiplier;
    [SerializeField] private int maxHealth = 1;

    [SerializeField] private UnityEvent onDestroy;

    [SerializeField] private GameObject explosion;

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
        resistanceMultiplier = Mathf.Clamp(resistanceMultiplier, 0f, 1f);
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

    public float colorIntensity => (1 + 5 * (Health / MaxHealth));

    public void TakeDamage(int baseAmount, SpellElement damageType)
    {
        int damage = ComputeDamage(baseAmount, damageType);
        
        float H;
        float S;
        float V = colorIntensity;
        float not_used;

        Color col = transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.GetColor("_EmissionColor");
        Color.RGBToHSV(col, out H, out S, out not_used);
        Color newCol = Color.HSVToRGB(H, S, V);
        transform.GetChild(0).transform.GetChild(1).GetComponent<Renderer>().material.SetColor("_EmissionColor", newCol);
        
        
        if (damage > 0)
        {
            GameManager.S.Audio.AttackObelisk(audioSource);
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
        if (affinity != SpellElement.None && damageType == affinity)
        {
            amount = Mathf.RoundToInt(amount * resistanceMultiplier);
        }
        return amount;
    }

    private void OnDeath()
    {
        //Disable the child gameobject of the obelisk containing the tower particle effects
        // transform.GetChild(0).gameObject.SetActive(false);

        onDestroy.Invoke();
        GameManager.S.Audio.ObeliskDeathSound(audioSource);
    }

    public void ApplyExplosion(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
    {
       //Pass
    }
    #endregion

    public void Explode()
    {
        Instantiate(explosion, transform.position, transform.rotation);
    }
}