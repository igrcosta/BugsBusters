using UnityEngine;

public class HealingItemScript : MonoBehaviour
{
    public float quantidadeCura = 25f; 
    public float tempoDeRespawn = 10f;

    private SpawnerItem spawnerPai;
    
    public void SpawnerConfigure(SpawnerItem spawner)
    {
        spawnerPai = spawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerSaude = other.GetComponent<Player>();
            if (playerSaude != null)
            {
                playerSaude.Curar((int)quantidadeCura);
            }

            gameObject.SetActive(false);
            spawnerPai.RespawnItem(tempoDeRespawn);
        }
    }
    
}
