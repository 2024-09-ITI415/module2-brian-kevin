using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    public float speed = 5f;             // Missile movement speed
    public float homingSpeed = 2f;       // Speed at which the missile rotates toward the target
    public float damage = 10f;           // Damage dealt upon impact

    private Transform target;            // Current target for the missile
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        FindTarget(); // Find the closest enemy when missile is created
    }

    private void Update()
    {
        if (target != null)
        {
            // Rotate the missile to face the target smoothly
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, homingSpeed * Time.deltaTime);

            // Move the missile forward in the direction it is facing
            rb.velocity = transform.up * speed;
        }
        else
        {
            // If no target found, destroy the missile after a short delay to avoid indefinite existence
            Destroy(gameObject, 2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Deal damage to the enemy and destroy the missile
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy the missile on impact
        }
    }

    /// <summary>
    /// Finds the closest enemy to target for homing behavior.
    /// </summary>
    private void FindTarget()
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
