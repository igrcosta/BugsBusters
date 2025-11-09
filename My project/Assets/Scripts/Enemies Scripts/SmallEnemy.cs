using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmallEnemy : MonoBehaviour
{
    [Header("Configurações Base")]
    [SerializeField] int Hp = 10;
    [SerializeField] float enemySpeed = 4.0f; 

    [Header("Lógica de Explodir em Proximidade")]
    [SerializeField] float explosionRadius = 5.0f; // Distância para começar a explodir
    [SerializeField] float safeReturnDistance = 0.8f; 
    [SerializeField] float explosionTimer = 1.0f; // Tempo para explodir após se aproximar
    [SerializeField] float damageRadius = 8.0f; // Raio onde a explosão causa dano/repulsão

    [Header("Ataque e Repulsão")]
    [SerializeField] int explosionDamage = 20; // Dano que a explosão causa (Player e Inimigo)
    [SerializeField] float explosionForce = 500f; // Força da repulsão

    [Header("Disparo de Balas ao Morrer")]
    [SerializeField] GameObject bulletPrefab; // O Prefab da bala 
    [SerializeField] int bulletsInCircle = 8; // Quantidade de balas para o círculo
    [SerializeField] float bulletDelay = 0.5f; // Delay entre a explosão e o disparo das balas

    [Header("Cor e Referências")]
    [SerializeField] GameObject explosionVisualPrefab;
    public int currentColor;
    private Renderer myRenderer;
    private Rigidbody rb;
    private Player playerReference; 
    private GameControllerScript gameControllerReference;

    private Coroutine explosionCoroutine;

    private bool isDying = false;
    //bool pra controlar estado de morte

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        TryInitializeRefferences();
    }

    void TryInitializeRefferences()
    {
        if (GameControllerScript.controller != null)
        {
            gameControllerReference = GameControllerScript.controller;
            playerReference = gameControllerReference.Player; 
            
            if (playerReference != null) 
            {
                // TUDO CERTO: Agora que temos o Player, podemos inicializar o resto
                InitializeColor();
                Debug.Log(gameObject.name + ": Inicialização de SmallEnemy bem-sucedida.");
            }
            else
            {
                // REFERÊNCIA NULA: Isso era a causa provável do crash
                Debug.LogError(gameObject.name + ": Player é NULO no GameController. SmallEnemy ficará parado.");
                // Deixa o playerReference nulo. O Update() vai sair (return)
            }
        }
        else
        {
            Debug.LogError(gameObject.name + ": GameController NULO no Start!");
        }
    }

    void Update()
    {
        // Ponto de segurança contra referências nulas ou se o inimigo está morrendo
        if (playerReference == null || rb == null || isDying) 
        {
            if (rb != null) 
            {
                // Garante que o inimigo para se a referência ou estado for inválido.
                rb.linearVelocity = Vector3.zero;
            }
            return;
        }
        
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

        // Rotação: Gira o inimigo para olhar o Player
        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));

        // --- Lógica do Timer de Explosão (Histerese) ---
        
        // 1. INICIA O TIMER e PARA
        if (distance <= explosionRadius && explosionCoroutine == null)
        {
            HandleStopping(); 
            explosionCoroutine = StartCoroutine(CountdownToExplosion());
        }
        // 2. CANCELA O TIMER e VOLTA A PERSEGUIR (Se saiu da zona de segurança)
        else if (distance > safeReturnDistance && explosionCoroutine != null)
        {
            Debug.Log("Saindo da zona de explosão, cancelando timer.");
            StopCoroutine(explosionCoroutine);
            explosionCoroutine = null;
            // [TODO: RESET VISUAL AQUI]
        }

        // 3. MOVIMENTO: Persegue APENAS se o timer estiver desativado E se estiver longe da zona de parada.
        if (explosionCoroutine == null && distance > explosionRadius)
        {
            HandleChasing(direction);
        }
        else if (explosionCoroutine != null)
        {
            // Garante que o inimigo pare totalmente enquanto o timer roda.
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
        // [TODO: FEEDBACK VISUAL/SOM AQUI (Ex: Creeper piscando)]
    }

    // --- Rotinas de Explosão/Morte ---

    // Morte por Proximidade (Auto-explosão)
    IEnumerator CountdownToExplosion()
    {
        yield return new WaitForSeconds(explosionTimer);
        
        // Se ainda não estiver morrendo (para evitar duplicação em caso de hit de bala no último frame)
        if (!isDying) 
        {
             isDying = true;
             // Chama a mesma rotina de morte/disparo
             StartCoroutine(ExplodeOnDeathRoutine());
        }
        // NOTA: Não chame Die() ou Destroy aqui. ExplodeOnDeathRoutine fará isso.
    }
    
    // Morte por Dano do Player (Tiro)
    IEnumerator ExplodeOnDeathRoutine()
    {
        // 1. Aplica o Dano/Repulsão da Esfera de Explosão
        ExplodeAreaDamage(); 
        myRenderer.enabled = false;
        
        // 2. Espera o delay para o disparo
        yield return new WaitForSeconds(bulletDelay);
        
        // 3. Spawna o círculo de balas
        ShootCircleOfBullets();
        
        // 4. Notificação de Morte (APÓS o disparo)
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        // 5. Destrói o objeto APÓS o disparo e a notificação.
        DieVisualsAndDestroy();
    }

    // --- Lógica de Dano e Morte ---
    
    public void TakingDamage(int bulletDamage, int bulletColor)
    {
        // CORREÇÃO CRÍTICA: Ignorar dano se o inimigo já estiver morrendo
        if (isDying) return; 
        
        Hp -= bulletDamage;

        if (Hp <= 0)
        {
            isDying = true; // Define a flag
            
            // Se a coroutine de auto-explosão (proximidade) estava rodando, cancela.
            if (explosionCoroutine != null) StopCoroutine(explosionCoroutine);
            explosionCoroutine = null; 

            // Inicia a rotina de explosão causada por Dano (Bala)
            StartCoroutine(ExplodeOnDeathRoutine());
        }
    }

    void ExplodeAreaDamage()
{
    // Cria a esfera de colisão no raio de dano
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);

    foreach (var hitCollider in hitColliders)
    {
        // Ponto de segurança: Não cause dano a si mesmo
        if (hitCollider.gameObject == gameObject)
        {
            continue; // Pula para o próximo collider
        }

        // --- 1. Lógica de Repulsão (MUITO Importante) ---
        // Pega o Rigidbody, SE existir.
        Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
        if (hitRb != null)
        {
            // Aplica a repulsão. Usar Força é mais seguro do que AddExplosionForce se a física estiver instável.
            // AddExplosionForce é o método ideal se a física estiver OK, mas vamos tentar uma versão mais simples
            // para evitar o crash.
            
            // Calculamos a direção para aplicar a força
            Vector3 explosionDir = hitCollider.transform.position - transform.position;
            // Garantimos que a força é aplicada para cima (eixo Y) e para fora (magnitude)
            float distanceFactor = 1f - (explosionDir.magnitude / damageRadius);
            
            // Aplica a força, garantindo um certo impulso vertical (eixo Y)
            hitRb.AddForce((explosionDir.normalized * explosionForce * distanceFactor) + (Vector3.up * explosionForce * 0.5f), ForceMode.Impulse);
            
            // OU, se você quiser manter a função padrão (e mais segura):
            // hitRb.AddExplosionForce(explosionForce, transform.position, damageRadius, 1f, ForceMode.Impulse);
        }

        // --- 2. Lógica de Dano ---
        if (explosionVisualPrefab != null)
    {
        // Instancia o objeto visual na posição do inimigo
        GameObject visualGO = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
        
        ExplosionVisual visualScript = visualGO.GetComponent<ExplosionVisual>();
        
        if (visualScript != null)
        {
            // Inicializa com o raio de dano
            visualScript.Initialize(damageRadius); 
        }
    }

        // Dano ao Player
        Player playerScript = hitCollider.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.ReceiveDamage(explosionDamage);
        }
        
        // Dano a Outros Inimigos
        Enemy1 enemy1Script = hitCollider.GetComponent<Enemy1>();
        BettleEnemyScript bettleScript = hitCollider.GetComponent<BettleEnemyScript>();
        SmallEnemy smallEnemyScript = hitCollider.GetComponent<SmallEnemy>();

        if (enemy1Script != null) 
        {
            enemy1Script.TakingDamage(explosionDamage);
        }
        if (bettleScript != null) 
        {
            // Assumindo que o método TakingDamage do BettleEnemyScript espera a cor da bala (0 para neutro/explosão)
            bettleScript.TakingDamage(explosionDamage, 0); 
        }
        // Dano em outros SmallEnemy (o 'this' é o SmallEnemy que está explodindo)
        if (smallEnemyScript != null && smallEnemyScript != this) 
        {
            smallEnemyScript.TakingDamage(explosionDamage, 0); 
        }
    }
    
    // [TODO: INSTANCIAR PARTICLE SYSTEM DA EXPLOSÃO AQUI]
    
    // Para dar a impressão de explosão (se você quiser um feedback visual imediato)
    // Você pode fazer o objeto do inimigo ficar invisível ou mudar de cor AQUI.
}
    void DieVisualsAndDestroy()
    {
        // Desativamos o Renderer e o Collider imediatamente
        myRenderer.enabled = false;
        
        if (rb != null) 
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Destrói o GameObject no final da rotina, com um pequeno delay de segurança.
        Destroy(gameObject, 0.1f);
    }
    
    void ShootCircleOfBullets()
    {
        if (bulletPrefab == null) 
        {
            Debug.LogError("Bullet Prefab não está anexado ao SmallEnemy.");
            return;
        }
        
        float angleStep = 360f / bulletsInCircle;
        
        var standardController = gameControllerReference as GameControllerScript; 
        Material targetMaterial = null;
        if (standardController != null)
        {
            targetMaterial = (currentColor == 1) ?
                standardController.PlayerMatFirst :
                standardController.PlayerMatSecond;
        }

        for (int i = 0; i < bulletsInCircle; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, angle, 0); 

            GameObject newBullet = Instantiate(bulletPrefab, transform.position, rotation);
            
            // CORREÇÃO DE ESCALA PARA VISUALIZAÇÃO
            newBullet.transform.localScale = Vector3.one * 0.05f; 
            
            BulletController bulletScript = newBullet.GetComponent<BulletController>();

            if (bulletScript == null) 
            {
                 Debug.LogError("O Prefab da bala NÃO tem um componente BulletController anexado!");
                 continue;
            }
            
            bulletScript.isFiredByPlayer = false;
            bulletScript.bulletColor = currentColor;

            Renderer bulletRenderer = newBullet.GetComponent<Renderer>();
            if (bulletRenderer != null && targetMaterial != null)
            {
                bulletRenderer.material = targetMaterial;
            }
        }
    }

    void Die()
    {
        // NOTA: Certifique-se de que a função Die só é chamada NO FINAL
        
        // Notifica o GameController
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        // Desativamos o Renderer e o Collider imediatamente
        myRenderer.enabled = false;
        // Ponto de segurança extra: Se o Rigidbody for nulo, não tente mexer nele
        if (rb != null) 
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Desativamos o collider para evitar mais interações de física
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Adicionamos um pequeno delay de 0.1s para permitir que o motor de física se recupere do loop.
        Destroy(gameObject, 0.1f);
    }
    
    // --- Métodos Auxiliares ---

    void InitializeColor()
    {
        currentColor = Random.Range(0, 2); 
        
        var standardController = gameControllerReference as GameControllerScript;
        
        if (standardController != null)
        {
            SetEnemyColor(standardController);
        }
    }
    
    void SetEnemyColor(GameControllerScript Controller)
    {
        if (myRenderer != null && Controller != null)
        {
            myRenderer.material = (currentColor == 1) ?
                Controller.PlayerMatFirst:
                Controller.PlayerMatSecond;
        }
    }
    
    Vector3 GetPlayerPosition()
    {
        if (playerReference != null)
        {
            return playerReference.transform.position;
        }
        return Vector3.zero;
    }
}