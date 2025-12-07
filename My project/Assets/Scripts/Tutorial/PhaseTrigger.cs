using UnityEngine;

// Anexe este script aos seus BoxColliders de Trigger.
// Objetos: Tutorial Phase 1 Trigger, Tutorial Phase 2 Trigger, Tutorial Phase 3 Trigger
public class PhaseTrigger : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("O número da fase que este trigger inicia (1, 2 ou 3)")]
    public int phaseNumber = 1;

    private TutorialManager manager;
    private bool hasBeenTriggered = false;

    void Start()
    {
        // Encontra o TutorialManager na cena (apenas uma vez)
        // Isso assume que o TutorialManager está na cena (FindObjectOfType)
        manager = FindObjectOfType<TutorialManager>();
        if (manager == null)
        {
            Debug.LogError("TutorialManager não encontrado na cena. O trigger não funcionará.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto é o Player (certifique-se da tag "Player") e se ainda não foi ativado.
        if (other.CompareTag("Player") && !hasBeenTriggered && manager != null)
        {
            // Notifica o manager para iniciar a fase correspondente
            manager.OnTriggerEntered(phaseNumber);
            
            // Garante que este trigger não será ativado novamente
            hasBeenTriggered = true;
        }
    }
}