using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class GameControllerScript : MonoBehaviour
{
    [Header("Tudo sobre o Player")]
    public Player Player; // Acessar o gameObject do tipo Player

    [Header("Materiais que Inimigos/Player usam")]
    public Material PlayerMatFirst, PlayerMatSecond;

    [Header("Elementos dentro da MainScene")]
    public TimerScript Timer;
    public SpawnPointsControllerScripts EnemySpawnManagerScriptRef;
    public SafeZoneScript SafeZone;
    public GameUI GameUI;

    private bool IsPaused = false;
    private int ActualSceneIndex;

    public static GameControllerScript controller;

    private Coroutine ActualCoroutine;

    private int EnemiesNumber; // Não está sendo usado
    private int pontos; // Não está sendo usado
    private int totalEnemiesToKill;

    int inimigosMortos; // Contador de inimigos mortos

    private bool HasWaveStarted = false;
    private bool WinCondition = false;
    private bool IsGameActive = false;

    public WaveManager WaveManagerRef;

    public int[] ColorLogic = { 1, 0 }; // Corrigido para 0 e 1, seguindo a lógica do Player.cs

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
        // Só faz a checagem se estiver na MainScene (índice 1)
        if (scene.buildIndex == 1)
        {
            // Garantimos que a wave só tenta começar uma vez.
            if (!HasWaveStarted)
            {
                HasWaveStarted = true;
                SafeZone.DisableAndReset();
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
        FindingActualScene();

        if (ActualSceneIndex == 1)
        {
            Pause();

            if (IsGameActive)
            {
                CountingEnemies();

                if (WinCondition)
                {
                    if (ActualCoroutine != null)
                    {
                        StopCoroutine(ActualCoroutine);
                        Timer.StopTimer();

                        GameOver();
                        WinCondition = false;
                        IsGameActive = false;
                    }
                }
            }
        }
    }

    IEnumerator FirstWaveRoutine()
    {
        // Garante que todas as referências essenciais (exceto Player, que se registra) existam
        while (Timer == null || SafeZone == null || GameUI == null || EnemySpawnManagerScriptRef == null || WaveManagerRef == null)
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

        Timer.StartTimer();

        Player.EnableInputs();

        IsGameActive = true;
    }

    IEnumerator NextWaveTransitionRoutine()
    {
        Debug.Log("Wave Finalizada! preparando para a próxima");

        // CORREÇÃO CRÍTICA: Zera a contagem de inimigos mortos para a nova wave
        inimigosMortos = 0; 
        GameUI.AlterarInimigosMortosnaHUD(inimigosMortos);
        // Fim da Correção

        yield return new WaitForSeconds(5f);

        EnemySpawnManagerScriptRef.ResetSpawners();
        Timer.ResetTimer();

        WaveManagerRef.StartNextWave();

        SafeZone.ActivateAndBeginShrinking();
        Timer.StartTimer();
        IsGameActive = true;
    }

    public void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == false)
        {
            IsPaused = true;
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == true)
        {
            IsPaused = false;
            Time.timeScale = 1;
        }
    }

    private void FindingActualScene()
    {
        ActualSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    public void GameOver()
    {
        CleanUpGame();
        SceneManager.LoadScene(2);
    }

    public void EndGame()
    {
        //SceneManager.LoadScene();
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

    public void WaveFinished()
    {
        if (ActualCoroutine != null)
        {
            StopCoroutine(ActualCoroutine);
            Timer.StopTimer();

            WinCondition = false;
            IsGameActive = false;

            ActualCoroutine = StartCoroutine(NextWaveTransitionRoutine());
        }
    }

    private void CountingEnemies()
    {
        // Se o jogo está ativo e a contagem total foi definida...
        if (IsGameActive && totalEnemiesToKill > 0)
        {
            // ...checa se a wave acabou.
            WaveManagerRef.CheckWinCondition(inimigosMortos, totalEnemiesToKill);
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

    public void SetTotalEnemiesToKill(int total)
    {
        totalEnemiesToKill = total;
        Debug.Log("Meta de inimigos para matar nesta wave: " + totalEnemiesToKill);
    }
}