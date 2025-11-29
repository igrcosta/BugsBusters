using UnityEngine;

public class GlobalAudioPlayer : MonoBehaviour
{
    public static GlobalAudioPlayer Instance;
    
    private AudioSource audioSource; 

    void Awake()
    {
        // Padrão Singleton: Garante que só há uma instância e a torna acessível globalmente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Pega o AudioSource anexado a este objeto
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("GlobalAudioPlayer precisa de um componente AudioSource anexado!");
        }
    }

    /// <summary>
    /// Método usado por todos os scripts (como o HealingItem) para tocar um som curto.
    /// </summary>
    public void PlayPickupSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Usa PlayOneShot() para disparar o som sem interromper outros sons.
            // O som sairá através do Mixer Group configurado neste AudioSource.
            audioSource.PlayOneShot(clip);
        }
    }
}