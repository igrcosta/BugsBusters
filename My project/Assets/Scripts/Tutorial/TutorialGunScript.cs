using UnityEngine;

public class TutorialGunScript : MonoBehaviour
{
    private Vector3 targetPoint; // Ponto de mira horizontal (alvo)
    
    // VARIÁVEIS SERIALIZADAS
    [SerializeField] private Camera mainCamera;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    private TutorialPlayer Player;
    private int PlayerShootColor;

    //Variável para o plano do chão
    private Plane groundPlane;

    private void Update()
{
    // 1. Tenta obter a referência se ela estiver NULA (só faz isso uma vez)
    if (Player == null) 
    {
        if (TutorialController.controller != null && TutorialController.controller.PlayerTutorialRef != null)
        {
            Player = TutorialController.controller.PlayerTutorialRef;
        }
        
        // Se ainda for nulo, saia e tente novamente no próximo frame.
        if (Player == null) 
        {
            return;
        }
    }
    
    // 2. Agora que Player != null, o código continua.
    if(!Player.DummyMode)
    {
        ShootingLogic();
    }
}
    void ShootingLogic()
{
    // 1. Cria um raio da posição do mouse na tela
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        float distance;

        // 2. Faz o Raycast para o plano (chão)
    if (groundPlane.Raycast(ray, out distance))
    {
        // 3. Obtém o ponto no mundo onde o raio atingiu o plano
        Vector3 worldMousePos = ray.GetPoint(distance);
        
        // 4. Calcula o vetor de direção
        Vector3 dir = worldMousePos - transform.position;
        
        // 5. Zera o Y (opcional, mas bom para garantir a rotação horizontal pura)
        dir.y = 0;
        
        // 6. Aplica a rotação
        if (dir.sqrMagnitude > 0.01f) // Se o vetor não for muito pequeno
        {
           // 1. Calcula a rotação alvo que mira no mouse (aponta para o 'dir')
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

            Quaternion correction = Quaternion.Euler(0, 90, 0);

            transform.rotation = targetRotation * correction;
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
        // Instancia a bala na posição e rotação do FirePoint.
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        BulletController bulletScript = newBullet.GetComponent<BulletController>();
        Renderer bulletRenderer = newBullet.GetComponent<Renderer>();

        Material targetMaterial = null;

        bulletScript.isFiredByPlayer = true;

        if (Player.currentColor == 1)
        {
            targetMaterial = TutorialController.controller.MatFirst;
        }
        else
        {
            targetMaterial = TutorialController.controller.MatSecond;
        }

        if (bulletRenderer != null && targetMaterial != null)
        {
            bulletRenderer.material = targetMaterial;
        }
        
        bulletScript.bulletColor = Player.currentColor;
    }
    
    void Start()
    {
        groundPlane = new Plane(Vector3.up, Vector3.up * 0f);
    }
}