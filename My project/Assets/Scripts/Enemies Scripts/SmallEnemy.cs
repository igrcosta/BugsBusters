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

    [SerializeField] GameObject redBulletPrefab;   // Prefab de Bala VERMELHA
    [SerializeField] GameObject greenBulletPrefab; // Prefab de Bala VERDE

    [SerializeField] int bulletsInCircle = 8;

    [SerializeField] float bulletDelay = 0.5f;

    [SerializeField] float bulletScale = 0.05f; // 💡 NOVO: Controla o tamanho do projétil


    [Header("Visual, Componentes e Rotação")]

    [SerializeField] GameObject explosionVisualPrefab;

    [SerializeField] float rotationSpeed = 10f; // 💡 NOVO: Para rotação suave

    private Rigidbody rb;

   

    // --- REFERÊNCIAS DINÂMICAS ---

    private Transform playerTargetTransform; // Unificado (Player ou PlayerTutorialRef)

    private Component gameControllerRef; // Unificado (GameControllerScript ou TutorialController)

    // ----------------------------

   

    private Coroutine explosionCoroutine;

    private bool isDying = false;

    private ColorHandler visualColorHandler;


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

        visualColorHandler = GetComponent<ColorHandler>();

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

        ColorHandler myColorHandler = GetComponent<ColorHandler>();

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

       

        // INICIA O TIMER
if (distance <= explosionRadius && explosionCoroutine == null)
{   
    explosionCoroutine = StartCoroutine(CountdownToExplosion());
}

// CANCELA O TIMER
else if (distance > safeReturnDistance && explosionCoroutine != null)
{
    StopCoroutine(explosionCoroutine);
    explosionCoroutine = null;
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


       

        yield return new WaitForSeconds(bulletDelay);

       

        ShootCircleOfBullets();

       

        // Destrói o objeto APÓS o disparo.

        DieVisualsAndDestroy();

    }


    // --- Lógica de Dano e Morte ---

   

    public void TakingDamage(int bulletDamage)

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
    // o Visual da Explosão
    if (explosionVisualPrefab != null)
    {
        GameObject visualGO = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
        
        ExplosionVisual visualScript = visualGO.GetComponent<ExplosionVisual>();
        if (visualScript != null)
        {
            visualScript.Initialize(damageRadius);  
        }
    }

    visualColorHandler.myRenderer.enabled = false;
    //SOME COM ELE VISUALMENTE PRA DAR A IDEIA DE QUE ELE SE EXPLODIU

    // Dano/Repulsão SOMENTE AO PLAYER
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);

    foreach (var hitCollider in hitColliders)
    {
        if (hitCollider.gameObject == gameObject) continue;

        // Tenta obter o componente Player
        Player player = hitCollider.GetComponent<Player>();

        if (player != null)
        {
            // APLICA REPULSÃO NO PLAYER
            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                // Usamos o AddExplosionForce para a repulsão
                hitRb.AddExplosionForce(explosionForce, transform.position, damageRadius, 1f, ForceMode.Impulse);
            }

            // APLICA DANO NO PLAYER
            player.TakingDamage(explosionDamage);
        }
        
        // Todos os outros objetos, incluindo o Boss, são ignorados (não recebem repulsão nem dano).
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
    ColorHandler myColorHandler = GetComponent<ColorHandler>(); 
    if (myColorHandler == null) return;
    
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

    if (prefabToInstantiate == null) return; // Sai se o prefab não for encontrado
    
    float angleStep = 360f / bulletsInCircle;
    
    for (int i = 0; i < bulletsInCircle; i++)
    {
        float angle = i * angleStep;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);  

        // 2. INSTANCIA O PREFAB SELECIONADO (prefabToInstantiate)
        GameObject newBullet = Instantiate(prefabToInstantiate, transform.position, rotation);

        newBullet.transform.localScale = Vector3.one * bulletScale;  
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();

        if (bulletScript == null) continue;

        bulletScript.isFiredByPlayer = false;
        // 3. Atribui a cor correta
        bulletScript.bulletColor = enemyColor; 
    }
}
} 