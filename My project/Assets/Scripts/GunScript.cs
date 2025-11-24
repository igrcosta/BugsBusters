using UnityEngine;

public class GunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    
    [Header("VARIÁVEIS SERIALIZADAS")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    private Player Player;
    // Removido Enemy1 Enemy, pois não é usado
    private int PlayerShootColor;

    // 🚨 VARIÁVEL CRÍTICA: Plano do chão para o Raycast
    private Plane groundPlane; 
    
    // 🚨 VARIÁVEL CRÍTICA: Compensação de rotação para alinhar o modelo
    [Header("Correções de Rotação")]
    [Tooltip("Ajuste o Z e X para corrigir a orientação do modelo 3D.")]
    [SerializeField] private float rotationXOffset = 0f;
    [SerializeField] private float rotationZOffset = -180f; // Mantendo seu último teste

    private void Awake()
    {
        // Busca o Player no objeto pai (HeadPivot -> Player)
        Player = GetComponentInParent<Player>(); 
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (Player == null)
        {
            Debug.LogWarning("GunScript não encontrou o componente Player no objeto pai.");
        }
    }
    
    // 🚨 NOVO: Inicializa o plano do chão (altura 0.0)
    void Start()
    {
        // O Vector3.up * 0f define o plano no Y=0 do mundo.
        groundPlane = new Plane(Vector3.up, Vector3.up * 0f);
    }

    private void Update()
    {
        // Garante que o Player não seja nulo ANTES de acessar .DummyMode
        if (Player == null) 
        {
            if (GameControllerScript.controller != null && GameControllerScript.controller.Player != null)
            {
                Player = GameControllerScript.controller.Player;
            }
            if (Player == null) return; 
        }
        
        // Garante que a Câmera exista para o Raycast
        if (mainCamera == null) return;

        if(Player.DummyMode)
        {
            return;
        }
        
        ShootingLogic();
    }
    
    void ShootingLogic()
    {
        // 1. Cria um raio da posição do mouse na tela
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        float distance;

        // 2. Faz o Raycast para o plano (chão)
        // Se o raio atingir o plano...
        if (groundPlane.Raycast(ray, out distance))
        {
            // 3. Obtém o ponto no mundo onde o raio atingiu o plano
            Vector3 worldMousePos = ray.GetPoint(distance);
            
            // 4. Calcula o vetor de direção do HeadPivot até o ponto do mouse
            Vector3 dir = worldMousePos - transform.position;
            
            // 5. Zera o Y para que a rotação seja puramente horizontal
            dir.y = 0;
            
            // 6. Aplica a rotação
            if (dir.sqrMagnitude > 0.01f) // Evita NREs se o vetor for zero
            {
                // Calcula a rotação alvo que mira no mouse (aponta para o 'dir')
                Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

                // Aplica as correções para alinhar o modelo 3D.
                Quaternion correction = Quaternion.Euler(rotationXOffset, 0f, rotationZOffset);

                // A rotação final é a rotação de mira MÚLTIPLICADA pela correção.
                // Usamos targetRotation.eulerAngles.y para garantir que apenas o Y seja afetado.
                transform.localRotation = Quaternion.Euler(rotationXOffset, targetRotation.eulerAngles.y, rotationZOffset);
            }
        }
        
        // 7. VERIFICA O CLIQUE E ATIRA
        if (Input.GetMouseButtonDown(0)) 
        {
            Atirar();
        }
    }

    void Atirar()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("Bullet Prefab ou Fire Point não atribuído no GunScript.");
            return;
        }
        
        // Garante que o GameController existe para pegar materiais
        if (GameControllerScript.controller == null)
        {
            Debug.LogError("GameController não encontrado. Não é possível atirar.");
            return;
        }
        
        // Instancia a bala na posição e rotação do FirePoint.
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();

        if (bulletScript == null)
        {
            Debug.LogError("BulletController não encontrado no Prefab da bala.");
            Destroy(newBullet);
            return;
        }
        
        Material targetMaterial = (Player.currentColor == 1) 
            ? GameControllerScript.controller.PlayerMatFirst 
            : GameControllerScript.controller.PlayerMatSecond;

        bulletScript.isFiredByPlayer = true;
        
        if (bulletRenderer != null && targetMaterial != null)
        {
            bulletRenderer.material = targetMaterial;
        }
        
        bulletScript.bulletColor = Player.currentColor;
    }
}