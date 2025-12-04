using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameControllerScript : MonoBehaviour
{
    [Header("CHEATS")]
    public bool cheatsEnabled = true;

    [Header("Tudo sobre o Player")]
    public Player Player; // Acessar o gameObject do tipo Player

    [Header ("Infos para transição de waves")]
    [SerializeField] Vector3 InterwavePosition = new Vector3(0.553f, 0.018f, -11.597f);

    [Header("Tudo sobre o BOSS")]
    public boss BossRef;

    [Header("Elementos dentro do Level01")]
    public SpawnPointsControllerScripts EnemySpawnManagerScriptRef;
    public GameUI GameUI;
 

    public static GameControllerScript controller;

    private Coroutine ActualCoroutine;

    private int totalEnemiesToKill;

    int inimigosMortos; // Contador de inimigos mortos

    private bool HasWaveStarted = false;
    private bool WinCondition = false;
    private bool IsGameActive = false;

    public WaveManager WaveManagerRef;

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
    // A checagem de IsGameActive já garante que estamos na cena de jogo e prontos.
    // O ActualSceneIndex é redundante se você usa OnSceneLoaded e IsGameActive.
    
    if (IsGameActive)
    {
        // Esta função chama WaveManagerRef.CheckWinCondition, que por sua vez,
        // chama WaveFinished() (a função que encerra a wave).
        CountingEnemies(); 

    }

    if (cheatsEnabled && Input.GetKeyDown(KeyCode.F1))
    {
        ForceNextWaveCheat();
    }
}

    public void DestroyAllActiveEnemies()
{
    // Encontra todos os GameObjects que têm a tag "Enemy"
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

    int enemiesDestroyed = 0;
    foreach (GameObject enemy in enemies)
    {
        // Certifica-se de que o objeto é destruído
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

        // 1. Para a coroutine atual (seja FirstWave, NextWave ou outra)
        if (ActualCoroutine != null)
        {
            StopCoroutine(ActualCoroutine);
        }
        
        // 2. Reseta estados de transição
        WinCondition = false;
        IsGameActive = false; // Desativa antes de iniciar a transição
        
        // 3. Inicia a nova rotina INSTANTÂNEA
        ActualCoroutine = StartCoroutine(InstantNextWaveRoutine());
    }
    else
    {
        Debug.Log("Cheat Ignorado: Jogo não está ativo.");
    }
}

    IEnumerator FirstWaveRoutine()
    {
        // Garante que todas as referências essenciais (exceto Player, que se registra) existam
        while (GameUI == null || EnemySpawnManagerScriptRef == null || WaveManagerRef == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        // CORREÇÃO: Garante que o Player se registrou no Singleton antes de continuar
        while (Player == null) 
        {
            Debug.LogWarning("GameController esperando o Player se registrar...");
            yield return new WaitForEndOfFrame();
        }

        EnemySpawnManagerScriptRef.ResetSpawners();

        Player.DisableInputs();

        yield return new WaitForSeconds(3f);

        WaveManagerRef.StartNextWave();

        yield return null;
        // espera um frame pro jogo poder começar já com inimigos spawnados

        Player.EnableInputs();

        IsGameActive = true;
    }

    IEnumerator InstantNextWaveRoutine()
{
    Debug.LogWarning("CHEATER: Transição Instantânea para a próxima Wave!");
    
    // NOVO: Destrói todos os inimigos existentes para limpar a cena
    DestroyAllActiveEnemies(); 

    // 1. Zera a contagem de inimigos mortos para a nova wave
    // ... (restante do código: inimigosMortos = 0, GameUI, etc.) ...
    inimigosMortos = 0; 
    GameUI.AlterarInimigosMortosnaHUD(inimigosMortos);

    // 2. Reseta Spawners
    EnemySpawnManagerScriptRef.ResetSpawners();

    // 3. Inicia a Próxima Wave
    WaveManagerRef.StartNextWave();

    // 4. Ativa elementos do jogo
    IsGameActive = true;
    
    yield return null; // Finaliza a coroutine
}


    IEnumerator NextWaveTransitionRoutine()
{
    Debug.Log("Wave Finalizada! Iniciando PAUSA de 5 segundos.");
    
    // ===================================================================
    // 1. LIMPEZA E PREPARAÇÃO DO ESTADO DE PAUSA (IMEDIATA)
    // ===================================================================
    
    // Zera o contador de mortes e o HUD
    inimigosMortos = 0; 
    GameUI.AlterarInimigosMortosnaHUD(inimigosMortos);
    
    // Reseta a meta de inimigos (a nova wave irá recontar)
    ResetTotalEnemiesToKill(); 
    
    // Desativa e reseta os spawners da wave anterior (para que nada spawne)
    EnemySpawnManagerScriptRef.ResetSpawners();

    // NOVO: Exclui todos os inimigos restantes da cena
    DestroyAllActiveEnemies();

    // NOVO: Teleporta o Player para a posição de "respiro"
    TeleportPlayerToInterwavePosition();

    // ===================================================================
    // 2. PAUSA/TRANSIÇÃO VISUAL: DURAÇÃO DO "MOMENTO DE RESPIRAR"
    // ===================================================================
    
    // Agora o player tem 5 segundos de pausa. O Timer não está rodando, mas o tempo passa.
    yield return new WaitForSeconds(5f); 

    // O Timer.ResetTimer() não é necessário aqui, pois você quer manter o tempo total.
    // Se você usa o Timer para o tempo *restante* de jogo, ele deve ser reiniciado/continuado.
    // Vamos assumir que você quer continuar o tempo de jogo (o TimerScript deve ter sido pausado em WaveFinished()).
    
    // ===================================================================
    // 3. INICIA A PRÓXIMA WAVE E ATIVA O JOGO
    // ===================================================================

    // Inicia a próxima wave
    WaveManagerRef.StartNextWave();

    // NOVO: RestartTimer() é mais adequado do que StartTimer() aqui, se o seu TimerScript
    // tiver uma lógica de 'continuação' após uma pausa, mas StartTimer() funciona para 
    // reiniciar a contagem. Se o Timer já estava rodando, ele continua a rodar.
    IsGameActive = true;
    
    Debug.Log("Fim da Pausa. Wave seguinte iniciada!");
}

    public void GameOver()
    {
        CleanUpGame();
        SceneManager.LoadScene(2);
    }

    public void EndGame()
{
    // Transição para a cena de Vitória/Créditos (Índice 3) COM FADE.
    
    // 2. CRÍTICO: INICIA A COROUTINE NO SceneFader.Instance
    if (SceneFader.Instance != null)
    {
        // O Fader agora roda a Coroutine em seu próprio objeto persistente.
        SceneFader.Instance.StartCoroutine(SceneFader.Instance.FadeOutAndLoadScene(3));
    }
    else
    {
        Debug.LogError("ERRO FATAL: SceneFader.Instance é NULO! Carregando cena diretamente (SEM FADE).");
        SceneManager.LoadScene(3);
    }
    
    // 3. LIMPA O JOGO APÓS A COROUTINE SER INICIADA (Garante que o Fader sobreviva)
    CleanUpGame();
}



    public void CleanUpGame()
    {
        // Destruição do singleton para que ele se recrie limpo ao carregar a cena inicial
        Destroy(gameObject);
        
        // Essas linhas são redundantes se você destruir o GameObject, mas servem como garantia
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
        // Para todas as operações e transiciona para a cena 4 (Game Over por Tempo)
        CleanUpGame();
        // Não usamos o Fade, apenas trocamos a cena (Assumindo que 4 é a tela de Game Over)
        SceneManager.LoadScene(4);
    }
}

    public void WaveFinished()
{
    // 1. DESATIVA O JOGO E LIMPA ESTADOS (IMEDIATAMENTE)
    IsGameActive = false;
    WinCondition = false;

    // 3. PARA A COROUTINE ATUAL (A que está gerenciando a wave - First/NextWaveRoutine)
    if (ActualCoroutine != null)
    {
        StopCoroutine(ActualCoroutine);
        ActualCoroutine = null; // Limpa a referência
    }

    // 4. DECISÃO DE TRANSIÇÃO
    if (WaveManagerRef != null && WaveManagerRef.IsFinalWaveCompleted()) 
    {
        Debug.Log("Última wave finalizada! Iniciando EndGame (Com Fade para Cena 3).");
        EndGame(); 
    }
    else
    {
        Debug.Log("Wave finalizada! Iniciando transição para a próxima wave...");
        // Inicia a nova rotina de transição
        ActualCoroutine = StartCoroutine(NextWaveTransitionRoutine());
    }
}

    private void CountingEnemies()
{
    // Não precisa mais checar IsGameActive aqui, pois o Update já faz isso.
    if (WaveManagerRef != null)
    {
        // ...checa se a wave acabou, passando SOMENTE o contador de mortos.
        // O WaveManager agora acessa a meta real (totalEnemiesToKill) no GC.
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

    public void RegisterNewEnemySpawn()
{
    totalEnemiesToKill++;
    // Você pode querer atualizar o HUD com a meta, se ele mostrar isso.
    // Ex: GameUI.AlterarMetaInimigos(totalEnemiesToKill);
    Debug.Log("Inimigo spawnado registrado. Meta atual: " + totalEnemiesToKill);
}

public void ResetTotalEnemiesToKill()
{
    totalEnemiesToKill = 0;
    // Opcional: GameUI.AlterarMetaInimigos(0);
    Debug.Log("Meta de inimigos zerada para a próxima wave.");
}

public int GetTotalEnemiesToKill()
{
    return totalEnemiesToKill;
}

    //Função para quando o player matar o boss
    public void TheGameIsOver()
    {
        SceneManager.LoadScene(5);
    }

    public void TeleportPlayerToInterwavePosition()
{
    if (Player != null)
    {
        // Define a posição diretamente
        Player.transform.position = InterwavePosition;
        Debug.Log("Player teletransportado para posição de transição.");
    }
    else
    {
        Debug.LogWarning("Player é nulo. Não foi possível teletransportar.");
    }
}
}
