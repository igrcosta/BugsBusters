using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Enum para rastrear o estado atual do tutorial.
public enum TutorialState
{
    PHASE_1_START,
    PHASE_1_COMBAT,
    PHASE_2_START,
    PHASE_2_COMBAT,
    PHASE_3_START,
    TUTORIAL_COMPLETE
}

public class TutorialManager : MonoBehaviour
{
    // --- Referências de Objetos de Cena ---
    [Header("Objetos de Parede (Desativar após a fase)")]
    public GameObject wallPhase1; // Phase 1 Tutorial Wall
    public GameObject wallPhase2; // Phase 2 Tutorial Wall (A Parede 2)
    
    [Header("Canvases de Mensagem")]
    public GameObject canvasPhase1; // FirstCanvas
    public GameObject canvasPhase2; // SecondCanvas
    public GameObject canvasPhase3; // ThirdCanvas
    
    [Header("Inimigos e Power-ups")]
    // Prefab para a Fase 1 (Spawn Point 1)
    public GameObject enemyPrefabPhase1; 
    // NOVO: Prefab específico para a Posição 1 da Fase 2 (Spawn Point 2_1)
    public GameObject enemyPrefabPhase2_Pos1; 
    // NOVO: Prefab específico para a Posição 2 da Fase 2 (Spawn Point 2_2)
    public GameObject enemyPrefabPhase2_Pos2; 
    public GameObject powerUpPrefab; // O prefab do Power-up de vida

    public GameObject BuffPrefab;
    
    [Header("Pontos de Spawn (Transform de Posição)")]
    public Transform spawnPoint1; // Fase 1: firstposition
    public Transform spawnPoint2_1; // Fase 2: Pos1
    public Transform spawnPoint2_2; // Fase 2: Pos2
    public Transform spawnPoint3; // Fase 3: Ponto de spawn para o Power-up (Pos3)
    public Transform spawnPoint3_1; //Fase3: ponto para spawn do outro power-up

    // --- Variáveis de Controle ---
    private TutorialState currentState = TutorialState.PHASE_1_START;
    private int enemiesToKill = 0; 
    private int enemiesKilledInPhase = 0; 
    private bool phaseActive = false; // Bloqueia ativação de outros triggers durante a fase

    void Start()
    {
        // Garante o estado inicial dos objetos.
        if (wallPhase1 != null) wallPhase1.SetActive(true);
        if (wallPhase2 != null) wallPhase2.SetActive(true);
        
        if (canvasPhase1 != null) canvasPhase1.SetActive(false);
        if (canvasPhase2 != null) canvasPhase2.SetActive(false);
        if (canvasPhase3 != null) canvasPhase3.SetActive(false);
        
        Debug.Log("Tutorial Manager Inicializado na " + currentState);
    }

    // Chamado pelo script PhaseTrigger
    public void OnTriggerEntered(int phaseNumber)
    {
        if (phaseActive) return;

        if (phaseNumber == 1 && currentState == TutorialState.PHASE_1_START)
        {
            StartCoroutine(StartPhase1());
        }
        else if (phaseNumber == 2 && currentState == TutorialState.PHASE_2_START)
        {
            StartCoroutine(StartPhase2());
        }
        else if (phaseNumber == 3 && currentState == TutorialState.PHASE_3_START)
        {
            StartCoroutine(StartPhase3());
        }
    }

    // --- FASES ---

    IEnumerator StartPhase1()
    {
        phaseActive = true;
        currentState = TutorialState.PHASE_1_COMBAT;
        Debug.Log("Iniciando Fase 1: Pausa e Canvas.");

        // 1. Mostrar Canvas 1 e PAUSAR o jogo (Time.timeScale = 0f)
        ShowCanvas(canvasPhase1, true);
        
        // NOVO: Espera até que o botão "OK" seja pressionado.
        yield return new WaitUntil(() => !canvasPhase1.activeSelf);
        
        // 2. Spawna 1 inimigo usando o Prefab da FASE 1
        if (spawnPoint1 != null && enemyPrefabPhase1 != null)
        {
            SpawnEnemy(spawnPoint1.position, enemyPrefabPhase1);
            enemiesToKill = 1;
        }
        else { enemiesToKill = 0; Debug.LogError("Prefab Fase 1 ou Spawn Point 1 não anexado!"); }

        phaseActive = false;
    }

