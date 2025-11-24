using UnityEngine;

public class GunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    
    [Header("VARIÁVEIS SERIALIZADAS")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    private Player Player;
    private Enemy1 Enemy; 
    private int PlayerShootColor;

    private void Awake()
    {
        // 🚨 CORREÇÃO NRE (Busca de Referência): Busca o Player no objeto pai (HeadPivot -> Player)
        Player = GetComponentInParent<Player>(); 
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (Player == null)
        {
            Debug.LogWarning("GunScript não encontrou o componente Player no objeto pai. Tentará buscar no Update.");
        }
    }

    private void Update()
    {
        // 🚨 NRE FIX: Garante que o Player não seja nulo ANTES de acessar .DummyMode
        if (Player == null) 
        {
            // Tenta pegar a referência do Player via GameController se a busca no Awake falhou
            if (GameControllerScript.controller != null && GameControllerScript.controller.Player != null)
            {
                Player = GameControllerScript.controller.Player;
            }
            if (Player == null) return; // Retorna se ainda for nulo
        }

        if(Player.DummyMode)
        {
            return;
        }
        
        ShootingLogic();
    }
    
    void ShootingLogic()
    {
        // --- LÓGICA DE ROTAÇÃO (APENAS Y) ---
        
        // 1. Obtém a posição 2D do mouse na tela
        Vector3 mousePos = Input.mousePosition;
        
        // 2. Define a profundidade (Z)
        // Pega a distância da câmera até o objeto atual (HeadPivot)
        float distanceToTarget = (mainCamera != null) 
            ? Vector3.Distance(mainCamera.transform.position, transform.position) 
            : 10f; // Valor padrão de segurança
            
        mousePos.z = distanceToTarget;

        // 3. Transforma a posição 2D do mouse em um ponto 3D no mundo
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
        
        // 4. Calcula o vetor de direção
        Vector3 dir = worldMousePos - transform.position;
        
        // 5. Zera o Y para que o Player olhe apenas na horizontal (chão).
        dir.y = 0;

        // 6. Aplica a rotação SOMENTE no eixo Y.
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

        float rotationY = targetRotation.eulerAngles.y;
        const float MODEL_COMPENSATION_Z = 180f;

        // Aplica (0, Y, 0) ao HeadPivot. Isso preserva a correção X/Z do modelo filho.
        transform.localRotation = Quaternion.Euler(0f, rotationY, MODEL_COMPENSATION_Z);
        
        // -----------------------------------------------------------
        
        // 2. VERIFICA O CLIQUE E ATIRA
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

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();

        if (bulletScript == null || GameControllerScript.controller == null)
        {
            Debug.LogError("BulletController ou GameController não encontrado. Verifique Prefab.");
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