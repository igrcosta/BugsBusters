 using UnityEngine;

using System.Collections;

using System.Collections.Generic;


public class SmallEnemy : MonoBehaviour

{
    private TutorialManager tutorialManager;

    // ====================================================================

    // 1. CONFIGURAÇÕES & REFERÊNCIAS

    // ====================================================================


    [Header("Configurações Base")]

    [SerializeField] int Hp = 10;

    [SerializeField] float enemySpeed = 4.0f;  

   

    [Header("Lógica de Explodir em Proximidade")]

    [SerializeField] float explosionRadius = 5.0f; // Distância para começar a explodir

    [SerializeField] float safeReturnDistance = 8.0f; // Distância de histerese

    [SerializeField] float explosionTimer = 1.0f; // Tempo para explodir após se aproximar

    [SerializeField] float damageRadius = 8.0f; // Raio onde a explosão causa dano/repulsão

   

    [Header("Ataque e Repulsão")]

    [SerializeField] int explosionDamage = 20;

    [SerializeField] float explosionForce = 500f;

   

    [Header("Disparo de Balas ao Morrer")]

    [SerializeField] GameObject bulletPrefab;

    [SerializeField] int bulletsInCircle = 8;

    [SerializeField] float bulletDelay = 0.5f;

    [SerializeField] float bulletScale = 0.05f; // 💡 NOVO: Controla o tamanho do projétil


    [Header("Visual, Componentes e Rotação")]

    [SerializeField] GameObject explosionVisualPrefab;

    [SerializeField] float rotationSpeed = 10f; // 💡 NOVO: Para rotação suave

    public int currentColor;

    private Renderer myRenderer;

    private Rigidbody rb;

   

    // --- REFERÊNCIAS DINÂMICAS ---

    private Transform playerTargetTransform; // Unificado (Player ou PlayerTutorialRef)

    private Component gameControllerRef; // Unificado (GameControllerScript ou TutorialController)

    // ----------------------------

   

    private Coroutine explosionCoroutine;

    private bool isDying = false;


    // ====================================================================

    // 2. INICIALIZAÇÃO

    // ====================================================================


public void SetManager(TutorialManager manager)
{
    tutorialManager = manager;
}


    void Awake()

    {

        rb = GetComponent<Rigidbody>();

        myRenderer = GetComponent<Renderer>();

    }


    void Start()

    {

        StartCoroutine(WaitForInitialization());

    }

   

    // Coroutine para inicializar referências de forma segura (como nas versões anteriores)

    IEnumerator WaitForInitialization()

    {

        while (GameControllerScript.controller == null && TutorialController.controller == null)

        {

            yield return null;

        }


        FindTargetAndController();

       

        while (playerTargetTransform == null)

        {

            FindTargetAndController();

            yield return null;

        }

       

        InitializeColor();

        Debug.Log(gameObject.name + ": Inicialização de SmallEnemy bem-sucedida.");

    }


    // Unifica a busca de referências para Player e Controller (suporta GameController e TutorialController)

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

    // 3. MOVIMENTO E LÓGICA DE EXPLOSÃO (CORRIGIDA)

    // ====================================================================


    void Update()

    {

        if (playerTargetTransform == null || isDying)  

        {

            if (rb != null) rb.linearVelocity = Vector3.zero;

            return;

        }

       

        Vector3 playerPosition = playerTargetTransform.position;

        Vector3 direction = playerPosition - transform.position;

        float distance = direction.magnitude;


        // 1. Rotação Suave (Slerp) e Estável (Horizontal)

        // 💡 CORRIGIDO: Substitui transform.LookAt pelo Slerp horizontal para evitar o flickering

        if (explosionCoroutine == null)

        {

            Vector3 targetXZ = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);

            Vector3 lookDirection = targetXZ - transform.position;


            if (lookDirection.sqrMagnitude > 0.01f)

            {

                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            }

        }


        // 2. Lógica do Timer de Explosão (Histerese)

       

        // INICIA O TIMER (e a parada será gerenciada pelo FixedUpdate)

        if (distance <= explosionRadius && explosionCoroutine == null)

        {

            // 💡 Acrescenta a inversão de cor para feedback visual imediato

            currentColor = (currentColor == 1) ? 0 : 1;

            ApplyEnemyMaterial(myRenderer);

           

            explosionCoroutine = StartCoroutine(CountdownToExplosion());

        }

        // CANCELA O TIMER (Se saiu da zona de segurança)

        else if (distance > safeReturnDistance && explosionCoroutine != null)

