using UnityEngine;
using System.Collections;
//utilizei para o AttackRoutine()

// criamos uma lista de comportamentos que o inimigo terá
//chamamos isso de estados da State Machine do inimigo
public enum EnemyState {Chasing, Attacking, CoolingDown }

public class Enemy1 : MonoBehaviour
{
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
    private AffinityColorENUM currentColor = AffinityColorENUM.X;


    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        //encontramos o primeiro objeto na cena com a tag "Player" e guardamos na var playerTarget

        rb = GetComponent<Rigidbody>();
        //só tô pegando o próprio RigidBody do inimigo e jogando na variável rb
    }

    
    void FixedUpdate()
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
    {
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

        if (bulletScript != null)
        {
            bulletScript.isFiredByPlayer = false;

            ShooterTypeCounter++;

            if (ShooterTypeCounter % 2 == 0)
            {
            //se o ShooterTypeCounter estiver em um número par

            currentColor = AffinityColorENUM.X;
            //a cor do tiro que o inimigo soltar, vai estar definida como X, e a sua própria cor TAMBÉM

            bulletScript.bulletColor = currentColor;
            //o BulletController recebe essa informação e guarda com ele
            }
            else {
                //se o ShooterTypeCounter estiver em um número ímpar

                currentColor = AffinityColorENUM.Y;
                //a cor do tiro que o inimigo soltar, vai estar definida como X, e a sua própria cor TAMBÉM

                bulletScript.bulletColor = currentColor;
                //o BulletController recebe essa informação e guarda com ele
            }
            //lógica da cor da bala aqui
        }
    }
}
