using UnityEngine;
using System.Collections;
// Não precisamos do System.Threading.Tasks com Coroutines

// Máquina de Estados Finitos (FSM)
public enum EnemyState { Chasing, Attacking, CoolingDown }

public class Enemy1 : MonoBehaviour
{
    // ====================================================================
    // 1. COMPONENTES E REFERÊNCIAS
    // ====================================================================
    [Header("Componentes")]
    private Animator anim;
    private Rigidbody rb;
    private Renderer myRenderer;

    [Header("Stats")]
    [SerializeField] int Hp = 20;
    // enemySpeed aumentado para garantir que o movimento seja visível
    [SerializeField] float enemySpeed = 15f; 
    [SerializeField] float stoppingDistance = 1.5f;

    [Header("Alvo & Cena")]
    private GameObject playerTarget;
    private bool hasLanded = false;
    
    private GameControllerScript gameControllerRef;
    private TutorialController tutorialControllerRef;

    [Header("Ataque & Cores")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int shotsPerBurst = 3;
    [SerializeField] float cooldownTime = 2f;
    
    private int burstCounter = 0;
    public int currentColor;
    private Coroutine attackCoroutine; 

    // Estado inicial
    private EnemyState currentState = EnemyState.Chasing;

    // ====================================================================
    // 2. INICIALIZAÇÃO E ENCONTRO DE ALVO
    // ====================================================================

    void Start()
    {
        // 1. Obtém Componentes
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        myRenderer = GetComponent<Renderer>();

        // 2. Encontra o Alvo e Controladores
        FindTargetAndController();
        
        // 3. Inicialização de Estado e Animação
        currentState = EnemyState.Chasing;
        anim.SetBool("isFalling", true);
    }

    void FindTargetAndController()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");

        if (GameControllerScript.controller != null)
        {
            gameControllerRef = GameControllerScript.controller;
            currentColor = gameControllerRef.ColorLogic[0];
            return;
        }

        if (TutorialController.controller != null)
        {
            tutorialControllerRef = TutorialController.controller;
            if (tutorialControllerRef.PlayerTutorialRef != null)
            {
                currentColor = tutorialControllerRef.PlayerTutorialRef.currentColor; 
            }
            else
            {
                 currentColor = 1; 
            }
            return;
        }

        Debug.LogError("Nenhum controlador de cena (Tutorial ou Game) encontrado! O Inimigo não funcionará corretamente.");
    }
    
    // ====================================================================
    // 3. MÁQUINA DE ESTADOS (FSM)
    // ====================================================================

    void Update()
    {
        // O Update é usado para detecção de distância e rotação (responsividade visual)
        switch (currentState)
        {
            case EnemyState.Chasing:
                HandleChasingLogicAndRotation();
                break;
            // Attacking e CoolingDown são passivos e apenas esperam a Coroutine
        }
    }
    
    void FixedUpdate()
    {
        // A aplicação de velocidade (física) DEVE ocorrer no FixedUpdate
        if (currentState == EnemyState.Chasing)
        {
            ApplyMovementVelocity();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLanded && collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            anim.SetBool("isFalling", false);
            anim.SetTrigger("FallImpact");
        }
    }

    // ====================================================================
    // 4. LÓGICA DE ESTADOS
    // ====================================================================
    
    // Variável de instância para armazenar a direção (calculada no Update, usada no FixedUpdate)
    private Vector3 currentDirection = Vector3.zero;

    void HandleChasingLogicAndRotation()
    {
        if (playerTarget == null) return;

        Vector3 playerPosition = playerTarget.transform.position;
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

        // 1. Transição de estado: Chasing -> Attacking
        if (distance <= stoppingDistance)
        {
            if (attackCoroutine == null)
            {
                HandleStopping(); 
                attackCoroutine = StartCoroutine(AttackRoutine());
            }
            return; 
        }

        // 2. Cálculo da Rotação e Direção (X e Z)
        direction.y = 0f;
        currentDirection = direction.normalized; // Armazena a direção para o FixedUpdate

        if (currentDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection, Vector3.up);
        }

        anim.SetBool("isWalking", true);
    }
    
    // NOVO MÉTODO: Aplica a velocidade horizontal, respeitando a gravidade
    void ApplyMovementVelocity()
    {
        // 🚨 CORREÇÃO: Aplicamos o movimento no XZ, MANTENDO o Y da gravidade.
        rb.linearVelocity = new Vector3(
            currentDirection.x * enemySpeed, 
            rb.linearVelocity.y, // <-- AQUI RESPEITAMOS A GRAVIDADE (Eixo Y)
            currentDirection.z * enemySpeed
        );
    }

    void HandleStopping()
    {
        // Para o movimento horizontal do Rigidbody, mantendo a gravidade (y)
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        anim.SetBool("isWalking", false);
    }

    // Gerencia o ciclo de ataque completo (Attack -> Cooldown -> Chasing)
    IEnumerator AttackRoutine()
    {
        // --- Fase 1: PREPARAÇÃO DO ATAQUE ---
        currentState = EnemyState.Attacking;

        burstCounter++;
        currentColor = (burstCounter % 2 == 0) ? 1 : 0;
        ApplyEnemyMaterial();
        
        // Aplica a mira XZ final antes de atirar
        if (playerTarget != null)
        {
            Vector3 targetXZ = playerTarget.transform.position;
            targetXZ.y = transform.position.y;
            Vector3 lookDirection = targetXZ - transform.position;

            if(lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        // --- Fase 2: CICLO DE TIROS ---
        anim.SetBool("isAttacking", true); 
        anim.SetTrigger("Attack");

        for (int i = 0; i < shotsPerBurst; i++)
        {
            ShootBullet(); 
            yield return new WaitForSeconds(fireRate);
        }

        // --- Fase 3: COOLDOWN ---
        currentState = EnemyState.CoolingDown;
        anim.SetBool("isAttacking", false);
        
        yield return new WaitForSeconds(cooldownTime); 

        // --- Fase 4: RETORNO ---
        currentState = EnemyState.Chasing;
        attackCoroutine = null; 
    }

    void ShootBullet()
    {
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();
        Material targetMaterial = GetBulletMaterial(); 

        if (bulletScript != null)
        {
            bulletScript.isFiredByPlayer = false;
            bulletScript.bulletColor = currentColor;
        }

        if (bulletRenderer != null && targetMaterial != null)
        {
            bulletRenderer.material = targetMaterial;
        }
    }

    // ====================================================================
    // 5. LÓGICA REUTILIZÁVEL (Cores e Dano)
    // ====================================================================

    void ApplyEnemyMaterial()
    {
        Material targetMaterial = GetBulletMaterial();
        if (myRenderer != null && targetMaterial != null)
        {
            myRenderer.material = targetMaterial;
        }
    }

    Material GetBulletMaterial()
    {
        if (gameControllerRef != null)
        {
             return (currentColor == 1) ? 
                gameControllerRef.PlayerMatFirst : 
                gameControllerRef.PlayerMatSecond;
        }
        
        if (tutorialControllerRef != null)
        {
            return (currentColor == 1) ? 
                tutorialControllerRef.MatFirst : 
                tutorialControllerRef.MatSecond;
        }
        return null;
    }

    public void TakingDamage(int bulletDamage)
    {
        Hp -= bulletDamage;
        Debug.Log("Inimigo recebeu " + bulletDamage + "de dano. Vida restante: " + Hp);

        if (Hp <= 0)
        {
            if (gameControllerRef != null)
            {
                gameControllerRef.AumentarNumerodeInimigosMortos();
            }
            
            Destroy(gameObject);
        }
    }
}