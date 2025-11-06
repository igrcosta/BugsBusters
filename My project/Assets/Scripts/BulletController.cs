using UnityEngine;

public class BulletController : MonoBehaviour
{
    // variáveis da forma como o tiro vai se comportar
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 3f;

    public int bulletColor;
    public bool isFiredByPlayer = true;

    public int PLayerDamage = 10;
    public int EnemyDamage  = 5;

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
        BulletController otherBullet = other.GetComponent<BulletController>();

        string hitTag = other.tag;

        if(otherBullet != null)
        {
            //se uma bala foi atirada pelo player e a outra pelo inimigo
            if (this.isFiredByPlayer != otherBullet.isFiredByPlayer)
            {
                Destroy(this.gameObject);
                //destroi a bala do player

                return;
            }
            else
            {
                return;
                //igonora
            }
        }

        // Se a bala atingiu o Player que a disparou ou vice-versa (regra de ignorar)
        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            return; 
        }

        // --- LÓGICA DE DANO: PLAYER ATIROU (isFiredByPlayer == true) ---
        if (isFiredByPlayer && hitTag == "Enemy")
        {
            // Tenta obter o script do Inimigo 1
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            
            // Tenta obter o script do Besouro
            BettleEnemyScript bettleEnemy = other.GetComponent<BettleEnemyScript>();

            // Verifica qual inimigo foi atingido e aplica a lógica
            if (enemy1 != null)
            {
                ApplyDamageToEnemy1(enemy1);
            }
            else if (bettleEnemy != null)
            {
                ApplyDamageToBettle(bettleEnemy); // <--- Chama o método para Besouro
            }
            
            // Destrói a bala após atingir um inimigo válido
            if (enemy1 != null || bettleEnemy != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        // --- LÓGICA DE DANO: INIMIGO ATIROU (isFiredByPlayer == false) ---
        if (!isFiredByPlayer && hitTag == "Player")
        {
            // Tenta obter o script do Player de TESTE
            TESTPlayer testPlayer = other.GetComponent<TESTPlayer>();
            
            // Tenta obter o script do Player PADRÃO
            Player standardPlayer = other.GetComponent<Player>();

            if (testPlayer != null)
            {
                ApplyDamageToPlayer(testPlayer, testPlayer.currentColor);
            }
            else if (standardPlayer != null)
            {
                ApplyDamageToStandardPlayer(standardPlayer, standardPlayer.currentColor); 
            }
            
            // Destrói a bala após atingir o Player
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
            enemy.TakingDamage(PLayerDamage); 
            Debug.Log("DANO Enemy1: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Enemy1 mesma cor da bala");
        }
    }

    private void ApplyDamageToBettle(BettleEnemyScript bettle)
    {
        // NOVIDADE: Adicionando a lógica de anulação de dano do Besouro (igual ao Enemy1)
        if (bettle.currentColor != bulletColor)
        {
            // Passamos o dano e a cor da bala. O Besouro fará a subtração de HP.
            bettle.TakingDamage(PLayerDamage, bulletColor); 
            Debug.Log("DANO Besouro: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Besouro mesma cor da bala");
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

    // NOVIDADE: Método para o Player PADRÃO (Resolve o erro CS1503)
    private void ApplyDamageToStandardPlayer(Player player, int playerCurrentColor)
    {
        // Assume-se que o Player Padrão tem os métodos 'ReceiveDamage' e a variável 'currentColor'
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
}