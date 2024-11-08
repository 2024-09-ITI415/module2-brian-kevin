using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_5 : Enemy
{
    [Header("Set in Inspector: Enemy_5")]
    public float lifeTime = 5;
    public float sineFrequency = 2f;
    public float sineAmplitude = 0.5f;
    public Color startColor = Color.red;
    public Color endColor = Color.blue;
    public GameObject projectilePrefab;  // Assign the projectile prefab here
    public float enemyFireRate = 1f;     // Shots per second
    public float projectileSpeed = 20f;   // Speed of the projectile

    [Header("Set Dynamically: Enemy_5")]
    public Vector3[] points;
    public float birthTime;
    private Vector3 p0, p1;
    private float timeStart;
    private float duration = 4;
    private SpriteRenderer sr;
    private float nextFireTime;

    void InitMovement()
    {
        p0 = p1;
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);
        timeStart = Time.time;
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogWarning("SpriteRenderer not found on Enemy_5. Color change will be skipped.");
        }
        else
        {
            sr.color = startColor;
        }

        InitMovement();
        points = new Vector3[3];
        points[0] = pos;

        float xMin = -bndCheck.camWidth + bndCheck.radius;
        float xMax = bndCheck.camWidth - bndCheck.radius;

        Vector3 v;
        v = Vector3.zero;
        v.x = Random.Range(xMin, xMax);
        v.y = -bndCheck.camHeight * Random.Range(2.75f, 2);
        points[1] = v;

        v = Vector3.zero;
        v.y = pos.y;
        v.x = Random.Range(xMin, xMax);
        points[2] = v;

        birthTime = Time.time;
        nextFireTime = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2);

        Vector3 newPos = ((1 - u) * p0) + (u * p1);

        float sineWaveOffset = Mathf.Sin(Time.time * sineFrequency) * sineAmplitude;
        newPos.y += sineWaveOffset;

        if (sr != null)
        {
            float colorLerp = Mathf.PingPong(Time.time, lifeTime) / lifeTime;
            sr.color = Color.Lerp(startColor, endColor, colorLerp);
        }

        float speedMultiplier = Mathf.Abs(Mathf.Sin(Time.time * sineFrequency * 0.5f)) + 0.5f;
        newPos += (p1 - p0).normalized * speedMultiplier * Time.deltaTime;

        pos = newPos;

        FireProjectile();
    }

    void FireProjectile()
    {
        if (Time.time >= nextFireTime)
        {
            // Instantiate the projectile at the current position
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Ensure the projectile is on the correct layer
            projectile.layer = LayerMask.NameToLayer("ProjectileEnemy");

            // Apply downward velocity to the projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.down * projectileSpeed;  // Moves the projectile downward in 3D space
            }

            // Ignore collision between Enemy_5 and its projectile
            Collider enemyCollider = GetComponent<Collider>();
            Collider projectileCollider = projectile.GetComponent<Collider>();
            if (enemyCollider != null && projectileCollider != null)
            {
                Physics.IgnoreCollision(enemyCollider, projectileCollider);
            }

            // Update the next fire time
            nextFireTime = Time.time + (1f / enemyFireRate);
        }
    }
}
