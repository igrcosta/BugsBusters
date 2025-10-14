using UnityEngine;

public class GunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    
    // VARIÁVEIS SERIALIZADAS
    [SerializeField] private Camera mainCamera;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    private Player Player;

    private void Update() 
    {
        if(Player.DummyMode == false)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // 1. A Lógica de Mira e Rotação só acontece se o Raycast acertar algo
        if (Physics.Raycast(ray, out RaycastHit raycastHit)) 
        {
            // A. Armazena o ponto de acerto no cenário
            targetPoint = raycastHit.point;

            // B. CORREÇÃO ESSENCIAL: Força o ponto alvo a ter a mesma altura (Y) do player.
            // Isso elimina qualquer rotação vertical (inclinação/capotamento).
            targetPoint.y = transform.position.y;
            
            // C. CÁLCULO E APLICAÇÃO DA ROTAÇÃO Y:
            // Calcula o vetor de direção do player para o ponto alvo (horizontal)
            Vector3 direction = targetPoint - transform.position;
            
            // Cria a rotação (Quaternion) que olha nessa direção
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Aplica a rotação no Player_ROOT (this.transform)
            transform.rotation = targetRotation;
        }

        // 2. VERIFICA O CLIQUE E ATIRA
        // Usamos GetMouseButtonDown(0) para atirar apenas no momento do clique, não segurando.
        if (Input.GetMouseButtonDown(0)) 
        {
            Atirar();
        }
        }
        else
        {
            //fazer nada
        }
    }

    void Atirar()
    {
        // Instancia a bala na posição e rotação do FirePoint.
        // O FirePoint herda a rotação Y do Player_ROOT.
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    void Start()
    {
        Player = GetComponent<Player>();
    }
}