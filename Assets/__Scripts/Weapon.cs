using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none,       // The default / no weapons
    blaster,    // A simple blaster
    spread,     // Two shots simultaneously
    phaser,     // [NI] Shots that move in waves
    missile,    // Homing missiles
    shield      // Raise shield level
}

[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter;                  // Letter to show on the power-up
    public Color color = Color.white;      // Color of Collar & power-up
    public GameObject projectilePrefab;    // Prefab for projectiles
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;          // Damage per shot
    public float delayBetweenShots = 0;    // Delay between shots
    public float velocity = 20;            // Speed of projectiles
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime; // Time last shot was fired
    private Renderer collarRend;

    private void Start()
    {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();

        // Call SetType() for the default _type of WeaponType.none
        SetType(_type);

        // Dynamically create an anchor for all Projectiles
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        // Find the fireDelegate of the root GameObject
        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Hero>() != null)
        {
            rootGO.GetComponent<Hero>().fireDelegate += Fire;
        }
    }

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0; // You can fire immediately after _type is set.
    }

    public void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;

        // Check if enough time has passed since the last shot
        if (Time.time - lastShotTime < def.delayBetweenShots)
        {
            return;
        }

        Projectile p;
        switch (type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.velocity = Vector3.up * def.velocity;
                break;

            case WeaponType.spread:
                p = MakeProjectile();
                p.rigid.velocity = Vector3.up * def.velocity;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * Vector3.up * def.velocity;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * Vector3.up * def.velocity;
                break;

            case WeaponType.missile:
                p = MakeProjectile();
                p.rigid.velocity = Vector3.up * def.velocity;
                break;
        }

        // Update the lastShotTime to enforce the delay
        lastShotTime = Time.time;
    }

    public Projectile MakeProjectile()
    {
        if (def.projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is missing for weapon type: " + type);
            return null;
        }

        GameObject go = Instantiate(def.projectilePrefab);
        if (collar != null) 
        {
            go.transform.position = collar.transform.position;
        }
        else
        {
            go.transform.position = transform.position;
        }

        if (transform.parent.CompareTag("Hero"))
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }

        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShotTime = Time.time;

        return p;
    }
}
