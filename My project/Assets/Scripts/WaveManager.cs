using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Necessário para List

public class WaveManager : MonoBehaviour
{
    [Header("Configuração de Waves")]
    [SerializeField]
    private List<WaveConfig> waves;

    [Header("Referências")]
    public SpawnPointsControllerScripts spawnController;

    private int currentWaveIndex = 0;

    // Método chamado pelo GameController para iniciar a próxima wave
    public void StartNextWave()
    {
        if (spawnController == null)
        {
            Debug.LogError("WaveManager: Referência ao SpawnPointsController está faltando.");
            return;
        }

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("Todas as waves foram concluídas. Fim de jogo!");
            // TODO: Chamar o método de Vitória Final no GameController
            return;
        }

        WaveConfig currentWave = waves[currentWaveIndex];
        
        // NOVO: Chamamos a nova função de ativação do SpawnController
        spawnController.Activation(
            currentWave.numberOfSpawnersToActivate, 
            currentWave.spawnAttemptsPerSpawner, 
            currentWave.enemyRates
        );
        
        Debug.Log($"Wave {currentWaveIndex + 1} iniciada!");
        currentWaveIndex++;
    }

    public void CheckWinCondition(int totalEnemiesKilled, int totalEnemiesToKill)
    {
        if (totalEnemiesKilled >= totalEnemiesToKill && totalEnemiesToKill > 0)
        {
            // Se esta wave terminou, informa ao GameController.
            GameControllerScript.controller.WaveFinished();
        }
    }
}