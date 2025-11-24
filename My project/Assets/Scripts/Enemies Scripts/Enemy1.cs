using UnityEngine;
using System.Collections;

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
    [SerializeField] float enemySpeed = 15f; 
    [SerializeField] float stoppingDistance = 1.5f;

    [Header("Alvo & Cena")]
    private Transform playerTargetTransform; 
    // Referência UNIFICADA para o GameController OU TutorialController
    private Component gameControllerRef; // ✅ Mantido como Component
    private bool hasLanded = false;
    
    [Header("Ataque & Cores")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int shotsPerBurst = 3;
    [SerializeField] float cooldownTime = 2f;
    
    private int burstCounter = 0;
    public int currentColor;
    private Coroutine attackCoroutine; 

    private EnemyState currentState = EnemyState.Chasing;
    private Vector3 currentDirection = Vector3.zero;

    // ====================================================================
    // 2. INICIALIZAÇÃO E ENCONTRO DE ALVO
    // ====================================================================

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        myRenderer = GetComponent<Renderer>();

        FindTargetAndController();
        
        if (playerTargetTransform != null)
        {
            InitializeColor();
            currentState = EnemyState.Chasing;
            anim.SetBool("isFalling", true);
        }
        else
        {
            Debug.LogError("Player não encontrado! Inimigo desativado.");
            enabled = false;
        }
    }

    void FindTargetAndController()
    {
        // Prioridade 1: GameController Principal
        if (GameControllerScript.controller != null)
        {
            gameControllerRef = GameControllerScript.controller;
            if (GameControllerScript.controller.Player != null)
            {
                playerTargetTransform = GameControllerScript.controller.Player.transform;
            }
        }
        // Prioridade 2: TutorialController
        else if (TutorialController.controller != null)
        {
            gameControllerRef = TutorialController.controller;
            // Assumindo que PlayerTutorialRef existe no TutorialController
            // Se o TutorialController tem um campo PlayerTutorialRef que é o Transform
            if (TutorialController.controller.PlayerTutorialRef != null) 
            {
                playerTargetTransform = TutorialController.controller.PlayerTutorialRef.transform;
            }
        }
    }
    
    void InitializeColor()
    {
        currentColor = (burstCounter % 2 == 0) ? 1 : 0; 
        ApplyEnemyMaterial();
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

    void HandleChasingLogicAndRotation()
    {
        Vector3 playerPosition = playerTargetTransform.position;
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

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

        anim.SetBool("isWalking", true);
    }
    
    void ApplyMovementVelocity()
    {
        rb.linearVelocity = new Vector3(
            currentDirection.x * enemySpeed, 
            rb.linearVelocity.y, 
            currentDirection.z * enemySpeed
        );
    }

    void HandleStopping()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        anim.SetBool("isWalking", false);
    }

    IEnumerator AttackRoutine()
    {
        currentState = EnemyState.Attacking;

        burstCounter++;
        currentColor = (currentColor == 1) ? 0 : 1; // Inverte a cor
        ApplyEnemyMaterial();
        
        if (playerTargetTransform != null)
        {
            Vector3 targetXZ = playerTargetTransform.position;
            targetXZ.y = transform.position.y;
            Vector3 lookDirection = targetXZ - transform.position;

            if(lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        anim.SetBool("isAttacking", true); 
        anim.SetTrigger("Attack");

        for (int i = 0; i < shotsPerBurst; i++)
        {
            ShootBullet(); 
            yield return new WaitForSeconds(fireRate);
        }

        currentState = EnemyState.CoolingDown;
        anim.SetBool("isAttacking", false);
        
        yield return new WaitForSeconds(cooldownTime); 

        currentState = EnemyState.Chasing;
        attackCoroutine = null; 
    }

    void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();
        Material targetMaterial = GetMaterialForColor(); 

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
        Material targetMaterial = GetMaterialForColor();
        if (myRenderer != null && targetMaterial != null)
        {
            myRenderer.material = targetMaterial;
        }
    }

    // 🚨 CORREÇÃO CS0103: Usa gameControllerRef (variável de classe)
    Material GetMaterialForColor()
    {
        if (gameControllerRef is GameControllerScript standardController)
        {
            return (currentColor == 1) ? standardController.PlayerMatFirst : standardController.PlayerMatSecond;
        }
        
        if (gameControllerRef is TutorialController tutorialController)
        {
            return (currentColor == 1) ? tutorialController.MatFirst : tutorialController.MatSecond;
        }
        return null;
    }

    public void TakingDamage(int bulletDamage, int bulletColor)
    {
        // Seu código já estava correto, aceitando os 2 argumentos
        Hp -= bulletDamage;
        Debug.Log("Louva-Deus recebeu " + bulletDamage + " de dano. Vida restante: " + Hp);

        if (Hp <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        
        Destroy(gameObject);
    }
}