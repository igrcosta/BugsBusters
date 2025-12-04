using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necessário para usar .Sum() e LINQ
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuração de Itens")]
    [Tooltip("Lista de Scriptable Objects de Power-Ups que este Spawner pode gerar.")]
    // Agora referenciamos o SO que armazena o Prefab, Peso e Tempo de Respawn.
    public List<PowerUpConfigSO> availablePowerUps;
    
    [Header("Pontos de Spawn")]
    [Tooltip("Lista dos Transformes que definem os locais de spawn.")]
    public Transform[] spawnPoints;

    // Armazena a Coroutine de respawn para cada ponto (usa o índice como chave)
    private Dictionary<int, Coroutine> respawnCoroutines;

    void Start()
    {
        // Inicializa o dicionário para rastrear as Coroutines por índice de spawn.
        respawnCoroutines = new Dictionary<int, Coroutine>();
        SpawnInitialItems();
    }

    void SpawnInitialItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Spawna um item aleatório inicial em cada ponto
            SpawnRandomItem(i);
        }
    }

    /// <summary>
    /// Escolhe e instancia um item aleatório com base nos pesos configurados
    /// no Scriptable Object, no ponto de spawn especificado.
    /// </summary>
    /// <param name="spawnIndex">O índice do ponto de spawn a ser usado.</param>
    private void SpawnRandomItem(int spawnIndex)
    {
        if (availablePowerUps == null || availablePowerUps.Count == 0)
        {
            Debug.LogError("[PowerUpSpawner] Lista de Power-Ups (Scriptable Objects) está vazia!");
            return;
        }
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length)
        {
            Debug.LogError($"[PowerUpSpawner] Índice de spawn inválido: {spawnIndex}");
            return;
        }

        // 1. Calcula o peso total de todos os Power-Ups disponíveis
        int totalWeight = availablePowerUps.Sum(config => config.spawnWeight);
        
        // 2. Escolhe um número aleatório dentro do range do peso total
        int randomPoint = Random.Range(0, totalWeight);

        PowerUpConfigSO selectedConfig = null;

        // 3. Seleção do Power-Up por peso (Weighted Random Selection)
        foreach (var config in availablePowerUps)
        {
            // Se o número aleatório for menor que o peso deste item, ele é o escolhido.
            if (randomPoint < config.spawnWeight)
            {
                selectedConfig = config;
                break;
            }
            // Caso contrário, subtrai o peso deste item e passa para o próximo.
            randomPoint -= config.spawnWeight;
        }

        if (selectedConfig != null && selectedConfig.itemPrefab != null)
        {
            // Instancia o item usando o Prefab do Scriptable Object
            GameObject newItem = Instantiate(
                selectedConfig.itemPrefab, 
                spawnPoints[spawnIndex].position, 
                spawnPoints[spawnIndex].rotation
            );

            // 4. Configura o script do item com informações do spawn point e respawn
var healingScript = newItem.GetComponent<HealingItemScript>();
var sprayScript = newItem.GetComponent<SprayPowerUp>();

if (healingScript != null)
{
    // Se for um item de cura, use o ConfigureSpawn dele
    healingScript.ConfigureSpawn(this, spawnIndex, selectedConfig.respawnTime);
}
else if (sprayScript != null)
{
    // Se for o power-up spray, use o ConfigureSpawn dele
    sprayScript.ConfigureSpawn(this, spawnIndex, selectedConfig.respawnTime);
}
else
{
    Debug.LogWarning($"Prefab {selectedConfig.itemPrefab.name} não tem um script de Power-Up reconhecido (HealingItemScript ou SprayPowerUp).");
}
        }
        else
        {
            Debug.LogWarning("[PowerUpSpawner] Falha ao selecionar/instanciar item aleatório. Verifique os Prefabs e Pesos no Scriptable Object.");
        }
    }

    /// <summary>
    /// Chamado pelo Power-Up após ser coletado. Inicia o timer para respawn.
    /// </summary>
    public void RespawnItem(int spawnIndex, float respawnTime)
    {
        // Interrompe qualquer Coroutine de respawn anterior que possa estar rodando neste índice
        if (respawnCoroutines.ContainsKey(spawnIndex) && respawnCoroutines[spawnIndex] != null)
        {
            StopCoroutine(respawnCoroutines[spawnIndex]);
        }
        
        // Inicia a nova Coroutine e a armazena no dicionário
        Coroutine respawnRoutine = StartCoroutine(WaitAndRespawn(spawnIndex, respawnTime));
        respawnCoroutines[spawnIndex] = respawnRoutine;
    }

    IEnumerator WaitAndRespawn(int spawnIndex, float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        
        // Spawna um novo item, que será aleatório de acordo com os pesos configurados
        SpawnRandomItem(spawnIndex);
        
        // Limpa a entrada no dicionário (a Coroutine terminou)
        respawnCoroutines.Remove(spawnIndex);
    }
}