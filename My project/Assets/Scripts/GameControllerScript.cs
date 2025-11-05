using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class GameControllerScript : MonoBehaviour
{
    [Header("Tudo sobre o Player")]

    public Player Player;
    //acessar o gameObject do tipo Player

    [Header("Materiais que Inimigos/Player usam")]

    public Material PlayerMatFirst, PlayerMatSecond;

    [Header ("Elementos dentro da MainScene")]

    public TimerScript Timer;

    public SpawnPointsControllerScripts EnemySpawnManagerScriptRef;

    public SafeZoneScript SafeZone;

    public GameUI GameUI;

    private bool IsPaused = false;
    private int ActualSceneIndex;


    public static GameControllerScript controller;

    private Coroutine ActualCoroutine;

    private int EnemiesNumber;

    private int pontos;

    private int totalEnemiesToKill;

    int inimigosMortos;

    private bool HasWaveStarted = false;
    private bool WinCondition = false;
    private bool IsGameActive = false;

    public WaveManager WaveManagerRef;

    public int[] ColorLogic = { 1, 2 };
    //agora, o game controller vai se responsabilizar pela lógica de cores durante o jogo, X é uma cor, Y é outra

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
            // O Player, Timer, SafeZone, etc., no Awake/Start, se conectam aqui.
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

            //lógica das waves aqui

            //esperar 3 segundos depois que a cena carregar, mostrando uma contagem regressiva na tela 
            //colocar texto para o cara se preparar, sem permitir o player interagir antes dessa contagem
            //fazer tudo que tiver que fazer antes do jogo iniciar

            //determinar onde vai spawnar inimigos (FEITO)
            //trazer safe zone pra posição original (ficar maior)
            //começar timer (FEITO)

            //tempo acabou
            //tem inimigos vivos? então tela de derrota

            //SE NÃO HOUVER MAIS INIMIGOS PARA SPAWNAR NESSA ONDA
            //Player vence, FICANDO SEM PODER INTERAGIR DENOVO. enquanto outra onda vai se iniciar
            //a ideia é que no futuro ele possa roletar entre as trocas de waves para novos tipos de armas
            //NÃO ESQUECER DISSO NO FUTURO

        }
    }

    IEnumerator FirstWaveRoutine()
    {
        while (Player == null || Timer == null || SafeZone == null || GameUI == null || EnemySpawnManagerScriptRef == null || WaveManagerRef == null)
        {
            yield return new WaitForEndOfFrame();
        }
        EnemySpawnManagerScriptRef.ResetSpawners();

        Player.DisableInputs();

        SafeZone.ResetSize();

        yield return new WaitForSeconds(3f);

        WaveManagerRef.StartNextWave();

        yield return null;
        //espera um frame pro jogo poder começar já com inimigos spawnados

        Timer.StartTimer();

        Player.EnableInputs();

        SafeZone.BeginShrinking();

        IsGameActive = true;
        //essa booleana serve pro jogo não perceber que tem 0 inimigos no início e já dar gameOver
    }

    IEnumerator NextWaveTransitionRoutine()
    {
        Debug.Log("Wave Finalizada! preparando para a próxima");

        yield return new WaitForSeconds(5f);

        EnemySpawnManagerScriptRef.ResetSpawners();
        SafeZone.ResetSize();

        WaveManagerRef.StartNextWave();

        Timer.StartTimer();
        SafeZone.BeginShrinking();
        IsGameActive = true;
    }

    public void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == false)
        {
            IsPaused = true;
            //aparecer tela de pause com um SetActive
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == true)
        {
            IsPaused = false;
            //aparecer tela de pause com um SetActive
            Time.timeScale = 1;
        }
    }

    private void FindingActualScene()
    {
        ActualSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //pegamos o index da cena atual
    }

    public void GameOver()
    {
        CleanUpGame();
        SceneManager.LoadScene(2);
    }

    public void CleanUpGame()
{
    //vou resetar todas as referências do gameController pra ele achar tudo denovo quando der retry
    controller = null;
    
    // Zera todas as variáveis de estado
    HasWaveStarted = false;
    WinCondition = false;
    IsGameActive = false;
    inimigosMortos = 0;

    //Destroy(gameObject);
    //tava dando erro de um retry pro outro, então vamo deixar o GameController se recriar
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

   
    




    //GameController vai servir para o seguinte:
    //gerenciar as waves, o que engloba:
    //Controlar o Timer
    //Gerenciar os spawns de inimigos 
    //trazer as condições de vitória e derrota (quando o tempo acaba, derrota aparece, falta verificar se o player matou todos os inimigos, que aí, vai ter a de vitória)
    //gerenciar quando começa ondas e termina outras
    //trocar entre cenas (Parcialmente)
    //garantir "pauses" na gameplay (FEITO)
}
