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
    private Animator anim;
    private Rigidbody rb;
    private Renderer myRenderer;

    [Header("Stats")]
    [SerializeField] int Hp = 20;
    [SerializeField] float enemySpeed = 15f;
    [SerializeField] float stoppingDistance = 1.5f;

    [Header("Alvo & Cena")]
    private Transform playerTargetTransform;
    private Component gameControllerRef;
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

    IEnumerator Start()
    {
        yield return null;
        tutorial = FindObjectOfType<TutorialController>();

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

    void InitializeColor()
    {
        currentColor = Random.Range(0, 2);
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
            anim.SetBool("isFalling", false);
            anim.SetTrigger("FallImpact");

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
        currentColor = (currentColor == 1) ? 0 : 1;
        ApplyEnemyMaterial();

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
    // 5. DANO E MATERIAIS
    // ====================================================================

    void ApplyEnemyMaterial()
    {
        Material targetMaterial = GetMaterialForColor();
        if (myRenderer != null && targetMaterial != null)
        {
            myRenderer.material = targetMaterial;
        }
    }

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
        Hp -= bulletDamage;

        Debug.Log("Louva-Deus recebeu " + bulletDamage + " de dano. Vida restante: " + Hp);

        if (Hp <= 0)
        {
            Die();
        }
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
}
