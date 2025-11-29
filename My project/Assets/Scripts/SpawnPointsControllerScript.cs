using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class SpawnPointsControllerScripts : MonoBehaviour
{
    GameObject[] SpawnPoints;
    //criamos um array com os spawners, ele vai ser responsável por aleatorizar quais spawners vão estar ativos

    private GameObject SpawnerSelected;

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        SpawnPoints = new GameObject[transform.childCount];
        for (int i=0;i<transform.childCount;i++)
        {
            SpawnPoints[i] = transform.GetChild(i).gameObject;
        }
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.RegisterSpawnManager(this);
            }
            else
            {
                Debug.LogError("GameController não encontrado na cena!");
                }

        Debug.Log($"[SpawnManager] Spawners encontrados na cena: {SpawnPoints.Length}");
    }

    public void ResetSpawners()
    {
        for(int u = 0; u < SpawnPoints.Length;u++)
        {
            GameObject actualSpawner = SpawnPoints[u];
            actualSpawner.SetActive(false);
        }
    }

    public void Activation(int numSpawnersToActivate, int spawnAttempts, List<EnemySpawnRate> enemyRates)
    {
        int currentWaveTotalEnemies = 0;
        //int para contar o total de inimigos que vão spawnar, assim o player só ganha se matar esse número

        // Garante que não tentamos ativar mais spawners do que existem
        int spawnersToUse = Mathf.Min(numSpawnersToActivate, SpawnPoints.Length);

        ResetSpawners();

        // Cria uma lista de índices disponíveis (0, 1, 2, 3, ...)
        List<int> availableIndices = Enumerable.Range(0, SpawnPoints.Length).ToList();
        
        // Embaralha a lista de índices (Fisher-Yates simplificado)
        // Isso garante que os 'spawnersToUse' primeiros que pegarmos serão únicos.
        for (int i = 0; i < availableIndices.Count; i++)
        {
            int temp = availableIndices[i];
            int randomIndex = Random.Range(i, availableIndices.Count);
            availableIndices[i] = availableIndices[randomIndex];
            availableIndices[randomIndex] = temp;
        }

        // ATIVAÇÃO E CONFIGURAÇÃO
        for (int i = 0; i < spawnersToUse; i++)
{
    // Pega o índice aleatório e único
    int spawnerIndex = availableIndices[i];
    
    GameObject selectedSpawner = SpawnPoints[spawnerIndex];

    // Ativa o Spawner
    selectedSpawner.gameObject.SetActive(true);

    // Configura o Script do Spawner
    Spawner SpawnerScript = selectedSpawner.GetComponent<Spawner>();
    
    // CRÍTICO: VERIFICA SE O SCRIPT FOI ENCONTRADO
    if (SpawnerScript == null)
    {
        // Se o script Spawner não estiver no objeto, loga o erro e pula para o próximo spawner no loop.
        Debug.LogError($"[SPAWNPOINTS] O Spawner do índice {spawnerIndex} ({selectedSpawner.name}) NÃO possui o componente Spawner. Pulando.");
        // Não conta inimigos e nem inicia o spawn.
        continue; 
    }

    // Passa a lista de inimigos e as tentativas de spawn
    SpawnerScript.SetupSpawner(spawnAttempts, enemyRates);

    // CRÍTICO: Contagem segura
    currentWaveTotalEnemies += spawnAttempts;

    SpawnerScript.StartSpawning();
}

        // Atualiza a contagem de inimigos
        GameControllerScript.controller.SetTotalEnemiesToKill(currentWaveTotalEnemies);

    }
}
