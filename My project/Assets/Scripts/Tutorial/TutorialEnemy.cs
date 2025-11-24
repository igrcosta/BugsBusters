using UnityEngine;

// Este script DEVE ser anexado ao seu Prefab de Inimigo.
// Substitui o código antigo para usar 'EnemyKilled()'.
public class TutorialEnemy : MonoBehaviour
{
    // O Manager é referenciado aqui.
    private TutorialManager manager;

    [Header("Configurações do Inimigo")]
    public float maxHealth = 10f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Se o Manager não foi setado no spawn, tenta encontrar um na cena.
        if (manager == null)
        {
             manager = FindObjectOfType<TutorialManager>();
        }

        if (manager == null)
        {
            Debug.LogError("TutorialManager não encontrado. O inimigo não poderá reportar sua morte.");
        }
    }

    // Função que o TutorialManager usa para injetar sua própria referência no spawn.
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
            // ADICIONE ISSO:
            Debug.Log("Inimigo morreu! Notificando Manager."); 
        }

        // 2. Destrói o objeto.
        Destroy(gameObject);
    }
}