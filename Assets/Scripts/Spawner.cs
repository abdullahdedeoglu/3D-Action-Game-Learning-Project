using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Spawner : MonoBehaviour
{
    private List<SpawnPoint> spawnPoints;
    private List<Character> spawnedCharacterList = new List<Character>();
    private bool hasSpawned;
    public UnityEvent OnAllSpawnedCharacterEliminated;

    private void Awake()
    {
        var spawnPointsList = transform.parent.GetComponentsInChildren<SpawnPoint>();
        spawnPoints = new List<SpawnPoint>(spawnPointsList);
    }
    private void Update()
    {
        if (!hasSpawned && spawnedCharacterList.Count == 0)
        {
            return;
        }

        bool allSpawnedAreDead = true;

        foreach (Character enemy in spawnedCharacterList)
        {
            if(enemy.currentState != Character.characterState.deathState)
            {
                allSpawnedAreDead = false;
                break;
            }
        }

        if (allSpawnedAreDead)
        {
            if(OnAllSpawnedCharacterEliminated != null)
                OnAllSpawnedCharacterEliminated.Invoke();
            spawnedCharacterList.Clear();
        }
    }
    public void SpawnEnemy()
    {
        if(hasSpawned) return;
        hasSpawned = true;

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if(spawnPoint.enemyToSpawn != null)
            {
                GameObject spawnedEnemy = Instantiate(spawnPoint.enemyToSpawn, spawnPoint.transform.position, Quaternion.identity);
                spawnedCharacterList.Add(spawnedEnemy.GetComponent<Character>());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            SpawnEnemy();
        }
    }
}
