using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public enum BettleState { Chasing, PreparingAttack, CoolingDown }

public class BettleEnemyScript : MonoBehaviour
{
    [Header("Configurações Base")]
    [SerializeField] int Hp = 40;
    [SerializeField] float enemySpeed = 2.5f;
    
    [Header("Lógica de Perseguição")]
    [SerializeField] float stopAndShootDistance = 12f; // Distância para parar e atacar
    
    [Header("Ataque Rolabosta")]
    [SerializeField] GameObject bulletPrefab; // Projétil grande
    [SerializeField] Transform firePoint;
    [SerializeField] float prepTime = 1.5f; // Tempo para "montar" a bosta antes de atirar
    [SerializeField] float cooldownTime = 10f; // Longo cooldown

    [Header("Cor e Referências")]
    public int currentColor; // Cor atual (0 ou 1)
    private Renderer myRenderer;
    private Rigidbody rb;

    // --- REFERÊNCIAS FLEXÍVEIS (Híbridas) ---
    // Usamos 'object' ou 'MonoBehaviour' para armazenar a referência dinâmica e fazer a conversão no uso.
    private MonoBehaviour playerReference; 
    private MonoBehaviour gameControllerReference;
    // ----------------------------------------
    
    private BettleState currentState = BettleState.Chasing;
    private Coroutine attackRoutineInstance;
    
    void Start()
    {
        // --- 1. Obter Componentes Essenciais ---
        rb = GetComponent<Rigidbody>();
        myRenderer = GetComponent<Renderer>();
        if (rb == null) Debug.LogError("Rigidbody faltando no Besouro!");

        // --- 2. CONEXÃO HÍBRIDA (Player e Controller) ---
        
        // Tenta encontrar o Controller de TESTE (prioridade)
        if (TESTGameController.controller != null)
        {
            gameControllerReference = TESTGameController.controller;
            playerReference = TESTGameController.controller.Player;
            Debug.Log("Besouro: Conectado ao TESTGameController.");
        }
        // Tenta encontrar o Controller PADRÃO
        else 
        {
            gameControllerReference = GameControllerScript.controller;
            playerReference = GameControllerScript.controller.Player;
            Debug.Log("Besouro: Tentando usar referências Padrão.");
        }
        
        // --- 3. Inicialização de Estado ---
        if (playerReference != null)
        {
            currentState = BettleState.Chasing;
            InitializeColor();
        }
        else
        {
            currentState = BettleState.PreparingAttack; // Fica parado se não achar o Player
            Debug.LogError("Player (TESTPlayer ou Padrão) não encontrado na cena!");
        }
    }
    
    void InitializeColor()
    {
        // Escolhe a cor aleatoriamente (0 ou 1) para o resto da vida do inimigo
        currentColor = Random.Range(0, 2); 
        
        var standardController = gameControllerReference as GameControllerScript;
        
        if (standardController != null)
        {
            SetEnemyColor(standardController);
        }
    }

    // --- Métodos de Update e Estados ---

    void Update()
    {
        if (playerReference == null) return;
        
        switch (currentState)
        {
            case BettleState.Chasing:
                HandleChasing();
                break;
            
            case BettleState.PreparingAttack:
            case BettleState.CoolingDown:
                HandleStopping(); 
                break;
        }
    }

    void HandleChasing()
    {
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

        // Condição de Ataque
        if (distance <= stopAndShootDistance)
        {
            currentState = BettleState.PreparingAttack;
            attackRoutineInstance = StartCoroutine(AttackRoutine());
            return; 
        }

        // Movimento e Rotação
        direction.y = 0f;
        direction = direction.normalized;
        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
        rb.linearVelocity = new Vector3(direction.x * enemySpeed, rb.linearVelocity.y, direction.z * enemySpeed);
    }

    void HandleStopping()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        Vector3 playerPosition = GetPlayerPosition();
        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
    }
    
    // --- Lógica de Ataque ---

    IEnumerator AttackRoutine()
    {
        // 1. Preparação (PreparingAttack)
        yield return new WaitForSeconds(prepTime); 

        // 2. Disparo
        Shoot();
        
        // 3. Cooldown (CoolingDown)
        currentState = BettleState.CoolingDown;
        yield return new WaitForSeconds(cooldownTime); 

        // 4. Recomeço
        currentState = BettleState.Chasing;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        // Tenta obter o Controller de TESTE
        var standardController = gameControllerReference as GameControllerScript; 

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BulletController bulletScript = newBullet.GetComponent<BulletController>();

        if (bulletScript != null)
        {
            bulletScript.isFiredByPlayer = false;
            bulletScript.bulletColor = currentColor;

            Renderer bulletRenderer = newBullet.GetComponent<Renderer>();
            if (bulletRenderer != null)
            {
                Material targetMaterial = null;

                if (standardController != null)
                {
                    targetMaterial = (currentColor == 1) ?
                        standardController.PlayerMatFirst :
                        standardController.PlayerMatSecond;
                }
                
                if (targetMaterial != null)
                {
                    bulletRenderer.material = targetMaterial;
                }
            }
        }
    }
    
    void SetEnemyColor(GameControllerScript Controller)
    {
        if (myRenderer != null && gameControllerReference != null)
        {
            myRenderer.material = (currentColor == 1) ?
                GameControllerScript.controller.PlayerMatFirst:
                GameControllerScript.controller.PlayerMatSecond;
        }
    }

    // --- Lógica de Dano/Morte ---

    public void TakingDamage(int bulletDamage, int bulletColor)
    {
    // A checagem de cor foi feita na bala (BulletController).
    // Aqui, apenas subtraímos o HP.
    Hp -= bulletDamage;
    Debug.Log("Besouro recebeu " + bulletDamage + " de dano. Vida restante: " + Hp);

    if (Hp <= 0)
    {
        Die();
    }
}

    void Die()
    {
        // Notificação de Morte (Usando Referência Híbrida)
        var testController = gameControllerReference as TESTGameController;
        // var standardController = gameControllerReference as GameControllerScript;

        if (testController != null)
        {
            // testController.AumentarNumerodeInimigosMortos(); // Atualmente comentado
        }
        // else if (standardController != null)
        // {
        //     // standardController.AumentarNumerodeInimigosMortos(); 
        // }
        
        if (attackRoutineInstance != null) StopCoroutine(attackRoutineInstance);
        
        Destroy(gameObject);
    }

    // --- Métodos Auxiliares ---
    
    Vector3 GetPlayerPosition()
    {
        // Acessa a posição do player de forma segura, seja ele TESTPlayer ou Player Padrão
        if (playerReference != null)
        {
            return playerReference.transform.position;
        }
        return Vector3.zero;
    }
}