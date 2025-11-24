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
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float prepTime = 1.5f;
    [SerializeField] float cooldownTime = 10f;

    [Header("Cor e Componentes")]
    public int currentColor;
    private Renderer myRenderer;
    private Rigidbody rb;
    
    // --- REFERÊNCIAS DINÂMICAS ---
    private Transform playerTargetTransform;
    private Component gameControllerRef;      
    // ----------------------------
    
    private BettleState currentState = BettleState.Chasing;
    // 🚨 LINHA CRÍTICA PARA CORRIGIR O ERRO CS0103:
    private Coroutine attackRoutineInstance; 
    private Vector3 currentDirection = Vector3.zero;


    // ====================================================================
    // 2. INICIALIZAÇÃO HÍBRIDA (COM ESPERA)
    // ====================================================================
    
    public void SetManager(TutorialManager manager)
{
    tutorialManager = manager;
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
            InitializeColor();
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
        InitializeColor();
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
        if (bulletPrefab == null || firePoint == null) return;
        
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BulletController bulletScript = newBullet.GetComponent<BulletController>();


        if (bulletScript != null)
        {
            bulletScript.isFiredByPlayer = false;
            bulletScript.bulletColor = currentColor;
            ApplyBulletMaterial(newBullet.GetComponent<Renderer>());
        }
    }
    
    void InitializeColor()
    {
        currentColor = Random.Range(0, 2);
        ApplyEnemyMaterial(myRenderer);
    }
    
    void ApplyEnemyMaterial(Renderer targetRenderer)
    {
        Material targetMat = GetTargetMaterial();
        if (targetRenderer != null && targetMat != null)
        {
            targetRenderer.material = targetMat;
        }
    }
    
    void ApplyBulletMaterial(Renderer bulletRenderer)
    {
        ApplyEnemyMaterial(bulletRenderer);
    }
    
    Material GetTargetMaterial()
    {
        if (gameControllerRef is GameControllerScript standardController)
        {
            return (currentColor == 1) ? standardController.PlayerMatFirst : standardController.PlayerMatSecond;
        }
        else if (gameControllerRef is TutorialController tutorialController)
        {
            return (currentColor == 1) ? tutorialController.MatFirst : tutorialController.MatSecond;
        }
        return null;
    }


    // ====================================================================
    // 5. LÓGICA DE DANO/MORTE
    // ====================================================================
    
    public void TakingDamage(int bulletDamage, int bulletColor) // ASSINATURA CORRETA
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