    IEnumerator CompletePhase1()
    {
        if (wallPhase1 != null) wallPhase1.SetActive(false); 

        currentState = TutorialState.PHASE_2_START;
        enemiesKilledInPhase = 0;
        Debug.Log("Fase 1 Completa. Parede 1 Desabilitada.");
        yield return null;
    }

    IEnumerator StartPhase2()
    {
        phaseActive = true;
        currentState = TutorialState.PHASE_2_COMBAT;
        Debug.Log("Iniciando Fase 2: Spawn de 2 inimigos (Prefabs diferentes).");

        // 1. Mostrar Canvas 2
        ShowCanvas(canvasPhase2, true);
        yield return new WaitUntil(() => !canvasPhase2.activeSelf);  // Pausa
        ShowCanvas(canvasPhase2, false);

        // 2. Spawna os inimigos usando os prefabs específicos
        int spawns = 0;
        
        // Spawn do Inimigo da POS 1 (Inimigo 2)
        if (spawnPoint2_1 != null && enemyPrefabPhase2_Pos1 != null) 
        { 
            SpawnEnemy(spawnPoint2_1.position, enemyPrefabPhase2_Pos1); 
            spawns++; 
        }
        else if (spawnPoint2_1 == null || enemyPrefabPhase2_Pos1 == null) 
        {
            Debug.LogError("Spawn Point 2_1 OU Prefab 2_Pos1 faltando!");
        }

        // Spawn do Inimigo da POS 2 (Inimigo 3)
        if (spawnPoint2_2 != null && enemyPrefabPhase2_Pos2 != null) 
        { 
            SpawnEnemy(spawnPoint2_2.position, enemyPrefabPhase2_Pos2); 
            spawns++; 
        }
        else if (spawnPoint2_2 == null || enemyPrefabPhase2_Pos2 == null) 
        {
            Debug.LogError("Spawn Point 2_2 OU Prefab 2_Pos2 faltando!");
        }
        
        enemiesToKill = 2; // Precisa matar a quantidade que conseguiu spawnar.
        phaseActive = false;
    }

    IEnumerator CompletePhase2()
    {
        if (wallPhase2 != null) wallPhase2.SetActive(false); // Desabilita a parede 2

        currentState = TutorialState.PHASE_3_START;
        enemiesKilledInPhase = 0;
        Debug.Log("Fase 2 Completa. Parede 2 Desabilitada.");
        yield return null;
    }

    IEnumerator StartPhase3()
    {
        phaseActive = true;
        currentState = TutorialState.TUTORIAL_COMPLETE; 
        Debug.Log("Iniciando Fase 3: Spawn Power-up (vida).");

        // 1. Mostrar Canvas 3
        ShowCanvas(canvasPhase3, true);
        yield return new WaitUntil(() => !canvasPhase3.activeSelf);
        ShowCanvas(canvasPhase3, false);
        
        // 2. Spawna o Power-up na Pos3 com Instantiate (conforme solicitado)
        if (powerUpPrefab != null && spawnPoint3 != null)
        {
             // Usa o Instantiate padrão, pois é um item e não precisa do SetManager
             Instantiate(powerUpPrefab, spawnPoint3.position, spawnPoint3.rotation);

             Instantiate(BuffPrefab, spawnPoint3_1.position, spawnPoint3_1.rotation);

             Debug.Log("Power-ups spawnados na Pos3 e Pos4.");
        }
        else
        {
             Debug.LogError("Power-up Prefab OU Spawn Point 3 não está anexado!");
        }

        Debug.Log("Tutorial Concluído!");
        phaseActive = false;
    }

