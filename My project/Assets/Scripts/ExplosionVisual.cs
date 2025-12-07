using UnityEngine;

public class ExplosionVisual : MonoBehaviour
{
    // Duração do efeito visual em segundos
    [SerializeField] private float lifeDuration = 0.1f; 

    // Este método é chamado pelo SmallEnemy
    public void Initialize(float radius)
    {
        // NOTA: Uma esfera Mesh padrão tem raio 0.5. 
        // Para que a escala seja igual ao raio (radius), multiplicamos por 2.
        transform.localScale = Vector3.one * (radius * 2f); 
        
        // Inicia a destruição após o tempo de vida
        Destroy(gameObject, lifeDuration);
    }
}