        {

            StopCoroutine(explosionCoroutine);

            explosionCoroutine = null;

            // 💡 Restaura a cor original ao cancelar a explosão

            currentColor = (currentColor == 1) ? 0 : 1;

            ApplyEnemyMaterial(myRenderer);

        }

    }

   

    void FixedUpdate()

    {

         if (playerTargetTransform == null || isDying) return;


         Vector3 playerPosition = playerTargetTransform.position;

         Vector3 direction = playerPosition - transform.position;

         float distance = direction.magnitude;

         

         // MOVIMENTO: Persegue APENAS se a coroutine for nula E estiver fora do raio de explosão.

         if (explosionCoroutine == null && distance > explosionRadius)

         {

             HandleChasing(direction);

         }

         else

         {

             // Garante que o inimigo pare totalmente enquanto o timer roda ou se atingiu o raio de explosão.

             HandleStopping();  

         }

    }


    void HandleChasing(Vector3 direction)

    {

        direction.y = 0f;

        direction = direction.normalized;

        rb.linearVelocity = new Vector3(direction.x * enemySpeed, rb.linearVelocity.y, direction.z * enemySpeed);

    }

   

    void HandleStopping()

    {

        rb.linearVelocity = Vector3.zero;

    }


    // ====================================================================

    // 4. ROTINAS DE EXPLOSÃO/MORTE

    // ====================================================================


    // Morte por Proximidade (Auto-explosão)

    IEnumerator CountdownToExplosion()

    {

        yield return new WaitForSeconds(explosionTimer);

       

        // 💡 CHECAGEM DE NULO: Garante que o objeto não foi destruído durante o yield.

        if (this == null) yield break;


        if (!isDying)  

        {

            isDying = true;

            StartCoroutine(ExplodeOnDeathRoutine());

        }

    }

   

    // Morte por Dano do Player (Tiro) ou Fim do Timer

    IEnumerator ExplodeOnDeathRoutine()

    {

        ExplodeAreaDamage();  

        myRenderer.enabled = false;

       

        yield return new WaitForSeconds(bulletDelay);

       

        ShootCircleOfBullets();

       

        // Destrói o objeto APÓS o disparo.

        DieVisualsAndDestroy();

    }


    // --- Lógica de Dano e Morte ---

   

    public void TakingDamage(int bulletDamage, int bulletColor)

    {

        if (isDying) return;  

       

        Hp -= bulletDamage;


        if (Hp <= 0)

        {

            isDying = true;

           

            if (explosionCoroutine != null) StopCoroutine(explosionCoroutine);

            explosionCoroutine = null;  


            // Inicia a rotina de explosão causada por Dano (Bala)

            StartCoroutine(ExplodeOnDeathRoutine());

        }

    }


    void ExplodeAreaDamage()

    {

        // 1. Instancia o Visual da Explosão

        if (explosionVisualPrefab != null)

        {

            GameObject visualGO = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

           

            ExplosionVisual visualScript = visualGO.GetComponent<ExplosionVisual>();

            if (visualScript != null)

            {

                visualScript.Initialize(damageRadius);  

            }

        }


        // 2. Aplica Dano/Repulsão

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);


        foreach (var hitCollider in hitColliders)

        {

            if (hitCollider.gameObject == gameObject) continue;


            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();

            if (hitRb != null)

            {

                 // Usamos o AddExplosionForce, que é mais robusto

                 hitRb.AddExplosionForce(explosionForce, transform.position, damageRadius, 1f, ForceMode.Impulse);

            }


            // Dano ao Player

            hitCollider.GetComponent<Player>()?.ReceiveDamage(explosionDamage);

           

            // Dano a Outros Inimigos (Usando o padrão de 2 argumentos para Bettle/SmallEnemy)

            hitCollider.GetComponent<Enemy1>()?.TakingDamage(explosionDamage, 0);

            hitCollider.GetComponent<BettleEnemyScript>()?.TakingDamage(explosionDamage, 0);

           

            SmallEnemy otherSmallEnemy = hitCollider.GetComponent<SmallEnemy>();

            if (otherSmallEnemy != null && otherSmallEnemy != this)  

            {

                 otherSmallEnemy.TakingDamage(explosionDamage, 0);  

            }

        }

    }

   

    void DieVisualsAndDestroy()
    {
        // NOVO: 1. Tenta notificar o TutorialController (se estiver no tutorial)
        if (gameControllerRef is TutorialController tutorialController)
        {
            // Assume que o método é 'EnemyKilled()' para avançar a fase
            tutorialController.EnemyKilled();
        }
        // 2. Mantém a lógica existente para o GameController (jogo principal)
        else if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        myRenderer.enabled = false;
        
        if (rb != null) rb.linearVelocity = Vector3.zero;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (tutorialManager != null)
    tutorialManager.EnemyKilled();
        Debug.Log($"ENEMY KILLED: {gameObject.name}");
        
        Destroy(gameObject, 0.1f);
    }

   

    void ShootCircleOfBullets()

    {

        if (bulletPrefab == null) return;

       

        float angleStep = 360f / bulletsInCircle;

       

        Material targetMaterial = GetTargetMaterial();

       

        for (int i = 0; i < bulletsInCircle; i++)

        {

            float angle = i * angleStep;

            Quaternion rotation = Quaternion.Euler(0, angle, 0);  


            GameObject newBullet = Instantiate(bulletPrefab, transform.position, rotation);

           

            // 💡 CORRIGIDO: Aplica a escala para garantir o tamanho dos projéteis

            newBullet.transform.localScale = Vector3.one * bulletScale;  

           

            BulletController bulletScript = newBullet.GetComponent<BulletController>();


            if (bulletScript == null) continue;

           

            bulletScript.isFiredByPlayer = false;

            bulletScript.bulletColor = currentColor;


            Renderer bulletRenderer = newBullet.GetComponent<Renderer>();

            if (bulletRenderer != null && targetMaterial != null)

            {

                bulletRenderer.material = targetMaterial;

            }

        }

    }


    // ====================================================================

    // 5. MÉTODOS AUXILIARES

    // ====================================================================


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

   

    Material GetTargetMaterial()

    {

        // Unifica a busca de material (suporta GameController e TutorialController)

        if (gameControllerRef is GameControllerScript standardController)

        {

            return (currentColor == 1) ? standardController.PlayerMatFirst : standardController.PlayerMatSecond;

        }

        else if (gameControllerRef is TutorialController tutorialController)

        {

             // Assumindo que TutorialController possui as variáveis MatFirst e MatSecond

             return (currentColor == 1) ? tutorialController.MatFirst : tutorialController.MatSecond;

        }

        return null;

    }

} 