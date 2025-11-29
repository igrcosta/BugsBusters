using UnityEngine;

public class HealingItemScript : MonoBehaviour
{
    public float quantidadeCura = 25f; 
    public float tempoDeRespawn = 10f;

    private SpawnerItem spawnerPai;

    [Header("Som de Pickup")]
    public AudioClip PickupSFX;
    
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
                GlobalAudioPlayer.Instance.PlayPickupSound(PickupSFX);
                //já que o objeto vai pro vasco, isso resolve o problema de não se ouvir nada
            }

            gameObject.SetActive(false);
            spawnerPai.RespawnItem(tempoDeRespawn);
        }
    }
    
}
