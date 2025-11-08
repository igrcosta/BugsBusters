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
    public int currentColor;
    private Renderer myRenderer;
    private Rigidbody rb;
    private MonoBehaviour playerReference; 
    private MonoBehaviour gameControllerReference;

    private Coroutine explosionCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRenderer = GetComponent<Renderer>();

        // CONExão GameController e Player
        if (GameControllerScript.controller != null)
        {
            gameControllerReference = GameControllerScript.controller;
            playerReference = GameControllerScript.controller.Player;
            
            // Checa se a referência do Player foi pega antes de inicializar a cor
            if (playerReference != null) 
            {
                InitializeColor();
            }
        }
    }

    void Update()
    {
        if (playerReference == null) return;
        
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 direction = playerPosition - transform.position;
        float distance = direction.magnitude;

        transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));

        // Lógica de Perseguição/Auto-explosão
        if (distance > explosionRadius)
        {
            // Persegue
            HandleChasing(direction);
            // Se estava explodindo, cancela
            if (explosionCoroutine != null)
            {
                StopCoroutine(explosionCoroutine);
                explosionCoroutine = null;
                // [TODO: RESET VISUAL AQUI]
            }
        }
        else
        {
            // Chegou perto, para e inicia a contagem
            HandleStopping(); 
            if (explosionCoroutine == null)
            {
                // Inicia o timer de explosão se ainda não estiver ativo
                explosionCoroutine = StartCoroutine(CountdownToExplosion());
            }
        }
    }
    
    void HandleChasing(Vector3 direction)
    {
        direction.y = 0f;
        direction = direction.normalized;
        // CORREÇÃO: Usar rb.velocity
        rb.linearVelocity = new Vector3(direction.x * enemySpeed, rb.linearVelocity.y, direction.z * enemySpeed);
    }
    
    void HandleStopping()
    {
        // CORREÇÃO: Usar rb.velocity
        rb.linearVelocity = Vector3.zero;
        // [TODO: FEEDBACK VISUAL/SOM AQUI (Ex: Creeper piscando)]
    }

    // --- Rotinas de Explosão/Morte ---

    // Morte por Proximidade (Auto-explosão)
    IEnumerator CountdownToExplosion()
    {
        yield return new WaitForSeconds(explosionTimer);
        
        // 1. Explode (causa dano e repulsão)
        ExplodeAreaDamage(); 
        
        // 2. Morte e notificação
        Die(); 
    }
    
    // Morte por Dano do Player (Tiro)
    IEnumerator ExplodeOnDeathRoutine()
    {
        // 1. Aplica o Dano/Repulsão da Esfera de Explosão
        ExplodeAreaDamage(); 
        
        // 2. Espera o delay
        yield return new WaitForSeconds(bulletDelay);
        
        // 3. Spawna o círculo de balas
        ShootCircleOfBullets();
        
        // 4. Morte e notificação
        Die();
    }

    // --- Lógica de Dano e Morte ---
    
    public void TakingDamage(int bulletDamage, int bulletColor)
    {
        Hp -= bulletDamage;

        if (Hp <= 0)
        {
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
            // Lógica de Repulsão
            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                // Aplica a repulsão (AddExplosionForce é ideal para este efeito)
                hitRb.AddExplosionForce(explosionForce, transform.position, damageRadius, 1f, ForceMode.Impulse);
            }

            // Lógica de Dano ao Player
            Player playerScript = hitCollider.GetComponent<Player>();
            if (playerScript != null)
            {
                // Dano direto sem checagem de cor (é uma explosão de área)
                playerScript.ReceiveDamage(explosionDamage);
            }
            
            // Lógica de Dano a Outros Inimigos (Incluindo SmallEnemy)
            Enemy1 enemy1Script = hitCollider.GetComponent<Enemy1>();
            BettleEnemyScript bettleScript = hitCollider.GetComponent<BettleEnemyScript>();
            SmallEnemy smallEnemyScript = hitCollider.GetComponent<SmallEnemy>();

            if (enemy1Script != null) enemy1Script.TakingDamage(explosionDamage);
            // Assumindo que o Besouro pode tomar dano de explosão sem checagem de cor
            if (bettleScript != null) bettleScript.TakingDamage(explosionDamage, 0); 
            
            // Causa dano em outros SmallEnemy (evita auto-dano)
            if (smallEnemyScript != null && smallEnemyScript != this) 
            {
                smallEnemyScript.TakingDamage(explosionDamage, 0); 
            }
        }
        
        // [TODO: INSTANCIAR PARTICLE SYSTEM DA EXPLOSÃO AQUI]
    }
    
    void ShootCircleOfBullets()
    {
        if (bulletPrefab == null) 
        {
            Debug.LogError("Bullet Prefab não está anexado ao SmallEnemy.");
            return;
        }
        
        float angleStep = 360f / bulletsInCircle;
        
        // Obtém o material para as balas
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
            
            // Rotação no plano Y
            Quaternion rotation = Quaternion.Euler(0, angle, 0); 

            GameObject newBullet = Instantiate(bulletPrefab, transform.position, rotation);
            BulletController bulletScript = newBullet.GetComponent<BulletController>();

            if (bulletScript != null)
            {
                bulletScript.isFiredByPlayer = false; // Bala inimiga
                bulletScript.bulletColor = currentColor;

                Renderer bulletRenderer = newBullet.GetComponent<Renderer>();
                if (bulletRenderer != null && targetMaterial != null)
                {
                    bulletRenderer.material = targetMaterial;
                }
            }
        }
    }

    void Die()
    {
        // Notifica o GameController
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
        }
        
        // Destrói o inimigo
        Destroy(gameObject);
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