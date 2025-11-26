using UnityEngine;
using System.Collections;


public enum BettleState { Chasing, PreparingAttack, CoolingDown }


public class BettleEnemyScript : MonoBehaviour
{
    private TutorialManager tutorialManager;

    // ====================================================================
    // 1. CONFIGURAÇÕES & REFERÊNCIAS
    // ====================================================================
    [Header("Configurações Base")]
    [SerializeField] int Hp = 40;
    [SerializeField] float enemySpeed = 2.5f;
    
    [Header("Lógica de Perseguição")]
    [SerializeField] float stopAndShootDistance = 12f;
    
    [Header("Ataque Rolabosta")]
    [SerializeField] GameObject redBulletPrefab;   // Prefab de Bala VERMELHA
    [SerializeField] GameObject greenBulletPrefab; // Prefab de Bala VERDE
    [SerializeField] Transform firePoint;
    [SerializeField] float prepTime = 1.5f;
    [SerializeField] float cooldownTime = 10f;

    [Header("Cor e Componentes")]
    [Tooltip("Defina a cor fixa deste prefab (RED ou GREEN).")]
    [SerializeField] private BulletColor InitialColor;

    private ColorHandler colorHandler;

    private Renderer myRenderer;
    private Rigidbody rb;
    
    // --- REFERÊNCIAS DINÂMICAS ---
    private Transform playerTargetTransform;
    private Component gameControllerRef;      
    // ----------------------------
    
    private BettleState currentState = BettleState.Chasing;
    private Coroutine attackRoutineInstance; 
    private Vector3 currentDirection = Vector3.zero;


    // ====================================================================
    // 2. INICIALIZAÇÃO HÍBRIDA (COM ESPERA)
    // ====================================================================
    
    public void SetManager(TutorialManager manager)
{
    tutorialManager = manager;
}
    private void Awake()
    {
        //Pega ref do ColorHandler
        colorHandler = GetComponent<ColorHandler>();

        //Define a cor desse merdinha
        colorHandler.currentColor = InitialColor;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRenderer = GetComponent<Renderer>();

        // Tenta encontrar o Player imediatamente
        FindTargetAndController();
        
        if (playerTargetTransform != null)
        {
            currentState = BettleState.Chasing;
        }
        else
        {
            // Se falhou (ordem de execução), inicia a coroutine de espera
            Debug.LogWarning("Besouro: Player não encontrado de primeira. Iniciando rotina de espera.");
            StartCoroutine(WaitForPlayerAndInitialize());
        }
    }
    
    IEnumerator WaitForPlayerAndInitialize()
    {
        while (GameControllerScript.controller == null && TutorialController.controller == null)
        {
            yield return null;
        }
        
        while (playerTargetTransform == null)
        {
            FindTargetAndController();
            yield return null;
        }
        
        currentState = BettleState.Chasing;
        Debug.Log("Besouro Rolabosta: Player encontrado via Coroutine! Iniciando.");
    }


    // Centraliza a busca do Player e do Controller em qualquer cena
    void FindTargetAndController()
    {
        if (GameControllerScript.controller != null)
        {
            gameControllerRef = GameControllerScript.controller;
            if (GameControllerScript.controller.Player != null)
            {
                playerTargetTransform = GameControllerScript.controller.Player.transform;
            }
        }
        else if (TutorialController.controller != null)
        {
            gameControllerRef = TutorialController.controller;
            if (TutorialController.controller.PlayerTutorialRef != null)
            {
                playerTargetTransform = TutorialController.controller.PlayerTutorialRef.transform;
            }
        }
    }


    // ====================================================================
    // 3. MOVIMENTO E ESTADOS
    // ====================================================================


    void Update()
    {
        if (playerTargetTransform == null) return;
        
        switch (currentState)
        {
            case BettleState.Chasing:
                HandleChasingLogicAndRotation();
                break;
            
            case BettleState.PreparingAttack:
            case BettleState.CoolingDown:
                HandleStoppingRotation();
                break;
        }
    }
    
    void FixedUpdate()
    {
        if (playerTargetTransform == null) return;

        if (currentState == BettleState.Chasing)
        {
            ApplyMovementVelocity();
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }


    void HandleChasingLogicAndRotation()
    {
        Vector3 playerPosition = playerTargetTransform.position;
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;


        // Transição para Ataque
        if (distance <= stopAndShootDistance && attackRoutineInstance == null)
        {
            currentState = BettleState.PreparingAttack;
            attackRoutineInstance = StartCoroutine(AttackRoutine());
            return;
        }


        // Cálculo da Direção para o FixedUpdate
        direction.y = 0f;
        currentDirection = direction.normalized;
        
        // Rotação: Olhar o Player (apenas no plano XZ)
        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
    }


    void ApplyMovementVelocity()
    {
        rb.linearVelocity = new Vector3(
            currentDirection.x * enemySpeed,
            rb.linearVelocity.y,
            currentDirection.z * enemySpeed
        );
    }
    
    void HandleStoppingRotation()
    {
        if (playerTargetTransform != null)
        {
            Vector3 playerPosition = playerTargetTransform.position;
            transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
        }
    }
    
    // ====================================================================
    // 4. LÓGICA DE ATAQUE E COR
    // ====================================================================
    
    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(prepTime);


        Shoot();
        
        currentState = BettleState.CoolingDown;
        yield return new WaitForSeconds(cooldownTime);


        currentState = BettleState.Chasing;
        attackRoutineInstance = null;
    }


    void Shoot()
{
    if (colorHandler == null || firePoint == null) return; 

    GameObject prefabToInstantiate = null;
    BulletColor enemyColor = colorHandler.currentColor;

    // 1. Seleciona o prefab de bala baseado na cor do inimigo.
    if (enemyColor == BulletColor.Red)
    {
        prefabToInstantiate = redBulletPrefab;
    }
    else if (enemyColor == BulletColor.Green)
    {
        prefabToInstantiate = greenBulletPrefab;
    }
    
    if (prefabToInstantiate == null) 
    {
        Debug.LogError($"Prefab de bala para a cor {enemyColor} está faltando no BettleEnemyScript.");
        return;
    }

    // 2. Instancia o prefab correto
    GameObject newBullet = Instantiate(prefabToInstantiate, firePoint.position, firePoint.rotation);
    BulletController bulletScript = newBullet.GetComponent<BulletController>();

    if (bulletScript != null)
    {
        bulletScript.isFiredByPlayer = false;
        bulletScript.bulletColor = enemyColor; // Passa a cor correta (Red ou Green)
    }
    else
    {
        Destroy(newBullet);
    }
}
    // ====================================================================
    // 5. LÓGICA DE DANO/MORTE
    // ====================================================================
    
    public void TakingDamage(int bulletDamage)
    {
        Hp -= bulletDamage;
        if (Hp <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        // NOVO: 1. Tenta notificar o TutorialController (se estiver no tutorial)
        if (gameControllerRef is TutorialController tutorialController)
        {
            tutorialController.EnemyKilled();
        }
        // 2. Mantém a lógica existente para o GameController (jogo principal)
        else if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        // 🚨 CORREÇÃO: Usa a variável de classe que foi reconfirmada no topo.
        if (attackRoutineInstance != null) StopCoroutine(attackRoutineInstance);

        if (tutorialManager != null)
    tutorialManager.EnemyKilled();
        Debug.Log($"ENEMY KILLED: {gameObject.name}");
        
        Destroy(gameObject);
    }
}
