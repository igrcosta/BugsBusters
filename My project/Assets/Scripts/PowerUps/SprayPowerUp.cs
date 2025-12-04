using UnityEngine;

public class SprayPowerUp : MonoBehaviour
{
    [Header("Bullets e afins")]
    [SerializeField] GameObject REDBIGBullet;
    [SerializeField] GameObject GREENBIGBullet;
    [SerializeField] GameObject redBullet;
    [SerializeField] GameObject greenBullet;

    [Header("Som de Pickup")]
    public AudioClip PickupSFX;

    // VARIÁVEIS DE SPAWN: Configuradas pelo PowerUpSpawner
    private PowerUpSpawner spawnerPai;
    private int spawnIndex; 
    private float respawnTime;

    private GunScript PlayerGun;

    private Player PlayerRef;

    // NOVO: Método para configurar as variáveis de spawn
    public void ConfigureSpawn(PowerUpSpawner spawner, int index, float time)
    {
        spawnerPai = spawner;
        spawnIndex = index;
        respawnTime = time;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerRef = other.GetComponent<Player>();
            //pegamos uma referencia do player

            PlayerGun = PlayerRef.GetComponentInChildren<GunScript>();
            // seu gunscript

            PlayerGun.StartCoroutine("PowerUpEffect");

            GlobalAudioPlayer.Instance.PlayPickupSound(PickupSFX);

            gameObject.SetActive(false);
        }
        
    }
}
