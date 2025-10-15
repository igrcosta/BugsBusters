using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class GameControllerScript : MonoBehaviour
{

    private Player Player;
    //acessar o gameObject do tipo Player

    private bool IsPaused = false;

    private int ActualSceneIndex;

    public TimerScript Timer;

    public SpawnPointsControllerScripts EnemySpawnManagerScriptRef;

    public SafeZoneScript SafeZone;

    public PlayerInput PlayerInputs;

    public static GameControllerScript controller;

    private Coroutine ActualCoroutine;
    private bool HasWaveStarted = false;


    public void RegisterSpawnManager(SpawnPointsControllerScripts manager)
    {
        EnemySpawnManagerScriptRef = manager;
        Debug.Log("Spawner registrado no GameController.");
    }
    
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;


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

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    // Verifica se a cena carregada é a cena do jogo (Index 1)
    if (scene.buildIndex == 1) 
    {
        InitializeGameSceneObjects();
    }
    else // Para outras cenas (Home, GameOver), garante que o estado é limpo
    {
        // Garante que a Coroutine da wave não comece
        HasWaveStarted = false; 

        // Limpa referências que só existem no jogo (opcional, mas bom)
        Player = null;
        Timer = null;
        SafeZone = null;
        // O Spawner se auto-registra.
    }
    }

    private void InitializeGameSceneObjects()
    {
        //Procura pelo Player, Timer, Spawn Manager e SafeZone APENAS QUANDO A CENA DO JOGO CARREGA.
    if (Player == null)
    {
        Player = FindFirstObjectByType<Player>();
        if (Player == null) Debug.LogError("Player não encontrado na cena!");
    }
    
    if (Timer == null)
    {
        Timer = FindFirstObjectByType<TimerScript>(); 
        if (Timer == null) Debug.LogError("Timer não encontrado na cena!");
    }

    if (EnemySpawnManagerScriptRef == null)
    {
        EnemySpawnManagerScriptRef = FindFirstObjectByType<SpawnPointsControllerScripts>();
        if (EnemySpawnManagerScriptRef == null) Debug.LogError("SpawnerManager não encontrado na cena!");
    }

    if (SafeZone == null)
    {
        SafeZone = FindFirstObjectByType<SafeZoneScript>();
        if(SafeZone == null) Debug.LogError("Não achei a SafeZone");
    }

    // Só inicia a primeira wave SE estiver na cena do jogo e a wave ainda não tiver começado.
    if (!HasWaveStarted)
    {
        ActualCoroutine = StartCoroutine(FirstWaveRoutine());
    }

    }

    void Update()
    {
        FindingActualScene();

        if(ActualSceneIndex == 1)
        {
            Pause();

            if (!HasWaveStarted)
            {
                HasWaveStarted = true;
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
        else
        {

        }

    }

    IEnumerator FirstWaveRoutine()
    {
        while (Player == null || Timer == null || EnemySpawnManagerScriptRef == null || SafeZone == null)
        {
            yield return null;
        }
    
    Player.DisableInputs();

    EnemySpawnManagerScriptRef.ResetSpawners();

    SafeZone.ResetSize();

    yield return new WaitForSeconds(3f);

    EnemySpawnManagerScriptRef.Activation();

    Timer.StartTimer();

    Player.EnableInputs();

    SafeZone.BeginShrinking();
    }

    public void Pause()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && IsPaused == false)
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
        SceneManager.LoadScene(2);
    }

    //gamecontroller gerenciar os spawns

    private void HandleSpawning()
    {

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
