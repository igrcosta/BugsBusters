using UnityEngine;

public class CameraFollowPosition : MonoBehaviour
{
    // Arraste o Player_ROOT para este campo no Inspetor.
    [SerializeField] private Transform playerRoot; 
    
    // Variável para armazenar o deslocamento inicial (a distância que você definiu)
    private Vector3 offset; 
    
    void Start()
    {
        if (playerRoot != null)
        {
            // CALCULA O OFFSET:
            // Diferença entre a posição inicial da Câmera e a posição do Player.
            offset = transform.position - playerRoot.position;
        }
    }

    void LateUpdate()
    {
        if (playerRoot != null)
        {
            // APLICA O NOVO CÁLCULO:
            // A posição da Câmera é a posição do Player SOMADA ao Offset.
            transform.position = playerRoot.position + offset;
        }
    }
}