    // --- Sistema de Combate e Ajuda ---

    // Chamado pelo TutorialEnemy (ou EnemyController) quando um inimigo morre.
    public void EnemyKilled()
    {
        // Verifica se a fase é de combate
        if (currentState != TutorialState.PHASE_1_COMBAT && currentState != TutorialState.PHASE_2_COMBAT) return; 

        enemiesKilledInPhase++;
        Debug.Log($"EnemyKilled() chamado. Contagem: {enemiesKilledInPhase}/{enemiesToKill}");

        if (enemiesKilledInPhase >= enemiesToKill)
        {
            if (currentState == TutorialState.PHASE_1_COMBAT)
            {
                StartCoroutine(CompletePhase1());
            }
            else if (currentState == TutorialState.PHASE_2_COMBAT)
            {
                StartCoroutine(CompletePhase2());
            }
        }
    }

    // Função de spawn que configura o inimigo
    private void SpawnEnemy(Vector3 position, GameObject enemyPrefabToUse)
{
    GameObject newEnemy = Instantiate(enemyPrefabToUse, position, Quaternion.identity);

    // Tenta SmallEnemy
    SmallEnemy sm = newEnemy.GetComponent<SmallEnemy>();
    if (sm != null)
    {
        sm.SetManager(this);
        return;
    }

    // Tenta BettleEnemy
    BettleEnemyScript be = newEnemy.GetComponent<BettleEnemyScript>();
    if (be != null)
    {
        be.SetManager(this);
        return;
    }

    // Tenta Enemy1 / TutorialEnemy
    Enemy1 te = newEnemy.GetComponent<Enemy1>();
    if (te != null)
    {
        te.SetManager(this);
        return;
    }

    Debug.LogError($"O prefab '{enemyPrefabToUse.name}' não tem nenhum script de inimigo válido!");
}

    public void ShowCanvas(GameObject canvas, bool show)
    {
        if (canvas != null)
        {
            canvas.SetActive(show);
            // ESSENCIAL: Controla a pausa do jogo
            Time.timeScale = show ? 0f : 1f; 
        }
    }

    // Esta função será anexada ao evento OnClick() do botão "OK"
    public void OnContinueButtonClicked(GameObject canvas)
    {
        // 1. Desativa o canvas (Isso libera o WaitUntil no Coroutine)
        ShowCanvas(canvas, false); 
        
        // O Time.timeScale será restaurado para 1f automaticamente dentro de ShowCanvas.
    }

    public void SkipTutorialCheat()
{
    Debug.LogWarning("CHEATER: Ignorando tutorial e forçando início da Wave 1.");
    
    // 1. Garante que o jogo não está pausado (CRUCIAL!)
    Time.timeScale = 1f;

    // 2. Limpa os Canvases (caso estivessem ativos)
    if (canvasPhase1 != null) canvasPhase1.SetActive(false);
    if (canvasPhase2 != null) canvasPhase2.SetActive(false);
    if (canvasPhase3 != null) canvasPhase3.SetActive(false);

    // 3. Desativa as paredes (para liberar o Player)
    if (wallPhase1 != null) wallPhase1.SetActive(false);
    if (wallPhase2 != null) wallPhase2.SetActive(false);

    // 4. Limpa inimigos restantes (se houver)
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    foreach (GameObject enemy in enemies)
    {
        Destroy(enemy);
    }
    
    // 5. CHAVE: Força o GameController a iniciar a Primeira Wave.
    if (GameControllerScript.controller != null)
    {
        // Precisamos de um método público no GameController para iniciar a primeira wave 
        // IGNORANDO a lógica de 'OnSceneLoaded'.
        GameControllerScript.controller.ForceStartFirstWave(); 
    }
    else
    {
        Debug.LogError("GameController não encontrado. O cheat de skip falhou ao iniciar a wave.");
    }
    
    // 6. Destrói o Tutorial Manager (ele não é mais necessário)
    Destroy(gameObject); 
}

    
}