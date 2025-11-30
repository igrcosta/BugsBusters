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

    [Header("FEEDBACK VISUAL")]
    [SerializeField] private Renderer enemyRenderer; // Para o Skinned Mesh Renderer do besouro
    [SerializeField] private Material damageMaterial; // O material 'mPreto'
    [SerializeField] private Material originalMaterial; // O material 'mCromado'
    [SerializeField] private float flashDuration = 0.1f; // Duração do piscar

    private Material[] originalMaterialsArray; 
    private int materialIndexToReplace = -1;  // O índice onde 'mCromado' está
    private Coroutine flashCoroutine; // Referência para controlar o piscar
    
    [Header("Som de Dano")]
    [SerializeField] private AudioSource hitAudioSource;

    private ColorHandler colorHandler;

    private Renderer myRenderer;
    private Rigidbody rb;
    private Animator animator; //pra animação funcionar

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
        animator = GetComponent<Animator>();  //pra animação funcionar
        myRenderer = GetComponent<Renderer>();

        if (enemyRenderer == null)
{
    enemyRenderer = GetComponentInChildren<Renderer>(); 
}

if (enemyRenderer != null && originalMaterial != null)
{
    // CRÍTICO: 1. Prepara o nome do material de referência (limpa e minúsculo)
    string originalMaterialName = originalMaterial.name.Trim().ToLower();
    
    // 2. Pega os materiais compartilhados para inspecionar
    Material[] sharedMaterials = enemyRenderer.sharedMaterials;

    // 3. Busca pelo índice
    for (int i = 0; i < sharedMaterials.Length; i++)
    {
        if (sharedMaterials[i] != null) 
        {
            // Pega o nome do material no slot do Renderer (limpa e minúsculo)
            string currentMaterialName = sharedMaterials[i].name.Trim().ToLower();
            
            // Compara. Se o nome do slot contiver o nome do seu Asset (mCromado)
            if (currentMaterialName.Contains(originalMaterialName)) 
            {
                materialIndexToReplace = i;
                break;
            }
        }
    }
    
    // 4. Finalização e Log
    if (materialIndexToReplace != -1)
    {
        originalMaterialsArray = enemyRenderer.materials;
        Debug.Log($"BettleEnemy: Material '{originalMaterialName}' encontrado no índice {materialIndexToReplace}. Feedback pronto.");
    }
    else
    {
        Debug.LogWarning($"BettleEnemy: Material '{originalMaterialName}' não encontrado no Renderer. O feedback visual não funcionará.");
    }
}

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

        // Define a velocidade para animação baseado no movimento real
        float currentMoveSpeed = currentDirection.sqrMagnitude > 0.01f ? enemySpeed : 0f;
        animator.SetFloat("enemySpeed", currentMoveSpeed);
    }
    
    void HandleStoppingRotation()
    {
        if (playerTargetTransform != null)
        {
            Vector3 playerPosition = playerTargetTransform.position;
            transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
        }
        animator.SetFloat("enemySpeed", 0); //pra forçar a parada da animação de andar 
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

IEnumerator FlashDamageRoutine()
{
    // 1. Verificações de segurança
    if (materialIndexToReplace == -1 || damageMaterial == null || enemyRenderer == null)
    {
        flashCoroutine = null;
        yield break;
    }
    
    // 2. Troca para o material de dano ('mPreto')
    // Cria uma cópia dos materiais
    Material[] currentMaterials = enemyRenderer.materials;
    
    // Aplica o material de Dano
    currentMaterials[materialIndexToReplace] = damageMaterial;
    enemyRenderer.materials = currentMaterials;

    // 3. Espera o tempo de piscar
    yield return new WaitForSeconds(flashDuration);

    // 4. Retorna para o material original
    currentMaterials[materialIndexToReplace] = originalMaterialsArray[materialIndexToReplace];
    enemyRenderer.materials = currentMaterials;

    flashCoroutine = null;
}
    // ====================================================================
    // 5. LÓGICA DE DANO/MORTE
    // ====================================================================
    
    public void TakingDamage(int bulletDamage)
    {
        Hp -= bulletDamage;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine); // Para o piscar anterior
        }
        flashCoroutine = StartCoroutine(FlashDamageRoutine());

        hitAudioSource.Play();

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
