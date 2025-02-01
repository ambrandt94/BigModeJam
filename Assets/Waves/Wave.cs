using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wave
{
    public string waveName = "Wave 1";
    public List<SubWave> subWaves = new List<SubWave>();
    public float delayBeforeStart = 0f;
    public event Action OnWaveStart;
    public event Action OnWaveEnd;

    private bool isRunning = false;
    private float timer = 0f;

    public bool IsComplete => enemiesAliveInWave == 0; // Wave is complete when no enemies are alive
    private int enemiesAliveInWave = 0; // Track enemies alive *in this wave*
    public bool IsRunning => isRunning;

    public void Start()
    {
        isRunning = true;
        OnWaveStart?.Invoke();
        if (subWaves.Count == 0)
        {
            OnWaveEnd?.Invoke();
            isRunning = false;
        }
        else
        {
            foreach (var subWave in subWaves)
            {
                subWave.Start();
            }
        }
    }

    public void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;

        if (timer >= delayBeforeStart)
        {
            foreach (var subWave in subWaves)
            {
                subWave.Update();
            }
        }

        if (IsComplete)
        {
            OnWaveEnd?.Invoke();
            isRunning = false;
        }
    }

    public void IncrementEnemiesAlive(int count)
    {
        enemiesAliveInWave += count;
    }

    public void DecrementEnemiesAlive()
    {
        enemiesAliveInWave--;
        if (enemiesAliveInWave < 0) enemiesAliveInWave = 0; // Safety check
    }
}


[Serializable]
public class SubWave
{
    public List<EnemySpawnInfo> enemyTypes = new List<EnemySpawnInfo>();
    public Transform spawnPoint;
    public float spawnDelay = 1f;
    public int numberOfEnemies = 10;
    public PathManager pathManager; // Add a public reference to your PathManager

    private int enemiesSpawned = 0;
    private float timer = 0f;
    private bool isRunning = false;

    public bool IsComplete => enemiesSpawned >= numberOfEnemies;

    public void Start()
    {
        isRunning = true;
    }

    public void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;

        if (timer >= spawnDelay && enemiesSpawned < numberOfEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }

        if (IsComplete)
        {
            isRunning = false;
        }
    }

    private void SpawnEnemy()
    {
        // This is now ONLY responsible for incrementing the counter.
        // Instantiation is handled by the WaveManager.
        enemiesSpawned++;
    }
}

[Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    // Add other properties like health, speed, etc. if needed.
}