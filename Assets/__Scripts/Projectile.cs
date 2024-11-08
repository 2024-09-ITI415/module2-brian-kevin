using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Set Dynamically")]
    public Rigidbody rigid;
    [SerializeField]
    private WeaponType _type;

    // Homing missile fields
    private Transform target;        // Target for the missile
    public float homingSpeed = 2f;   // Speed at which the missile rotates towards its target

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();

        // Null checks for required components
        if (bndCheck == null) Debug.LogError("BoundsCheck component missing on " + gameObject.name);
        if (rend == null) Debug.LogError("Renderer component missing on " + gameObject.name);
        if (rigid == null) Debug.LogError("Rigidbody component missing on " + gameObject.name);

        // If the projectile type is missile, find the closest target
        if (type == WeaponType.missile)
        {
            FindTarget();
        }
    }

    private void Update()
    {
        // Ensure bndCheck and rigid are not null before accessing them
        if (bndCheck == null || rigid == null) return;

        // Destroy projectile if it goes off screen
        if (bndCheck.offUp)
        {
            Destroy(gameObject);
            return;
        }

        // Homing behavior for missiles
        if (type == WeaponType.missile && target != null)
        {
            // Rotate towards the target
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, homingSpeed * Time.deltaTime);

            // Move forward in the direction of the target
            rigid.velocity = transform.up * Main.GetWeaponDefinition(type).velocity;
        }
    }

    public void SetType(WeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GetWeaponDefinition(_type);
        rend.material.color = def.projectileColor;

        // Set initial velocity for normal projectiles
        if (_type == WeaponType.blaster || _type == WeaponType.spread)
        {
            rigid.velocity = Vector3.up * def.velocity;
        }
        else if (_type == WeaponType.missile)
        {
            // Call FindTarget only if the projectile is a missile
            FindTarget();
        }
    }

    /// <summary>
    /// Finds the closest enemy to home in on if the projectile type is missile.
    /// </summary>
    private void FindTarget()
    {
        // Only find a target if one hasn't already been assigned
        if (target == null)
        {
            float closestDistance = Mathf.Infinity;
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = enemy.transform;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal damage for missile and other projectile types
            DealDamage(other);
            Destroy(gameObject);
        }
    }

    private void DealDamage(Collider enemy)
    {
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null)
        {
            WeaponDefinition def = Main.GetWeaponDefinition(_type);
            e.TakeDamage(def.damageOnHit); // One-time damage
        }
    }
}
