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
        
        // Tenta encontrar o Controller de TESTE (se ele existir na sua cena)
        if (TESTGameController.controller != null)
        {
            gameControllerReference = TESTGameController.controller;
            playerReference = TESTGameController.controller.Player;
            Debug.Log("Besouro: Conectado ao TESTGameController.");
        }
        // Tenta encontrar o Controller PADRÃO
        else if (GameControllerScript.controller != null)
        {
            gameControllerReference = GameControllerScript.controller;
            playerReference = GameControllerScript.controller.Player;
            Debug.Log("Besouro: Conectado ao GameController Padrão.");
        }
        else
        {
            Debug.LogError("GameController (TEST ou Padrão) não encontrado!");
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
            Debug.LogError("Player não encontrado para o Besouro.");
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
        // CORREÇÃO: Usar rb.velocity
        rb.linearVelocity = new Vector3(direction.x * enemySpeed, rb.linearVelocity.y, direction.z * enemySpeed);
    }

    void HandleStopping()
    {
        // CORREÇÃO: Usar rb.velocity
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
        if (bulletPrefab == null || firePoint == null) 
        {
            Debug.LogError("Faltando Prefab ou FirePoint no Besouro.");
            return;
        }
        
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
        // CORREÇÃO: Usar a referência 'Controller' recebida por argumento.
        if (myRenderer != null && Controller != null)
        {
            myRenderer.material = (currentColor == 1) ?
                Controller.PlayerMatFirst:
                Controller.PlayerMatSecond;
        }
    }

    // --- Lógica de Dano/Morte ---

    public void TakingDamage(int bulletDamage, int bulletColor)
    {
        Hp -= bulletDamage;
        Debug.Log("Besouro recebeu " + bulletDamage + " de dano. Vida restante: " + Hp);

        if (Hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // CORREÇÃO CRÍTICA: Notificar o GameController Padrão sobre a morte
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        else
        {
            Debug.LogError("GameController NULO no momento da morte do Besouro!");
        }
        
        if (attackRoutineInstance != null) StopCoroutine(attackRoutineInstance);
        
        Destroy(gameObject);
    }

    // --- Métodos Auxiliares ---
    
    Vector3 GetPlayerPosition()
    {
        if (playerReference != null)
        {
            return playerReference.transform.position;
        }
        return Vector3.zero;
    }
}