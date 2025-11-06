using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
//utilizei para o AttackRoutine()

// criamos uma lista de comportamentos que o inimigo terá
//chamamos isso de estados da State Machine do inimigo
public enum EnemyState {Chasing, Attacking, CoolingDown }

public class Enemy1 : MonoBehaviour
{
    private Renderer myRenderer;
    //randerizador da cor do inimigo

    [SerializeField] int Hp = 20;
    [SerializeField] float enemySpeed = 5f;
    [SerializeField] float stoppingDistance = 1.5f;
    //Esse vai ser o raio de distância para o inimigo parar e encarar o player

    private GameObject playerTarget;
    //precisamos jogar o player em uma variável, para que seja possível acessar seus dados
    // e realizar o comportamento desejado do inimigo

    private Rigidbody rb;
    //vamos usar essa variável para movimentar o inimigo por meio do seu rigidbody

    private Vector3 direction;
    //Direção que o inimigo vai seguir para achar o player

    private float distance;
    private Vector3 playerPosition;
    private Vector3 enemyPosition;
    //vetores e vars utilizados no método Chasing,
    //declarei elas aqui para poupar performance do computador

    //estamos escolhendo para o inimigo começar perseguindo o player
    private EnemyState currentState = EnemyState.Chasing;

    //variáveis para usar o prefab da bala
    [Header("Ataque")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int shotsPerBurst = 3;
    [SerializeField] float cooldownTime = 2f;

    //variáveis para comportamento das cores da bala
    private int ShooterTypeCounter = 0;
    public int currentColor;


    void Start()
    {
        Player meuPlayer = GameControllerScript.controller.Player;
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        //encontramos o primeiro objeto na cena com a tag "Player" e guardamos na var playerTarget

        rb = GetComponent<Rigidbody>();
        //só tô pegando o próprio RigidBody do inimigo e jogando na variável rb

        currentColor = GameControllerScript.controller.ColorLogic[0];

        myRenderer = GetComponent<Renderer>();
    }

    
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Chasing:
            HandleChasing(); //chamamos o método de perseguição
            break;

            case EnemyState.Attacking:
            case EnemyState.CoolingDown:
            HandleStopping(); //chama método para parar o movimento
            break;

            // esse dois pontos demonstra o que vai ser feito quando chegar naquele case,
            //no de chasing, vai ativar a função de de chasing,
            //no de attacking e de cooling down, eles vão chamar a função de parada

            //toda a sequência de ataque, cooldown e mudança de estado foi feita pelo AttackRoutine

            //FixedUpdate focado no Movimento, CoRoutines para tempo de intervalos e transições de estados
        }
    }

    //método para realizar o comportamento de perseguição do player 
    void HandleChasing()
    {   //se o player não existir, não faz nada
        if (playerTarget == null) 
        {
            return;
        }

        //obter posição do alvo (Player)
        playerPosition = playerTarget.transform.position;

        //obter posição do inimigo
        enemyPosition = transform.position;

        //calcular direção para seguir
        direction = playerPosition - enemyPosition;

        distance = direction.magnitude;

        if (distance > stoppingDistance) {
            direction.y = 0f;
            //colocamos o eixo Y do inimigo em 0 para evitar dele "voar" em direção ao seu alvo

            direction = direction.normalized;
            //depois de encontrarmos o vetor da direção, precisamos normalizar, pra ele andar em
            //velocidade constante e não ficar em velocidades absurdas em um único frame

            rb.linearVelocity = new Vector3(direction.x * enemySpeed, rb.linearVelocity.y, direction.z * enemySpeed);
            // para definir a velocidade do inimigo, vamos usar o x e o z da direção que calculamos
            // junto da velocidade linear do eixo Y do rigidbody, assim, ele vai procurar o player,
            //sem sair voando por aí, já que a única força aplicada verticalmente é a do rigidbody

            transform.LookAt(playerPosition);
        }
        else {
            //inicia o estado de ataque 
            currentState = EnemyState.Attacking;

            StartCoroutine(AttackRoutine()); //começa ciclo de Tiro e cooldown
        }
    }

    void HandleStopping()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    IEnumerator AttackRoutine()
    {
        //Alterna a cor e prepara o inimigo para o novo ciclo de ataque
        ShooterTypeCounter++; 

        // Alterna a cor do inimigo (e, consequentemente, das balas deste burst)
        currentColor = (ShooterTypeCounter % 2 == 0) ? 1 : 0;

        Material targetMaterial = (currentColor == 1) ?
        GameControllerScript.controller.PlayerMatFirst :
        GameControllerScript.controller.PlayerMatSecond;

        myRenderer.material = targetMaterial;

        //mirou no player
        transform.LookAt(playerPosition);

        //Estado 1 -> ATAQUE
        for (int i = 0; i < shotsPerBurst; i++)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate); //pausa entre tiros
        }

        //Estado 2 -> CoolDown
        currentState = EnemyState.CoolingDown;
        yield return new WaitForSeconds(cooldownTime); //pausa de cooldown

        //Estado 3 -> Chasing (FIM)
        currentState = EnemyState.Chasing;
    }

    void Shoot()
    {
        //cria uma bala
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        //pegamos o script da nova bala instaciada e jogamos na variável BulletScript, do tipo BulletController
        BulletController bulletScript = newBullet.GetComponent<BulletController>();

        Renderer BulletRenderer = newBullet.GetComponent<Renderer>();

        //variável de material vazia para podermos colocar o material do inimigo nas balas
        Material targetMaterial = null;

        if (bulletScript != null)
        {
            bulletScript.isFiredByPlayer = false;

            if (currentColor == 1)
            {
                targetMaterial = GameControllerScript.controller.PlayerMatFirst;
            }
            else
            {
                targetMaterial = GameControllerScript.controller.PlayerMatSecond;
            }

            if (BulletRenderer != null)
            {
                BulletRenderer.material = targetMaterial;
            }
            
            bulletScript.bulletColor = currentColor;
        }
    }

    public void TakingDamage(int bulletDamage)
    {
        //se a cor do inimigo é diferente da cor da bala
        Hp -= bulletDamage;
        Debug.Log("Inimigo recebeu " + bulletDamage + "de dano. Vida restante:  " + Hp);

        if (Hp <= 0)
        {
            GameControllerScript.controller.AumentarNumerodeInimigosMortos();
            //animação de morte e depois...
            Destroy(gameObject);
        }
    }
}
