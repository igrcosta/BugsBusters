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

    public static int LastLevelSceneIndex = 1;

    public static void RegisterLevelCheckpoint(int sceneIndex)
{
    // Armazenamos o índice da cena para onde o Retry deve retornar.
    LastLevelSceneIndex = sceneIndex;
    Debug.Log($"[Checkpoint] Checkpoint de Retry registrado: Cena Index {sceneIndex}");
}

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
        if(scene.buildIndex == 1)
        {
            RegisterLevelCheckpoint(scene.buildIndex);
        }
        // Só faz a checagem se estiver na MainScene (índice 1) (Erika: MainScene agora é Level01, indice 2)
        if (scene.buildIndex == 2)
        {
            RegisterLevelCheckpoint(scene.buildIndex);

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
        if(scene.buildIndex == 3)
        {
            RegisterLevelCheckpoint(scene.buildIndex);
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

        if (cheatsEnabled && Input.GetKeyDown(KeyCode.K))
        {
            ForceNextWaveCheat();
        }
        
        if (cheatsEnabled && Input.GetKey(KeyCode.L))
        {
            StopAllCoroutines();

            SceneManager.LoadScene(3);
        }

        if (cheatsEnabled && Input.GetKey(KeyCode.J))
        {
            TutorialManager tutorial = FindObjectOfType<TutorialManager>();
        if (tutorial != null)
        {
            tutorial.SkipTutorialCheat();
        }
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
    if (IsGameActive && WaveManagerRef != null) 
    {
        Debug.LogWarning("CHEATER: Forçando transição INSTANTÂNEA para a próxima Wave (F1).");

        // 1. Para qualquer rotina de transição que possa estar rodando
        if (ActualCoroutine != null)
        {
            StopCoroutine(ActualCoroutine);
        }
        
        // 2. Desliga o jogo
        WinCondition = false;
        IsGameActive = false; 
        
        // 3. Inicia a rotina instantânea
        ActualCoroutine = StartCoroutine(InstantNextWaveRoutine());
    }
    else
    {
        Debug.Log("Cheat Ignorado: Jogo não está ativo ou WaveManager faltando.");
    }
}
    public void ForceStartFirstWave()
{
    // Esta função é chamada pelo cheat do Tutorial Manager.
    // Ela garante que a rotina da primeira wave seja iniciada mesmo que o OnSceneLoaded já tenha ocorrido.
    
    if (ActualCoroutine != null)
    {
        StopCoroutine(ActualCoroutine);
    }

    if (!HasWaveStarted)
    {
        HasWaveStarted = true;
        ActualCoroutine = StartCoroutine(FirstWaveRoutine());
        Debug.Log("GameController: Primeira Wave forçada pelo cheat de skip do tutorial.");
    }
    else
    {
        Debug.LogWarning("GameController: Tentativa de forçar a primeira wave, mas o jogo já está ativo.");
    }
}

    IEnumerator FirstWaveRoutine()
    {
        SetWarningPulse(true);

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

        SetWarningPulse(false);

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
    
    // 1. Limpeza
    DestroyAllActiveEnemies(); 
    
    // 2. Não zere inimigosMortos (mantendo a contagem cumulativa no HUD)
    // O contador de mortes fica com o valor atual (cumulativo).

    // 3. Avance o índice da wave e pegue a nova meta
    EnemySpawnManagerScriptRef.ResetSpawners(); 

    // CHAVE: Chamamos StartNextWave() apenas para: 
    // a) Fazer o WaveManager avançar o índice interno.
    // b) Obter a WaveConfig para a próxima wave.
    int currentWaveIndex = WaveManagerRef.StartNextWave(); 
    
    // 4. Calcula e DEFINE A NOVA META
    if (currentWaveIndex != -1)
    {
        totalEnemiesToKill = CalculateNewEnemyGoal(currentWaveIndex);
        
        // NOVO: A lógica do cheat muda. O GameController forçará o spawn do número exato.
        
        // O SpawnManager agora PRECISA DE UM MÉTODO para spawnar o total de inimigos.
        EnemySpawnManagerScriptRef.ForceInstantWaveSpawn(totalEnemiesToKill); // <--- CHAMA O NOVO MÉTODO
        
        // Se este método for chamado, a meta para a próxima wave já foi atendida 
        // em termos de 'inimigos para spawnar', mas não 'inimigos mortos'.
        
        Debug.Log($"Wave {currentWaveIndex + 1} forçada! Meta de Mortes: {totalEnemiesToKill}.");
    }
    else
    {
         // Se não há mais waves, encerra o jogo
         EndGame();
         yield break;
    }

    // 5. Ativa o jogo
    IsGameActive = true;
    
    yield return null;
}


    IEnumerator NextWaveTransitionRoutine()
    {
        Debug.Log("Wave Finalizada! Iniciando PAUSA de 5 segundos.");
        
        // ===================================================================
        // 1. LIMPEZA E PREPARAÇÃO DO ESTADO DE PAUSA (IMEDIATA)
        // ===================================================================
        
        // Zera o contador de mortes só para script
        inimigosMortos = 0; 

        SetWarningPulse(true);
        
        EnemySpawnManagerScriptRef.ResetSpawners();

        DestroyAllActiveEnemies();

        TeleportPlayerToInterwavePosition();

        // ===================================================================
        // 2. PAUSA/TRANSIÇÃO VISUAL: DURAÇÃO DO "MOMENTO DE RESPIRAR"
        // ===================================================================
        
        yield return new WaitForSeconds(5f); 

        SetWarningPulse(false);

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

    public static void ReturnToMenuAndCleanup()
{
    // O Singleton se autodestrói e carrega o Menu.
    if (controller != null)
    {
        controller.CleanUpGame();
        SceneManager.LoadScene(0);
    }
    else
    {
        SceneManager.LoadScene(0);
    }
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

    private void SetWarningPulse(bool isActive)
{
    // Usa a referência que foi registrada pelo GameUI.Awake()
    if (GameUI != null && GameUI.warningSymbolAnimator != null)
    {
        GameUI.warningSymbolAnimator.SetBool("IsPulsing", isActive);
        GameUI.warningSymbolAnimator.gameObject.SetActive(true);
        
        if (!isActive)
        {
            GameUI.warningSymbolAnimator.gameObject.SetActive(false);
        }
    }
    else
    {
        Debug.LogError("GameUI ou Animator de Warning é nulo. O alerta não pode ser exibido.");
        return; 
    }

    // Parte do Áudio
    if (GameUI.transitionAudioSource != null)
    {
        if (isActive)
        {
            if (!GameUI.transitionAudioSource.isPlaying)
            {
                GameUI.transitionAudioSource.Play();
            }
        }
        else
        {
            GameUI.transitionAudioSource.Stop();
        }
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