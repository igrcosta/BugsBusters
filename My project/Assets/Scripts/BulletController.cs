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

        // Se o objeto for o Player ou um Inimigo, o componente será buscado abaixo.
        // Mantenha apenas a declaração de otherBullet e hitTag aqui, e as outras buscas
        // (BettleEnemy, SmallEnemy, Enemy1) dentro do bloco de dano do Player, para melhor performance
        // e leitura, ou ajuste a declaração inicial (como você fez, mas ajustando o bloco de dano).
        
        // --- COLISÃO ENTRE BALAS ---
        if(otherBullet != null)
        {
            // Se uma bala foi atirada pelo player e a outra pelo inimigo
            if (this.isFiredByPlayer != otherBullet.isFiredByPlayer)
            {
                // Destrói a bala do Player (a que entrou em trigger)
                Destroy(this.gameObject); 
                return;
            }
            else
            {
                return; // Ignora colisão entre balas do mesmo lado
            }
        }

        // --- REGRAS DE IGNORAR ---
        // Se a bala atingiu o Player que a disparou ou vice-versa (regra de ignorar)
        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            return; 
        }

        // --- LÓGICA DE DANO: PLAYER ATIROU (isFiredByPlayer == true) ---
        if (isFiredByPlayer && hitTag == "Enemy")
        {
            // Tenta obter os scripts dos inimigos
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            BettleEnemyScript bettleEnemy = other.GetComponent<BettleEnemyScript>();
            SmallEnemy smallEnemy = other.GetComponent<SmallEnemy>(); // NOVIDADE

            // Verifica qual inimigo foi atingido e aplica a lógica
            if (enemy1 != null)
            {
                ApplyDamageToEnemy1(enemy1);
            }
            else if (bettleEnemy != null)
            {
                ApplyDamageToBettle(bettleEnemy); 
            }
            else if (smallEnemy != null) // NOVO BLOCO para o SmallEnemy
            {
                ApplyDamageToSmallEnemy(smallEnemy);
            }
            
            // Destrói a bala após atingir um inimigo válido
            if (enemy1 != null || bettleEnemy != null || smallEnemy != null)
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
        if (bettle.currentColor != bulletColor)
        {
            // Passamos o dano e a cor da bala.
            bettle.TakingDamage(PLayerDamage, bulletColor); 
            Debug.Log("DANO Besouro: Cor diferente!");
        }
        else
        {
            Debug.Log("Dano anulado, Besouro mesma cor da bala");
        }
    }
    
    // NOVIDADE: Método para o SmallEnemy
    private void ApplyDamageToSmallEnemy(SmallEnemy smallEnemy)
    {
        if (smallEnemy.currentColor != bulletColor)
        {
            // Passamos o dano e a cor da bala.
            smallEnemy.TakingDamage(PLayerDamage, bulletColor); 
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
}