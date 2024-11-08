using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    public int damage = 1; // Damage dealt by the projectile

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Hero"))
        {
            Hero hero = coll.GetComponent<Hero>();
            if (hero != null)
            {
                hero.TakeDamage(damage); // Apply damage to the hero
            }
            Destroy(gameObject); // Destroy the projectile on impact
        }
    }
}

