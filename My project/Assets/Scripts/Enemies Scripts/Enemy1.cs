using UnityEngine;
using System.Collections;

public enum EnemyState { Chasing, Attacking, CoolingDown }

public class Enemy1 : MonoBehaviour
{
    // ====================================================================
    // 1. COMPONENTES E REFERÊNCIAS
    // ====================================================================
    private TutorialController tutorial;

    [Header("Componentes")]
    private Rigidbody rb;
    private Animator animator; //pra animação funcionar
    private ColorHandler myColorHandler;

    private TutorialManager tutorialManager;

    [Header("Stats")]
    [SerializeField] int Hp = 20;
    [SerializeField] float enemySpeed = 15f;
    [SerializeField] float stoppingDistance = 1.5f;

    [Header("Alvo & Cena")]
    private Transform playerTargetTransform;
    private Component gameControllerRef;
    private bool hasLanded = false;

    [Header("Ataque & Cores")]
    [SerializeField] GameObject redBulletPrefab;   // Prefab de Bala VERMELHA
    [SerializeField] GameObject greenBulletPrefab; // Prefab de Bala VERDE
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int shotsPerBurst = 3;
    [SerializeField] float cooldownTime = 2f;

    [Header("Som de Dano")]
    [SerializeField] private AudioSource hitAudioSource;

    private int burstCounter = 0;
    private Coroutine attackCoroutine;

    private EnemyState currentState = EnemyState.Chasing;
    private Vector3 currentDirection = Vector3.zero;

    [Header("FEEDBACK VISUAL")]
    [SerializeField] private Renderer enemyRenderer; // O componente Renderer (MeshRenderer ou SkinnedMeshRenderer)
    [SerializeField] private Material damageMaterial; // O material 'mPreto'
    [SerializeField] private Material originalMaterial; // O material 'mCromado' (Para atribuir no Inspector)
    [SerializeField] private float flashDuration = 0.1f; // Duração do piscar (0.1s é um bom padrão) 

    private Material[] originalMaterialsArray; // Array para guardar a lista completa de materiais
    private int materialIndexToReplace = -1;  // O índice onde 'mCromado' está no array
    private Coroutine flashCoroutine; // Referência para controlar o piscar


    // ====================================================================
    // 2. INICIALIZAÇÃO E ENCONTRO DE ALVO
    // ====================================================================

    IEnumerator Start()
    {

        yield return null;
        tutorial = FindObjectOfType<TutorialController>();

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();  //pra animação funcionar
        myColorHandler = GetComponent<ColorHandler>();

        if (enemyRenderer == null)
{
    enemyRenderer = GetComponentInChildren<Renderer>(); 
}

if (enemyRenderer != null && originalMaterial != null)
{
    // CRÍTICO: 1. Prepara o nome do material de referência (limpa e minúsculo)
    // O nome do Asset é o nome do Material que você arrastou para o Inspector.
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
        // Armazenamos a cópia da instância de materiais para restauração (para uso no FlashDamageRoutine)
        originalMaterialsArray = enemyRenderer.materials;
        Debug.Log($"Material '{originalMaterialName}' encontrado no índice {materialIndexToReplace}. Feedback pronto.");
    }
    else
    {
        // Se o nome não for encontrado, este Log será ativado.
        Debug.LogWarning($"Material '{originalMaterialName}' não encontrado no Renderer. O feedback visual não funcionará. Materiais no Renderer: {string.Join(", ", System.Array.ConvertAll(sharedMaterials, m => m.name))}");
    }
}

        FindTargetAndController();

