using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    private List<EnemySpawnRate> currentEnemyRates;
    //Armazena a lista de inimigos e chances da wave atual

    public int Enemycounter;
    //quantos inimigos vão spawnar, deixei público para o SpawnPointsControllerScript acessar e ajudar na WinCondition

    [SerializeField] float SpawnCoolDown;
    //variável que vai servir para marcar o intervalo de um spawn para outro

    Coroutine SpawningCycleVar;
    //variável para armazenar no cache

    public void SetupSpawner(int attempts, List<EnemySpawnRate> enemyRates)
    {
        Enemycounter = attempts;
        currentEnemyRates = enemyRates;
    }

    
    public void StartSpawning()
    {
        if (SpawningCycleVar == null)
        {
            SpawningCycleVar = StartCoroutine(SpawningCycle());
        }
    }

    //esse item tem que instanciar de tempos em tempos o gameObject do inimigo
    //depois de alguns spawns, fazer ele parar

    //para isso, vamos criar uma CoRoutine que vai fazer spawn de inimigos
    //de tempos em tempos

    //CoRoutines são basicamente semelhantes ao update, mas com a diferença que podemos
    //ter pausas no meio desse processo, o que queremos pro nosso spawner

    IEnumerator SpawningCycle()
    {
        int i = 0;

        while(i < Enemycounter)
        {
            i++;

            GameObject enemyToSpawn = GetRandomEnemyPrefab();

            Instantiate(enemyToSpawn, transform.position, transform.rotation);
            //instancia um prefab do inimigo na posição do nosso spawner
            
            yield return new WaitForSeconds(SpawnCoolDown);
            //espera alguns segundos definidos pelo SpawnCoolDown
        }
        SpawningCycleVar = null;
        //assim que chegar no final, a Coroutine vai parar, mas recomendam colocar um:
        //yield break;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (currentEnemyRates == null || currentEnemyRates.Count == 0) return null;

        int totalWeight = 0;
        foreach (var rate in currentEnemyRates)
        {
            totalWeight += rate.spawnWeight;
        }

        int randomPoint = Random.Range(0, totalWeight);

        foreach (var rate in currentEnemyRates)
        {
            if (randomPoint < rate.spawnWeight)
            {
                return rate.enemyPrefab;
            }
            randomPoint -= rate.spawnWeight;
        }
        return null; // Caso de fallback
    }
}
