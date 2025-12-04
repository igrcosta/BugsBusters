using UnityEngine;

public class HealingItemScript : MonoBehaviour
{
    public float quantidadeCura = 25f; 
    // tempoDeRespawn não é mais público, mas será configurado pelo Spawner
    
    // VARIÁVEIS DE SPAWN: Configuradas pelo PowerUpSpawner
    private PowerUpSpawner spawnerPai;
    private int spawnIndex; // O índice do ponto de spawn onde este item está
    private float respawnTime; // Tempo de respawn lido do Scriptable Object

    [Header("Som de Pickup")]
    public AudioClip PickupSFX;
    
    public void ConfigureSpawn(PowerUpSpawner spawner, int index, float time)
    {
        spawnerPai = spawner;
        spawnIndex = index;
        respawnTime = time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerSaude = other.GetComponent<Player>();
            if (playerSaude != null)
            {
                playerSaude.Curar((int)quantidadeCura);
                GlobalAudioPlayer.Instance.PlayPickupSound(PickupSFX);
            }

            // 1. Notifica o Spawner para iniciar a Coroutine de Respawn
            if (spawnerPai != null)
            {
                spawnerPai.RespawnItem(spawnIndex, respawnTime);
            }
            
            // 2. Destrói este Power-Up (limpando a cena)
            Destroy(gameObject);
        }
    }
    
}
