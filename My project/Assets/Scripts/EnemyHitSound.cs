using UnityEngine;

public class EnemyHitSound : MonoBehaviour
{
    [SerializeField] private AudioSource hitSound;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            hitSound.Play();

            // Exemplo: chamar a lógica de dano do próprio inimigo
            // TakingDamage(10); 
        }
    }
}