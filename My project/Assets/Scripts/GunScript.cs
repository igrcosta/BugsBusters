using UnityEngine;

public class GunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    
    // VARIÁVEIS SERIALIZADAS
    [SerializeField] private Camera mainCamera;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    private Player Player;
    private Enemy1 Enemy; // Não utilizado na lógica de tiro/mira, mas mantido
    private int PlayerShootColor;

    private void Update()
{
    if(!Player.DummyMode)
    {
        ShootingLogic();
    }
}
    void ShootingLogic()
    {
        // ... (Bloco de segurança e Player.DummyMode == false)

    // --- NOVA LÓGICA DE ROTAÇÃO (CORRIGIDA) ---
    
    // 1. Obtém a posição 2D do mouse na tela
    Vector3 mousePos = Input.mousePosition;
    
    // 2. CRÍTICO: Define a profundidade (Z) para a distância da câmera até o jogador/chão.
    // Usamos a distância Z entre a câmera e o jogador, o que geralmente é:
    mousePos.z = mainCamera.transform.position.y - transform.position.y;
    
    // **ALTERNATIVA MAIS SEGURA (Recomendada):**
    // Se sua câmera não estiver na vertical pura, é melhor usar a distância Z da câmera.
    // float cameraDistanceToPlayer = Vector3.Distance(mainCamera.transform.position, transform.position);
    // mousePos.z = cameraDistanceToPlayer;
    
    // O valor a seguir costuma funcionar se a câmera está estática:
    // mousePos.z = 10f; // Se 10 for a distância Z da sua câmera ao player

    // **Vamos tentar uma correção simples baseada na sua estrutura:**
    // A distância da câmera até o plano onde o player está.
    mousePos.z = mainCamera.transform.position.y; 

    // 3. Transforma a posição 2D do mouse em um ponto 3D no mundo
    Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
    
    // 4. Calcula o vetor de direção
    Vector3 dir = worldMousePos - transform.position;
    
    // 5. Zera o Y para que o Player olhe apenas na horizontal.
    dir.y = 0;
    
    // 6. Aplica a rotação
    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    
    // -----------------------------------------------------------
    
    // 2. VERIFICA O CLIQUE E ATIRA
    if (Input.GetMouseButtonDown(0)) 
    {
        Atirar();
    }
    }

    void Atirar()
    {
        // Instancia a bala na posição e rotação do FirePoint.
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();

        Material targetMaterial = null;

        bulletScript.isFiredByPlayer = true;

        if (Player.currentColor == 1)
        {
            targetMaterial = GameControllerScript.controller.PlayerMatFirst;
        }
        else
        {
            targetMaterial = GameControllerScript.controller.PlayerMatSecond;
        }

        if (bulletRenderer != null && targetMaterial != null)
        {
            bulletRenderer.material = targetMaterial;
        }
        
        bulletScript.bulletColor = Player.currentColor;
    }

    void Start()
    {
        // Assumindo que este script está anexado ao objeto Player
        Player = GetComponent<Player>(); 
        // Nota: O Enemy = GetComponent<Enemy1>() aqui parece incorreto, 
        // pois você está no GunScript do Player. Mantive para evitar quebrar seu código.
        Enemy = GetComponent<Enemy1>(); 
    }
}