        if (playerTargetTransform != null)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            Debug.LogError("Player não encontrado! Inimigo desativado.");
            enabled = false;
        }


    }

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
    // 3. MÁQUINA DE ESTADOS (FSM)
    // ====================================================================

    void Update()
    {

        if (playerTargetTransform == null) return;

        switch (currentState)
        {
            case EnemyState.Chasing:
                HandleChasingLogicAndRotation();
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Chasing && hasLanded)
        {
            ApplyMovementVelocity();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLanded && collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;

            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.angularVelocity = Vector3.zero;
            currentDirection = Vector3.zero;
        }


    }

    private void ResetVelocity()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    // ====================================================================
    // 4. LÓGICA DE ESTADOS
    // ====================================================================

    void HandleChasingLogicAndRotation()
    {
        Vector3 playerPosition = playerTargetTransform.position;
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

        //animator.SetFloat("enemySpeed", enemySpeed); //pra animação funcionar

        if (distance <= stoppingDistance)
        {
            if (attackCoroutine == null)
            {
                HandleStopping();
                attackCoroutine = StartCoroutine(AttackRoutine());
            }
            return;
        }

        direction.y = 0f;

        currentDirection = direction.normalized;

        if (currentDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection, Vector3.up);
        }

    }

    void ApplyMovementVelocity()
    {
        if (rb == null) return;

        if (currentDirection.sqrMagnitude < 0.01f)
        {
            ResetVelocity();
            return;
        }

        rb.linearVelocity = new Vector3(
            currentDirection.x * enemySpeed,
            rb.linearVelocity.y,
            currentDirection.z * enemySpeed
        );

        // Define a velocidade para animação baseado no movimento real
        float currentMoveSpeed = currentDirection.sqrMagnitude > 0.01f ? enemySpeed : 0f;
        animator.SetFloat("enemySpeed", currentMoveSpeed);
    }

    void HandleStopping()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        animator.SetFloat("enemySpeed", 0); //pra forçar a parada da animação de andar 
    }

    IEnumerator AttackRoutine()
    {
        currentState = EnemyState.Attacking;

        burstCounter++;

        if (playerTargetTransform != null)
        {
            Vector3 targetXZ = playerTargetTransform.position;
            targetXZ.y = transform.position.y;

            Vector3 lookDirection = targetXZ - transform.position;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        //animator.SetBool("isAttacking", true);
        //animator.SetTrigger("Attack");                   //não sei se vai dar pra usar a animação de ataque

        for (int i = 0; i < shotsPerBurst; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }

        currentState = EnemyState.CoolingDown;
        //animator.SetBool("isAttacking", false);

        yield return new WaitForSeconds(cooldownTime);

        currentState = EnemyState.Chasing;
        attackCoroutine = null;
    }

    void ShootBullet()
{
    if (myColorHandler == null || firePoint == null) return;

    GameObject prefabToInstantiate = null;
    BulletColor enemyColor = myColorHandler.currentColor;
    
    // 1. SELEÇÃO DO PREFAB CORRETO
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
        Debug.LogError($"Prefab de bala para a cor {enemyColor} está faltando no Inspector do Enemy1.");
        return;
    }
    
    // 2. INSTANCIAÇÃO
    GameObject newBullet = Instantiate(prefabToInstantiate, firePoint.position, firePoint.rotation);

    // 3. CONFIGURAÇÃO DO SCRIPT
    BulletController bulletScript = newBullet.GetComponent<BulletController>();

    if (bulletScript != null)
    {
        bulletScript.isFiredByPlayer = false;
        bulletScript.Initialize(false);
            // Atribui a cor, que deve ser a mesma cor visual do prefab instanciado.
        bulletScript.bulletColor = enemyColor; 
    }
}

    // ====================================================================
    // 5. DANO 
    // ====================================================================


    public void TakingDamage(int bulletDamage)
    {
        Hp -= bulletDamage;

        flashCoroutine = StartCoroutine(FlashDamageRoutine());

        // if (flashCoroutine != null)
        // {
        //     StopCoroutine(flashCoroutine); // Para o piscar anterior
        // }

       
        if (hitAudioSource != null)
            hitAudioSource.Play();


        Debug.Log("Louva-Deus recebeu " + bulletDamage + " de dano. Vida restante: " + Hp);

        if (Hp <= 0)
        {
            Die();
        }
    }

    // corotina para piscar

    IEnumerator FlashDamageRoutine()
    {
        // Verificações de segurança
        if (materialIndexToReplace == -1 || damageMaterial == null || enemyRenderer == null)
        {
            flashCoroutine = null;
            yield break;
        }

        // CRÍTICO 1: Pega uma CÓPIA dos materiais atuais (o que é feito com .materials)
        Material[] materialsToModify = enemyRenderer.materials;
    
        // CRÍTICO 2: Armazena o material que será substituído ANTES de modificá-lo
        Material materialToRestore = materialsToModify[materialIndexToReplace];

        // 2. Troca para o material de dano ('mPreto')
        materialsToModify[materialIndexToReplace] = damageMaterial;
        enemyRenderer.materials = materialsToModify;

        // 3. Espera o tempo de piscar
        yield return new WaitForSeconds(flashDuration);

        // 4. Retorna para o material original que armazenamos (materialToRestore)
        materialsToModify[materialIndexToReplace] = materialToRestore; 
        enemyRenderer.materials = materialsToModify;

       flashCoroutine = null;
    }

    // ====================================================================
    // 6. MORTE — CORRIGIDO
    // ====================================================================

    void Die()
    {
        // 1) Notifica o TutorialManager (se existir) — prioridade para o manager que controla paredes.
        TutorialManager tm = FindObjectOfType<TutorialManager>();
        if (tm != null)
        {
            tm.EnemyKilled();
        }
        else
        {
            // 2) Se não houver TutorialManager, tenta o TutorialController (compatibilidade)
            if (tutorial != null)
            {
                tutorial.EnemyKilled();
            }
            // 3) Se não for tutorial, notifica o GameController padrão (jogo principal)
            else if (GameControllerScript.controller != null)
            {
                GameControllerScript.controller.AumentarNumerodeInimigosMortos();
            }
        }

        // Para garantir que não reste coroutine ativa
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        Destroy(gameObject);
    }

    //Função para o tutorial funfar

    public void SetManager(TutorialManager manager)
{
    tutorialManager = manager;
}

    
}
