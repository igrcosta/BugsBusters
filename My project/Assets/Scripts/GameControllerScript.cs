using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameControllerScript : MonoBehaviour
{
    // ... (Usings omitidos para brevidade) ...

    [Header("CHEATS")]
    public bool cheatsEnabled = true;

    [Header("Tudo sobre o Player")]
    public Player Player; // Acessar o gameObject do tipo Player

    [Header ("Infos para transição de waves")]
    [SerializeField] Vector3 InterwavePosition = new Vector3(0.553f, 0.018f, -11.597f);

    [Header("Metas de Inimigos")]
    [Tooltip("A meta inicial de inimigos a ser morta na Wave 1.")]
    [SerializeField] private int initialEnemyKillGoal = 20; // Valor X

    [Tooltip("O incremento de inimigos a cada nova wave (ex: +10).")]
    [SerializeField] private int enemyGoalIncrement = 10; // Valor do incremento (+10)

    [Header("Tudo sobre o BOSS")]
    public boss BossRef;

    [Header("Elementos dentro do Level01")]
    public SpawnPointsControllerScripts EnemySpawnManagerScriptRef;
    public GameUI GameUI;
 

    public static GameControllerScript controller;

    private Coroutine ActualCoroutine;

    private int totalEnemiesToKill; // Meta da wave atual
    int inimigosMortos; // Contador de inimigos mortos

    private bool HasWaveStarted = false;
    private bool WinCondition = false;
    private bool IsGameActive = false;

    public WaveManager WaveManagerRef;

    // ... (OnEnable, OnDisable, OnSceneLoaded, Awake, Singleton omitidos para brevidade) ...

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Só faz a checagem se estiver na MainScene (índice 1) (Erika: MainScene agora é Level01, indice 2)
        if (scene.buildIndex == 2)
        {
            // Garantimos que a wave só tenta começar uma vez.
            if (!HasWaveStarted)
            {
                HasWaveStarted = true;
                ActualCoroutine = StartCoroutine(FirstWaveRoutine());
            }
        }
        else
        {
            // Quando em HomeScene ou DeathScene, reseta
            HasWaveStarted = false;
        }
    }

    private void Awake()
    {
        Singleton();
    }

    private void Singleton()
    {
        if (controller == null)
        {
            controller = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        if (IsGameActive)
        {
            CountingEnemies(); 
        }

        if (cheatsEnabled && Input.GetKeyDown(KeyCode.F1))
        {
            ForceNextWaveCheat();
        }
    }

    /// <summary>
    /// Calcula a nova meta de inimigos com base no índice da wave.
    /// Meta: InitialGoal + (Index * Increment)
    /// </summary>
    private int CalculateNewEnemyGoal(int waveIndex)
    {
        // Wave 1 (Index 0): 20 + (0 * 10) = 20
        // Wave 2 (Index 1): 20 + (1 * 10) = 30
        int goal = initialEnemyKillGoal + (waveIndex * enemyGoalIncrement);
        return goal;
    }

    // ... (DestroyAllActiveEnemies, ForceNextWaveCheat, InstantNextWaveRoutine omitidos para brevidade) ...
    
    public void DestroyAllActiveEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int enemiesDestroyed = 0;
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
            enemiesDestroyed++;
        }
        Debug.Log($"Destruiu {enemiesDestroyed} inimigos ativos na cena.");
    }

    public void ForceNextWaveCheat()
    {
        if (IsGameActive)
        {
            Debug.LogWarning("CHEATER: Forçando transição INSTANTÂNEA para a próxima Wave (F1).");

            if (ActualCoroutine != null)
            {
                StopCoroutine(ActualCoroutine);
            }
            
            WinCondition = false;
            IsGameActive = false; 
            
            ActualCoroutine = StartCoroutine(InstantNextWaveRoutine());
        }
        else
        {
            Debug.Log("Cheat Ignorado: Jogo não está ativo.");
        }
    }

    IEnumerator FirstWaveRoutine()
    {
        while (GameUI == null || EnemySpawnManagerScriptRef == null || WaveManagerRef == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (Player == null) 
        {
            Debug.LogWarning("GameController esperando o Player se registrar...");
            yield return new WaitForEndOfFrame();
        }

        EnemySpawnManagerScriptRef.ResetSpawners();

        Player.DisableInputs();

        yield return new WaitForSeconds(3f);

        // NOVO: Pega o índice e calcula a meta
        int currentWaveIndex = WaveManagerRef.StartNextWave();
        if (currentWaveIndex != -1) // Se a wave foi iniciada com sucesso
        {
            totalEnemiesToKill = CalculateNewEnemyGoal(currentWaveIndex); // <--- DEFINE A META
            // Você pode querer chamar GameUI.AlterarMetaDeInimigos(totalEnemiesToKill); aqui
        }

        yield return null;
        
        Player.EnableInputs();

        IsGameActive = true;
    }

    IEnumerator InstantNextWaveRoutine()
    {
        Debug.LogWarning("CHEATER: Transição Instantânea para a próxima Wave!");
        
        DestroyAllActiveEnemies(); 

        inimigosMortos = 0; 
        GameUI.AlterarInimigosMortosnaHUD(inimigosMortos);

        EnemySpawnManagerScriptRef.ResetSpawners();

        // NOVO: Pega o índice e calcula a meta
        int currentWaveIndex = WaveManagerRef.StartNextWave();
        if (currentWaveIndex != -1)
        {
            totalEnemiesToKill = CalculateNewEnemyGoal(currentWaveIndex); // <--- DEFINE A META
        }

        IsGameActive = true;
        
        yield return null;
    }


    IEnumerator NextWaveTransitionRoutine()
    {
        Debug.Log("Wave Finalizada! Iniciando PAUSA de 5 segundos.");
        
        // ===================================================================
        // 1. LIMPEZA E PREPARAÇÃO DO ESTADO DE PAUSA (IMEDIATA)
        // ===================================================================
        
        // Zera o contador de mortes e o HUD
        inimigosMortos = 0; 
        
        EnemySpawnManagerScriptRef.ResetSpawners();

        DestroyAllActiveEnemies();

        TeleportPlayerToInterwavePosition();

        // ===================================================================
        // 2. PAUSA/TRANSIÇÃO VISUAL: DURAÇÃO DO "MOMENTO DE RESPIRAR"
        // ===================================================================
        
        yield return new WaitForSeconds(5f); 

        // ===================================================================
        // 3. INICIA A PRÓXIMA WAVE E ATIVA O JOGO
        // ===================================================================

        // NOVO: Pega o índice e calcula a meta
        int currentWaveIndex = WaveManagerRef.StartNextWave();
        if (currentWaveIndex != -1)
        {
            totalEnemiesToKill = CalculateNewEnemyGoal(currentWaveIndex); // <--- DEFINE A META
            // Você pode querer chamar GameUI.AlterarMetaDeInimigos(totalEnemiesToKill); aqui
        }

        IsGameActive = true;
        
        Debug.Log("Fim da Pausa. Wave seguinte iniciada!");
    }

    // ... (GameOver, EndGame, CleanUpGame, TimeExpired omitidos para brevidade) ...

    public void GameOver()
    {
        CleanUpGame();
        SceneManager.LoadScene(2);
    }

    public void EndGame()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.StartCoroutine(SceneFader.Instance.FadeOutAndLoadScene(3));
        }
        else
        {
            Debug.LogError("ERRO FATAL: SceneFader.Instance é NULO! Carregando cena diretamente (SEM FADE).");
            SceneManager.LoadScene(3);
        }
        
        CleanUpGame();
    }

    public void CleanUpGame()
    {
        Destroy(gameObject);
        controller = null; 
        HasWaveStarted = false;
        WinCondition = false;
        IsGameActive = false;
        inimigosMortos = 0;
    }

    public void TimeExpired()
    {
        if (IsGameActive)
        {
            Debug.Log("Tempo esgotado! Game Over por tempo.");
            CleanUpGame();
            SceneManager.LoadScene(4);
        }
    }
    
    public void WaveFinished()
    {
        IsGameActive = false;
        WinCondition = false;

        if (ActualCoroutine != null)
        {
            StopCoroutine(ActualCoroutine);
            ActualCoroutine = null;
        }

        if (WaveManagerRef != null && WaveManagerRef.IsFinalWaveCompleted()) 
        {
            Debug.Log("Última wave finalizada! Iniciando EndGame (Com Fade para Cena 3).");
            EndGame(); 
        }
        else
        {
            Debug.Log("Wave finalizada! Iniciando transição para a próxima wave...");
            ActualCoroutine = StartCoroutine(NextWaveTransitionRoutine());
        }
    }

    private void CountingEnemies()
    {
        if (WaveManagerRef != null)
        {
            WaveManagerRef.CheckWinCondition(inimigosMortos); 
        }
    }

    public void AumentarNumerodeInimigosMortos()
    {
        inimigosMortos++;
        GameUI.AlterarInimigosMortosnaHUD(inimigosMortos);
    }
    
    public void RegisterSpawnManager(SpawnPointsControllerScripts manager)
    {
        EnemySpawnManagerScriptRef = manager;
        Debug.Log("Spawner registrado no GameController.");
    }

    // Método não é mais usado para a meta, mas pode ser útil para contadores internos ou debug.
    public void RegisterNewEnemySpawn()
    {
        // totalEnemiesToKill++; // Removido, pois a meta agora é calculada e fixa
        // Debug.Log("Inimigo spawnado registrado. Meta atual: " + totalEnemiesToKill);
    }

    public void ResetTotalEnemiesToKill()
    {
        totalEnemiesToKill = 0;
        Debug.Log("Meta de inimigos zerada para a próxima wave.");
    }

    public int GetTotalEnemiesToKill()
    {
        return totalEnemiesToKill;
    }

    public void TheGameIsOver()
    {
        SceneManager.LoadScene(5);
    }

    public void TeleportPlayerToInterwavePosition()
    {
        if (Player != null)
        {
            Player.transform.position = InterwavePosition;
            Debug.Log("Player teletransportado para posição de transição.");
        }
        else
        {
            Debug.LogWarning("Player é nulo. Não foi possível teletransportar.");
        }
    }
}