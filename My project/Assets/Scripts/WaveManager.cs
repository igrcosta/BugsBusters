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
            Debug.Log("Todas as waves foram concluídas neste nível. O GameController cuidará da transição final.");
            return;
        }
        
        // Restaurando a lógica de configuração e ativação da wave.
        WaveConfig currentWave = waves[currentWaveIndex];
        
        // Garante que a lista de inimigos está configurada antes de passar.
        if (currentWave.enemyRates == null || currentWave.enemyRates.Count == 0)
        {
            Debug.LogError($"WaveManager: Wave {currentWaveIndex + 1} não tem EnemyRates configurados. Verifique o Inspector!");
            return;
        }

        // 1. Ativa o controlador de spawn com as configurações da wave atual.
        spawnController.Activation(
            currentWave.numberOfSpawnersToActivate, 
            currentWave.spawnAttemptsPerSpawner, 
            currentWave.enemyRates
        );
        
        Debug.Log($"Wave {currentWaveIndex + 1} iniciada! Total de Waves: {waves.Count}.");
        
        // 2. Incrementa o índice para preparar a próxima chamada.
        currentWaveIndex++;
        // -------------------------------------------------------------------
    }

    public void CheckWinCondition(int totalEnemiesKilled) // Remove 'totalEnemiesToKill'
{
    // A meta (totalEnemiesToKill) será acessada diretamente do GameController.
    int totalEnemiesToKill = GameControllerScript.controller.GetTotalEnemiesToKill(); 
    
    if (totalEnemiesKilled >= totalEnemiesToKill && totalEnemiesToKill > 0)
    {
        // Se esta wave terminou, informa ao GameController.
        GameControllerScript.controller.WaveFinished();
    }
}

    private void Start()
    {
        // Garante que o GameController exista antes de tentar registrar.
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.WaveManagerRef = this;
        }
        else
        {
            Debug.LogError("GameController não encontrado/registrado. O WaveManager não pode se referenciar.");
        }
    }

    public bool IsFinalWaveCompleted()
    {
        // Se o índice atual (após o incremento em StartNextWave) for >= ao total, a última wave terminou.
        return currentWaveIndex >= waves.Count;
    }
}