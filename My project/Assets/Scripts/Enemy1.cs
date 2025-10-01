using UnityEngine;

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

    
    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        //encontramos o primeiro objeto na cena com a tag "Player" e guardamos na var playerTarget

        rb = GetComponent<Rigidbody>();
        //só tô pegando o próprio RigidBody do inimigo e jogando na variável rb
    }

    
    void FixedUpdate()
    {
        Chasing();
    }

    //método para realizar o comportamento de perseguição do player 
    void Chasing()
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
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}
