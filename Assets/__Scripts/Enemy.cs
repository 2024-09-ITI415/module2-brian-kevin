using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Set in Inspector: Enemy")]
    public float speed = 10f; // The speed in m/s
    public float fireRate = 0.3f; // Seconds/shot (Unused)
    public float health = 10;
    public int score = 100; // Points earned for destroying this
    public float showDamageDuration = 0.1f; // # seconds to show damage
    public float powerUpDropChance = 1f; // Chance to drop a power-up

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials; // All the Materials of this & its children
    public bool showingDamage = false;
    public float damageDoneTime; // Time to stop showing damage
    public bool notifiedOfDestruction = false; // Will be used later

    protected BoundsCheck bndCheck;

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    // Property for position
    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void Update()
    {
        Move();

        if (showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if (bndCheck != null && bndCheck.offDown)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    // General method to handle all types of damage
    public void TakeDamage(float damage)
    {
        health -= damage;
        ShowDamage();

        if (health <= 0)
        {
            if (!notifiedOfDestruction)
            {
                Main.S.ShipDestroyed(this);
                notifiedOfDestruction = true;
            }
            Destroy(gameObject);
        }
    }

    // Updated to use TakeDamage for projectile damage as well
    private void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;
        switch (otherGO.tag)
        {
            case "ProjectileHero":
                Projectile p = otherGO.GetComponent<Projectile>();
                
                if (p != null)
                {
                    if (!bndCheck.isOnScreen)
                    {
                        Destroy(otherGO);
                        break;
                    }

                    // Apply damage based on projectile's damage amount
                    TakeDamage(Main.GetWeaponDefinition(p.type).damageOnHit);
                    Destroy(otherGO);
                }
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGO.name);
                break;
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials)
        {
            m.color = Color.red;
        }
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }
}
