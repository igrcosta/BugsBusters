using UnityEngine;

// Anexe este script ao seu Prefab de Inimigo
public class EnemyController : MonoBehaviour
{
    private TutorialManager manager;

    [Header("Configurações do Inimigo")]
    public float maxHealth = 10f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Tenta encontrar o Manager se ele não foi setado no spawn.
        if (manager == null)
        {
             manager = FindObjectOfType<TutorialManager>();
        }
    }

    // Função pública para o Manager injetar sua referência após o spawn.
    public void SetManager(TutorialManager tm)
    {
        manager = tm;
    }

    // Função que será chamada quando o inimigo levar dano (chame isso do seu script de ataque).
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. Notifica o Manager que um inimigo foi morto.
        if (manager != null)
        {
            manager.EnemyKilled();
        }

        // 2. Destrói o objeto.
        Destroy(gameObject);
    }
}