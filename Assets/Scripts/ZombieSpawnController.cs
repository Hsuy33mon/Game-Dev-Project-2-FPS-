using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZombieSpawnController : MonoBehaviour
{
    public int initialZombiesPerWave = 2;
    public int currentZombiesPerWave;

    public float spawnDelay = 0.5f; // Delay between spawning each zombie spawn in a wave

    public int currentWave = 0;
    public float waveCooldown = 10.0f; // Time in seconds between waves

    public bool inCooldown;
    public float cooldownCounter = 0; // For testing and UI
    public bool gameCompleted = false; // Track if all waves are completed

    public List<Enemy> currentZombiesAlive;
    public GameObject zombiePrefab;

    public TextMeshProUGUI waveOverUI;
    public TextMeshProUGUI cooldownCounterUI;
    public TextMeshProUGUI currentWaveUI;

    void Start()
    {
        GlobalReferences.Instance.waveNumber = currentWave;
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentZombiesAlive.Clear();
        currentWave++;
        GlobalReferences.Instance.waveNumber = currentWave;

        // Stop spawning after wave 5
        if (currentWave > 5)
        {
            Debug.Log("All waves completed! No more zombies will spawn.");
            currentWaveUI.text = "All Waves Complete!";
            return;
        }

        // Calculate zombies per wave: 2, 4, 8, 16, 32 (waves 1-5)
        currentZombiesPerWave = initialZombiesPerWave * (int)Mathf.Pow(2, currentWave - 1);

        Debug.Log($"Wave {currentWave}: Spawning {currentZombiesPerWave} zombies (initialZombiesPerWave = {initialZombiesPerWave})");

        currentWaveUI.text = "Wave: " + currentWave.ToString();
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < currentZombiesPerWave; i++)
        {
            // Generate a random offset within a specified range
            Vector3 spawnOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            Vector3 spawnPosition = transform.position + spawnOffset;

            // Instantiate the Zombie
            var zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);

            // Get Enemy Script
            Enemy enemyScript = zombie.GetComponent<Enemy>();

            // Track this zombie
            currentZombiesAlive.Add(enemyScript);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Skip all updates if game is completed
        if (gameCompleted)
        {
            return;
        }

        // Get all dead zombies
        List<Enemy> zombiesToRemove = new List<Enemy>();
        foreach (Enemy zombie in currentZombiesAlive)
        {
            if (zombie.isDead)
            {
                zombiesToRemove.Add(zombie);
            }
        }

        // Remove dead zombies from the list
        foreach (Enemy zombie in zombiesToRemove)
        {
            currentZombiesAlive.Remove(zombie);
        }

        zombiesToRemove.Clear();

        // Start Cooldown if all zombies are dead
        if (currentZombiesAlive.Count == 0 && !inCooldown)
        {
            // Start Cooldown for next wave
            StartCoroutine(WaveCooldown());
        }

        // Run Cooldown Timer
        if (inCooldown)
        {
            cooldownCounter -= Time.deltaTime;
        }
        else
        {
            cooldownCounter = waveCooldown;
        }

        cooldownCounterUI.text = cooldownCounter.ToString("F0");
    }

    private IEnumerator WaveCooldown()
    {
        inCooldown = true;

        // Check if we just completed wave 5
        if (currentWave >= 5)
        {
            gameCompleted = true;
            waveOverUI.text = "All Waves Complete! You Win!";
            waveOverUI.gameObject.SetActive(true);

            // Hide the cooldown counter UI when game is completed
            cooldownCounterUI.text = "";

            // Clean up dead zombies after a short delay
            yield return new WaitForSeconds(5.0f);
            CleanupDeadZombies();

            Debug.Log("Game completed! All 5 waves finished.");
            // Don't start next wave, game is complete
            yield break;
        }

        waveOverUI.gameObject.SetActive(true);

        // Clean up dead zombies after a short delay to let death animations play
        yield return new WaitForSeconds(5.0f);
        CleanupDeadZombies();

        // Continue with the rest of the cooldown
        yield return new WaitForSeconds(waveCooldown - 5.0f);

        inCooldown = false;
        waveOverUI.gameObject.SetActive(false);

        StartNextWave();
    }

    private void CleanupDeadZombies()
    {
        // Find all dead zombies in the scene and destroy them
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.isDead)
            {
                enemy.DestroyZombie();
            }
        }
    }
}
