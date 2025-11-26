using UnityEngine;

public class BulletController : MonoBehaviour
{
    // variáveis da forma como o tiro vai se comportar
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 7f;

    public int bulletColor;
    public bool isFiredByPlayer = true;

    public int PLayerDamage = 10;
    public int EnemyDamage  = 5;
    public AudioSource AudioDamage;


    void Start()
    {
        // depois do tempo de lifetime, a bala que possui esse script será destruída
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Movimentação da bala
        transform.Translate(transform.forward * bulletSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ... (Seu código de OnTriggerEnter permanece o mesmo, pois as correções estão nos auxiliares)

        BulletController otherBullet = other.GetComponent<BulletController>();

        string hitTag = other.tag;

        // --- COLISÃO ENTRE BALAS ---
        if(otherBullet != null)
        {
            if (this.isFiredByPlayer != otherBullet.isFiredByPlayer)
            {
                Destroy(this.gameObject); 
                return;
            }
            else
            {
                return; // Ignora colisão entre balas do mesmo lado
            }
        }

        // --- REGRAS DE IGNORAR ---
        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            return; 
        }

        // --- LÓGICA DE DANO: PLAYER ATIROU (isFiredByPlayer == true) ---
        if (isFiredByPlayer && hitTag == "Enemy")
        {
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            BettleEnemyScript bettleEnemy = other.GetComponent<BettleEnemyScript>();
            SmallEnemy smallEnemy = other.GetComponent<SmallEnemy>(); 

            if (enemy1 != null)
            {
                ApplyDamageToEnemy1(enemy1);
            }
            else if (bettleEnemy != null)
            {
                ApplyDamageToBettle(bettleEnemy); 
            }
            else if (smallEnemy != null)
            {
                ApplyDamageToSmallEnemy(smallEnemy);
            }
            
          
        }

        // --- LÓGICA DE DANO: INIMIGO ATIROU (isFiredByPlayer == false) ---
        if (!isFiredByPlayer && hitTag == "Player")
        {
            TESTPlayer testPlayer = other.GetComponent<TESTPlayer>();
            Player standardPlayer = other.GetComponent<Player>();

            if (testPlayer != null)
            {
                ApplyDamageToPlayer(testPlayer, testPlayer.currentColor);
            }
            else if (standardPlayer != null)
            {
                ApplyDamageToStandardPlayer(standardPlayer, standardPlayer.currentColor); 
            }
            
            if (testPlayer != null || standardPlayer != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Se encostou em qualquer outra coisa (parede, etc.), destrói a bala
        Destroy(gameObject);
    }

    // --- Métodos Auxiliares para Aplicação de Dano ---

    private void ApplyDamageToEnemy1(Enemy1 enemy)
    {
        if (enemy.currentColor != bulletColor)
        {
            enemy.TakingDamage(PLayerDamage, bulletColor); // ✅ CORREÇÃO CS7036
            Debug.Log("DANO Enemy1: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Enemy1 mesma cor da bala");
        }
    }

    private void ApplyDamageToBettle(BettleEnemyScript bettle)
    {
        if (bettle.currentColor != bulletColor)
        {
            bettle.TakingDamage(PLayerDamage, bulletColor); // ✅ CORREÇÃO CS7036
            Debug.Log("DANO Bettle: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Besouro mesma cor da bala");
        }
    }
    
    private void ApplyDamageToSmallEnemy(SmallEnemy smallEnemy)
    {
        if (smallEnemy.currentColor != bulletColor)
        {
            smallEnemy.TakingDamage(PLayerDamage, bulletColor); // ✅ CORREÇÃO CS7036
            Debug.Log("DANO SmallEnemy: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, SmallEnemy mesma cor da bala");
        }
    }
    
    // Método para o Player de TESTE
    private void ApplyDamageToPlayer(TESTPlayer player, int playerCurrentColor)
    {
        if (playerCurrentColor != bulletColor)
        {
            player.ReceiveDamage(EnemyDamage);
            Debug.Log("DANO Player TEST: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Player TEST mesma cor da bala");
        }
    }

    // Método para o Player PADRÃO
    private void ApplyDamageToStandardPlayer(Player player, int playerCurrentColor)
    {
        if (playerCurrentColor != bulletColor)
        {
            player.ReceiveDamage(EnemyDamage); 
            Debug.Log("DANO Player PADRÃO: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Player PADRÃO mesma cor da bala");
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AudioDamage.Play();
        }
    }

}