using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo e Distância Fixa")]
    [SerializeField] private Transform target; // Arraste o PlayerTUTORIAL aqui
    
    // Distância da câmera em relação ao Player
    private Vector3 offset; 

    [Header("Configurações")]
    [SerializeField] private float smoothSpeed = 5f; 
    private Quaternion fixedRotation; 

    void Awake()
    {
        // 🚨 CRÍTICO: No Awake, a câmera deve estar na posição que você quer que ela fique.
        if (target != null)
        {
            // Calcula o OFFSET baseado na POSIÇÃO INICIAL DA CÂMERA na cena.
            offset = transform.position - target.position;
        }
        
        // Fixa a rotação inicial da Câmera, impedindo-a de girar.
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcula a posição desejada: Posição do Alvo + Offset inicial
        Vector3 desiredPosition = target.position + offset;

        // 2. Interpolação (movimento suave)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Aplica a nova posição (Apenas X e Z acompanham o Player)
        transform.position = smoothedPosition;

        // 4. Força a rotação original.
        transform.rotation = fixedRotation; 
    }
}