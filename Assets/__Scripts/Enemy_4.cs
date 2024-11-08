using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Part is another serializable data storage class just like WeaponDefinition
/// </summary>
[System.Serializable]
public class Part
{
    public string name; // The name of this part
    public float health; // The amount of health this part has
    public string[] protectedBy; // The other parts that protect this

    // These two fields are set automatically in Start()
    [HideInInspector]
    public GameObject go; // The GameObject of this part
    [HideInInspector]
    public Material mat; // The Material to show damage
}

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts; // The array of ship Parts

    private Vector3 p0, p1; // The two points to interpolate
    private float timeStart; // Birth time for this Enemy_4
    private float duration = 4; // Duration of movement

    private void Start()
    {
        // There is already an initial position chosen by Main.SpawnEnemy()
        p0 = p1 = pos;

        InitMovement();

        // Cache GameObject & Material of each Part in parts
        Transform t;
        foreach (Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
            else
            {
                Debug.LogError("Part named " + prt.name + " not found in children of " + gameObject.name);
            }
        }
    }

    void InitMovement()
    {
        p0 = p1; // Set p0 to the old p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        // Reset the time
        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); // Apply Ease Out easing to u
        pos = ((1 - u) * p0) + (u * p1); // Simple linear interpolation
    }

    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name == n)
            {
                return prt;
            }
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go == go)
            {
                return prt;
            }
        }
        return null;
    }

    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }

    bool Destroyed(Part prt)
    {
        if (prt == null) // If no real part was passed in
        {
            return true; // Return true (meaning yes, it was destroyed)
        }
        return (prt.health <= 0);
    }

    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();

                // Check if Projectile is null
                if (p == null)
                {
                    Debug.LogError("ProjectileHero collided but has no Projectile component.");
                    Destroy(other);
                    return;
                }

                // If this Enemy is off screen, don't damage it.
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    return;
                }

                // Hurt this Enemy
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);

                // If prtHit is null, try the otherCollider
                if (prtHit == null)
                {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                    if (prtHit == null)
                    {
                        Debug.LogError("Part not found for the collided GameObject: " + goHit.name);
                        Destroy(other);
                        return;
                    }
                }

                // Check whether this part is still protected
                if (prtHit.protectedBy != null)
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        if (!Destroyed(s)) // If one of the protecting parts hasn't been destroyed...
                        {
                            Destroy(other); // Destroy the ProjectileHero
                            return; // return before damaging Enemy_4
                        }
                    }
                }

                // It's not protected, so make it take damage
                WeaponDefinition def = Main.GetWeaponDefinition(p.type);

                // Ensure that WeaponDefinition is not null
                if (def == null)
                {
                    Debug.LogError("WeaponDefinition not found for Projectile type: " + p.type);
                    Destroy(other);
                    return;
                }

                // Apply damage
                prtHit.health -= def.damageOnHit;
                ShowLocalizedDamage(prtHit.mat);

                if (prtHit.health <= 0)
                {
                    prtHit.go.SetActive(false);
                }

                // Check if all parts are destroyed
                bool allDestroyed = true;
                foreach (Part prt in parts)
                {
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }

                if (allDestroyed)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }

                Destroy(other); // Destroy the ProjectileHero
                break;
        }
    }
}
