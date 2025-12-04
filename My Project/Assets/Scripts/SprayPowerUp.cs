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

    private GunScript PlayerGun;

    private Player PlayerRef;

    void Start()
    {
        gameObject.SetActive(true);
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
