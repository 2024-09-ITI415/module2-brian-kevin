using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    static public Main S; // Singleton instance for Main
    static Dictionary<WeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Set in Inspector")]
    public GameObject[] prefabEnemies; // Array of enemy prefabs
    public float enemySpawnPerSecond = 0.5f; // Enemies per second
    public float enemyDefaultPadding = 1.5f; // Padding for enemy spawn position
    public WeaponDefinition[] weaponDefinitions; // Array of weapon definitions
    public GameObject prefabPowerUp; // Power-up prefab
    public WeaponType[] powerUpFrequency = new WeaponType[]
    {
        WeaponType.blaster, WeaponType.blaster, WeaponType.spread, WeaponType.shield, WeaponType.missile
    };

    private BoundsCheck bndCheck;

    private void Awake()
    {
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds based on default values)
        Invoke("SpawnEnemy", 1f / enemySpawnPerSecond);

        // Initialize the weapon dictionary with WeaponDefinitions
        WEAP_DICT = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }
    }

    public void SpawnEnemy()
    {
        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);

        // Position the enemy above the screen with a random x position
        float enemyPadding = enemyDefaultPadding;
        if (go.GetComponent<BoundsCheck>() != null)
        {
            enemyPadding = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned enemy
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyPadding;
        float xMax = bndCheck.camWidth - enemyPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyPadding;
        go.transform.position = pos;

        // Invoke SpawnEnemy() again
        Invoke("SpawnEnemy", 1f / enemySpawnPerSecond);
    }

    public void ShipDestroyed(Enemy e)
    {
        // Possibly generate a power-up upon enemy destruction
        if (Random.value <= e.powerUpDropChance)
        {
            // Choose a power-up type from powerUpFrequency
            int ndx = Random.Range(0, powerUpFrequency.Length);
            WeaponType puType = powerUpFrequency[ndx];

            // Spawn a PowerUp and set its type
            GameObject go = Instantiate(prefabPowerUp) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            pu.SetType(puType);

            // Set the position of the power-up to the destroyed enemy's position
            pu.transform.position = e.transform.position;
        }
    }

    public void DelayedRestart(float delay)
    {
        // Invoke the Restart() method after a delay
        Invoke("Restart", delay);
    }

    public void Restart()
    {
        // Reload the game scene to restart
        SceneManager.LoadScene("_Scene_0");
    }

    ///<summary>
    /// Static function to retrieve a WeaponDefinition from the WEAP_DICT
    /// </summary>
    /// <returns>The WeaponDefinition or a new WeaponDefinition with type none if not found</returns>
    /// <param name="wt">The WeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GetWeaponDefinition(WeaponType wt)
    {
        // Check to make sure that the key exists in the dictionary
        if (WEAP_DICT.ContainsKey(wt))
        {
            return (WEAP_DICT[wt]);
        }
        // Return a default WeaponDefinition if not found
        return new WeaponDefinition();
    }
}
