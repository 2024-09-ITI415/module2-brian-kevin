using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S; // Singleton

    [Header("Set in Inspector")]
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

    private GameObject lastTriggerGo = null;
    public delegate void WeaponFireDelegate();
    public WeaponFireDelegate fireDelegate;

    // Dictionary to store the levels of each power-up
    private Dictionary<WeaponType, int> powerUpLevels = new Dictionary<WeaponType, int>();

    void Start()
    {
        if (S == null) S = this;
        else Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");

        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
    }

    void Update()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        if (Input.GetAxis("Jump") == 1 && fireDelegate != null)
        {
            fireDelegate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (go == lastTriggerGo) return;
        lastTriggerGo = go;

        if (go.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Destroy(go);
        }
        else if (go.CompareTag("PowerUp"))
        {
            AbsorbPowerUp(go);
        }
    }

    public void TakeDamage(int damage)
    {
        shieldLevel -= damage;
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        WeaponType puType = pu.type;

        // If the power-up is already in use, increase its level
        if (powerUpLevels.ContainsKey(puType))
        {
            powerUpLevels[puType]++;
        }
        else
        {
            powerUpLevels[puType] = 1;
        }

        // Apply stacked effects
        ApplyPowerUpEffects(puType);

        pu.AbsorbedBy(gameObject);
    }

    private void ApplyPowerUpEffects(WeaponType puType)
    {
        int level = powerUpLevels[puType];

        switch (puType)
        {
            case WeaponType.shield:
                shieldLevel = Mathf.Min(shieldLevel + level, 4);
                break;

            default:
                if (puType == weapons[0].type)
                {
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null) w.SetType(puType);
                }
                else
                {
                    ClearWeapons();
                    weapons[0].SetType(puType);
                }

                // Example: Increase firing rate or damage based on power-up level
                WeaponDefinition def = Main.GetWeaponDefinition(puType);
                def.delayBetweenShots /= (1 + (0.1f * level));
                def.damageOnHit *= (1 + (0.1f * level));
                break;
        }
    }

    public float shieldLevel
    {
        get { return _shieldLevel; }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            if (value < 0)
            {
                Destroy(this.gameObject);
                Main.S.DelayedRestart(gameRestartDelay);
            }
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        foreach (Weapon w in weapons)
        {
            if (w.type == WeaponType.none) return w;
        }
        return null;
    }

    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(WeaponType.none);
        }
    }
}
