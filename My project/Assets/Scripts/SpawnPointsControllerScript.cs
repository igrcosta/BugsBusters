using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class SpawnPointsControllerScripts : MonoBehaviour
{
    GameObject[] SpawnPoints;
    //criamos um array com os spawners, ele vai ser responsável por aleatorizar quais spawners vão estar ativos

    private GameObject SpawnerSelected;

    [Header("Parâmetros Internos do Spawner")]
    private bool isSpawningGradually = false; // Flag para controlar o loop normal

    // Armazena as taxas de spawn da wave atual para uso do cheat.
private List<EnemySpawnRate> currentEnemyRates; 
// Lista dos spawners ativos na wave atual, para que possamos spawnar neles.
private List<GameObject> activeSpawners;

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

    private GameObject SelectEnemyPrefabToSpawn()
{
    if (currentEnemyRates == null || currentEnemyRates.Count == 0)
    {
        Debug.LogError("currentEnemyRates está nulo ou vazio. Não é possível spawnar inimigo!");
        return null;
    }

    // Calcula o peso total (soma de todas as taxas)
    float totalWeight = 0f;
    foreach (var rate in currentEnemyRates)
    {
       totalWeight += rate.spawnWeight; 
    }

    if (totalWeight <= 0) return null;

    // Sorteia um valor aleatório
    float randomPoint = Random.Range(0f, totalWeight);

    // Itera para encontrar o inimigo correspondente ao ponto sorteado
    foreach (var rate in currentEnemyRates)
    {
        if (randomPoint < rate.spawnWeight)
        {
            // Assume que EnemySpawnRate tem uma propriedade 'enemyPrefab'
            return rate.enemyPrefab; 
        }
        randomPoint -= rate.spawnWeight;
    }
    
    return null; // Caso de erro
}

    public void ResetSpawners()
    {
        for(int u = 0; u < SpawnPoints.Length;u++)
        {
            GameObject actualSpawner = SpawnPoints[u];
            actualSpawner.SetActive(false);
        }
    }

    public void StopSpawning()
{
    isSpawningGradually = false;
    // Se você usa uma Coroutine para o spawn gradual, pare ela aqui.
    // Ex: if (spawnRoutine != null) StopCoroutine(spawnRoutine);
}

public void ForceInstantWaveSpawn(int countToSpawn)
{
    Debug.Log($"[SPAWN CHEAT] Forçando spawn instantâneo de {countToSpawn} inimigos.");
    
    // 1. Desliga o spawn gradual do Controller (caso ele estivesse ativo)
    StopSpawning(); 
    
    // 2. Desliga os scripts Spawner individuais para garantir que eles não criem mais nada.
    if (activeSpawners != null)
    {
        foreach (GameObject spawnerObject in activeSpawners)
        {
            Spawner SpawnerScript = spawnerObject.GetComponent<Spawner>();
            if (SpawnerScript != null)
            {
                // CHAMA O NOVO MÉTODO NO SCRIPT INDIVIDUAL
                SpawnerScript.StopSpawning(); 
            }
        }
    }

    // Verifica se há spawners e rates
    if (activeSpawners == null || activeSpawners.Count == 0 || currentEnemyRates == null)
    {
        Debug.LogError("[SPAWN CHEAT] Não há Spawners ativos ou Enemy Rates definidos. Impossível forçar o spawn.");
        return;
    }

    // 3. Itera para spawnar o número exato de inimigos
    for (int i = 0; i < countToSpawn; i++)
    {
        // A. Escolhe o prefab do inimigo (usa a função corrigida)
        GameObject enemyPrefab = SelectEnemyPrefabToSpawn();
        
        if (enemyPrefab != null)
        {
            // B. Escolhe um SpawnPoint ativo aleatório
            int randomIndex = Random.Range(0, activeSpawners.Count);
            GameObject selectedSpawner = activeSpawners[randomIndex];
            
            // C. Instancia
            Instantiate(enemyPrefab, selectedSpawner.transform.position, selectedSpawner.transform.rotation);

            // D. CRÍTICO: Registra o spawn no GameController (assim ele sabe que a meta de spawn foi atendida)
            // É importante que o GameController saiba que esses inimigos FORAM SPAWNADOS, 
            // mesmo que o Spawner individual não tenha feito isso no ciclo gradual.
            if (GameControllerScript.controller != null)
            {
                 GameControllerScript.controller.RegisterNewEnemySpawn();
            }
        }
    }
}


    public void Activation(int numSpawnersToActivate, int spawnAttempts, List<EnemySpawnRate> enemyRates)
    {

        currentEnemyRates = enemyRates;

        activeSpawners = new List<GameObject>();

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

    activeSpawners.Add(selectedSpawner);

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


    SpawnerScript.StartSpawning();
}

       
    }
}
