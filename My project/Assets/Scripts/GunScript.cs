using UnityEngine;

public class GunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    [Header("Prefabs de Bala por Cor")]
    [SerializeField] GameObject REDBulletPrefab;
    [SerializeField] GameObject GREENBulletPrefab;
    
    [Header("VARIÁVEIS SERIALIZADAS")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] Transform firePoint;

    [SerializeField] private float fireRate = 0.25f; //tempo entre tiro
    private float nextFireTime = 0f;

    private Player Player;
    private ColorHandler playerColorHandler;

    private bool isGamePaused = false;

    // Plano do chão para o Raycast
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

        //Busca o color handler do objeto pai (Player)
        if (Player != null)
        {
            playerColorHandler = Player.GetComponent<ColorHandler>();
        }

        if (playerColorHandler == null)
        {
             Debug.LogError("GunScript: FUDEU: ColorHandler não encontrado no Player. A cor do tiro não será definida.");
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
        // CHECAGEM PARA QUANDO A ARMA DEVE PARAR DE FUNCIONAR: Pausado ou DummyMode ativo
        if (isGamePaused || (Player != null && Player.DummyMode))
        {
            return;
        }
        
        if (mainCamera == null) return;

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
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) 
        {
            nextFireTime = Time.time + fireRate;
            Atirar();
        }
    }

    void Atirar()
    {
        if (firePoint == null || (REDBulletPrefab == null && GREENBulletPrefab == null)) 
        {
            Debug.LogError("Bullet Prefab ou Fire Point não atribuído no GunScript. como atirar sem bala carai?");
            return;
        }
        
        // Verifica se o ColorHandler existe antes de atirar
        if (playerColorHandler == null)
        {
            Debug.LogError("ColorHandler do Player não encontrado. SEM COR TU QUEBRA MEU JOGO");
            return;
        }

        // 1. SELEÇÃO DO PREFAB CORRETO
        GameObject prefabToInstantiate = null;
        BulletColor currentBulletColor = playerColorHandler.currentColor;

        if (currentBulletColor == BulletColor.Green)
        {
            prefabToInstantiate = GREENBulletPrefab;
        }
        else if (currentBulletColor == BulletColor.Red)
        {
            prefabToInstantiate = REDBulletPrefab;
        }
    
        if (prefabToInstantiate == null)
        {
            Debug.LogError($"Prefab para a cor {currentBulletColor} está faltando no GunScript.");
            return;
        }
        
        // Instancia a bala na posição e rotação do FirePoint.
        GameObject newBullet = Instantiate(prefabToInstantiate, firePoint.position, firePoint.rotation);
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();

        if (bulletScript == null)
        {
            Debug.LogError("BulletController não encontrado no Prefab da bala.");
            Destroy(newBullet);
            return;
        }

        bulletScript.bulletColor = currentBulletColor;
        

        bulletScript.isFiredByPlayer = true;
    }
}