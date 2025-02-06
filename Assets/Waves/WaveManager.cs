using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public List<Wave> waves = new List<Wave>();
    public event Action OnAllWavesComplete;
    public event Action<Wave> OnWaveStart;
    public event Action<Wave> OnWaveEnd;

    private int currentWaveIndex = 0;
    private Wave currentWave;
    private int enemiesAlive = 0;
    

    public bool AllWavesComplete => currentWaveIndex >= waves.Count;
    public bool WavesRunning => currentWave != null && currentWave.IsRunning;

    [Button("Randomize Span Positions")]
    private void RandomizeSpawnPositions()
    {
        GameObject[] randomSpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
        foreach(var wave in waves) {
            foreach (var sub in wave.subWaves) {
                sub.spawnPoint = ChainUtils.GetRandom(randomSpawns).transform;
            }
        }
    }
    [Button("Randomize Delays")]
    private void RandomizeSpawnDelays()
    {
        foreach (var wave in waves) {
            foreach (var sub in wave.subWaves) {
                sub.spawnDelay = UnityEngine.Random.Range(1, 3);
            }
        }
    }

    public void StartWaves()
    {
        currentWaveIndex = 0;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (AllWavesComplete)
        {
            OnAllWavesComplete?.Invoke();
            return;
        }

        StartCoroutine(NetWaveroutine());
    }

    private IEnumerator NetWaveroutine()
    {
        if (enemiesAlive > 0) yield break;

        currentWave = waves[currentWaveIndex];
        OnWaveStart?.Invoke(currentWave);
        currentWave.Start();
        currentWave.OnWaveEnd += OnCurrentWaveEnded;

        foreach (SubWave subWave in currentWave.subWaves) {
            foreach (EnemySpawnInfo enemyInfo in subWave.enemyTypes) {
                for (int i = 0; i < subWave.numberOfEnemies; i++) //Loop through all enemies to be spawned in this subwave
                {
                    Vector3 position = subWave.spawnPoint.position + (UnityEngine.Random.insideUnitSphere * 2);
                    GameObject enemyInstance = Instantiate(enemyInfo.enemyPrefab, position, subWave.spawnPoint.rotation); //Instantiate the enemy
                    Debug.Log($"Spawning enemy type: {enemyInfo.enemyPrefab.name}", enemyInstance.gameObject);
                    currentWave.IncrementEnemiesAlive(1); //Increment enemies alive in THIS wave
                    SmoothPatrolBehavior smoothPatrolBehavior = enemyInstance.GetComponent<SmoothPatrolBehavior>();
                    smoothPatrolBehavior?.SetPathManager(subWave.pathManager);
                    enemiesAlive++; //Increment alive enemies here.
                    FlyingEnemy enemy = enemyInstance.GetComponent<FlyingEnemy>();
                    if (enemy != null) {
                        enemy.OnDestroyed += OnEnemyDestroyed; // Subscribe to the INSTANCE's event
                    } else {
                        Debug.LogWarning($"Enemy instance {enemyInstance.name} does not have an Enemy component with an OnDestroyed event.");
                    }
                    yield return new WaitForSeconds(subWave.spawnDelay);
                }
            }
        }
    }

    private void OnEnemyDestroyed(FlyingEnemy destroyedEnemy)
    {
        currentWave.DecrementEnemiesAlive(); //Decrement alive enemies in THIS wave
        enemiesAlive--;
        if (enemiesAlive < 0) enemiesAlive = 0;

        destroyedEnemy.OnDestroyed -= OnEnemyDestroyed;
    }

    private void OnCurrentWaveEnded()
    {
        OnWaveEnd?.Invoke(currentWave);
        currentWave.OnWaveEnd -= OnCurrentWaveEnded;

        currentWaveIndex++;

        if (AllWavesComplete && enemiesAlive == 0)
        {
            FindObjectOfType<WorldManager>().TriggerGameFinish();
            OnAllWavesComplete?.Invoke();
        }
        else if (AllWavesComplete)
        {
            FindObjectOfType<WorldManager>().TriggerGameFinish();
            return; //Wait for all enemies to be killed
        }
        else
        {
            StartNextWave();
        }
    }

    public void Update()
    {
        if (WavesRunning)
        {
            currentWave.Update();
        }
    }
}