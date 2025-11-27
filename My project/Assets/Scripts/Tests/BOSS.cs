using UnityEngine;
using System.Collections;
//System.Collections é pra coroutines

public class boss : MonoBehaviour
{
    [Header ("Stats")]
    [SerializeField] int Health = 100;
    [SerializeField] int BulletsDamage = 10;
    [SerializeField] int ShockwaveDamage = 30;

    [SerializeField] float speed = 10f;

    [SerializeField] float FireRate = 0.5f;

    [SerializeField] float ShootBreathing = 1f;

    [SerializeField] float PhasesTransitionTime = 2f;


    [Header ("Balas e Shockwaves que vai utilizar")]
    [SerializeField] GameObject GREENBigBullet;
    [SerializeField] GameObject REDBigBullet;

    [SerializeField] GameObject GREENShockwave;
    [SerializeField] GameObject REDShockwave;

    [Header("Enemies Prefabs")]
    [SerializeField] GameObject REDSmallEnemy;
    [SerializeField] GameObject GREENSmallEnemy;
    //spawn na 3a fase

    [Header("ShootPoints que ele usa para atirar")]

    [SerializeField] Transform Front;
    [SerializeField] Transform FrontandRight;
    [SerializeField] Transform FrontandLeft;
    [SerializeField] Transform Right;
    [SerializeField] Transform Left;

    [Header("Ponto para spawn de shockwaves")]
    [SerializeField] Transform ExplosionPoint;

    //referências chatas

    private Rigidbody rb;

    private Player playerRef;

    private bool IsMovementTime = true;


    void Start()
    {
        //colocar aqui as referências das coisas que ele vai usar, que nem os 8 shootpoints, etc
        //no caso, a ordem das coroutines que determinam o comportamento do boss vão aqui

        rb = GetComponent<Rigidbody>();

        playerRef = GameControllerScript.controller.Player;

        StartCoroutine("FirstPhase");

        
    }

    void Update()
    {
        //sla pqp q bgl complexo

        //vou ter que colocar no fim de cada fase uma verificação pra ver a vida do boss, se atingir ao oq quero, passar pro próximo

        PlayerChasing();
    }

    IEnumerator FirstPhase()
    {
        Coroutine ActualShootingRef;

        Debug.Log("Comecei a primeira fase!");

        while(Health >= 67)
        {
            //1. ciclo de tiros verdes

            ActualShootingRef = StartCoroutine(FirstShooting());
            IsMovementTime = true;

            yield return new WaitForSeconds(10f);

            //2. Depois de um tempo andando e atirando, paramos o tiro e paramos o movimento dele
            StopCoroutine(ActualShootingRef);
            IsMovementTime = false;

            //3. tiros de transição
            SingleShot(GREENBigBullet);
            yield return new WaitForSeconds(ShootBreathing);
            SingleShot(GREENBigBullet);
            yield return new WaitForSeconds(ShootBreathing+1f);

            //4. ciclo de tiros vermelhos
            IsMovementTime = true;
            ActualShootingRef = StartCoroutine(SecondShooting());
            yield return new WaitForSeconds(10f);

            //5. Parada depois de um tempo andando e atirando
            StopCoroutine(ActualShootingRef);
            IsMovementTime = false;

            //6. tiros VERMELHOS de transição
            SingleShot(REDBigBullet);
            yield return new WaitForSeconds(ShootBreathing);
            SingleShot(REDBigBullet);
            yield return new WaitForSeconds(ShootBreathing+1.5f);
        }
        StopCoroutine("FirstPhase");
        StartCoroutine("SecondPhase");
    }

    IEnumerator FirstShooting()
    {
        while(true)
        //só pra ficar rodando "infinitamente" até que a Coroutine mande parar
        {
            Instantiate(GREENBigBullet,Front.position, Front.rotation);
            Instantiate(GREENBigBullet,Right.position, Right.rotation);
            Instantiate(GREENBigBullet,Left.position, Left.rotation);
            Instantiate(GREENBigBullet,FrontandLeft.position, FrontandLeft.rotation);
            Instantiate(GREENBigBullet,FrontandRight.position, FrontandRight.rotation);


            yield return new WaitForSeconds(FireRate);
        }
    }

    IEnumerator SecondShooting()
    {
        while(true)
        //só pra ficar rodando "infinitamente" até que a Coroutine mande parar
        {
            //depois mudar para segundo prefab de bala, criar uma bala na cor x e outra na cor y
            Instantiate(REDBigBullet,Front.position, Front.rotation);
            Instantiate(REDBigBullet,Right.position, Right.rotation);
            Instantiate(REDBigBullet,Left.position, Left.rotation);
            Instantiate(REDBigBullet,FrontandLeft.position, FrontandLeft.rotation);
            Instantiate(REDBigBullet,FrontandRight.position, FrontandRight.rotation);

            yield return new WaitForSeconds(FireRate);
        }
    }

    void SingleShot(GameObject bulletPrefab)
    {
        Instantiate(bulletPrefab,Front.position, Front.rotation);
        Instantiate(bulletPrefab,Right.position, Right.rotation);
        Instantiate(bulletPrefab,Left.position, Left.rotation);
        Instantiate(bulletPrefab,FrontandLeft.position, FrontandLeft.rotation);
        Instantiate(bulletPrefab,FrontandRight.position, FrontandRight.rotation);
    }

    void PlayerChasing()
    {
        if (playerRef != null && IsMovementTime)
        {
            Vector3 playerPos = playerRef.transform.position;
            Vector3 targetPos = new Vector3(playerPos.x, transform.position.y, playerPos.z);

            Vector3 direction = (targetPos - transform.position).normalized;

            //aplica uma velocidade para perseguir
            rb.linearVelocity = direction * speed;

            //boss olhando para o player enquanto faz isso
            if(direction.sqrMagnitude > 0)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else if (playerRef != null && !IsMovementTime)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    IEnumerator SecondPhase()
    {
        Debug.Log("Comecei a SEGUNDA fase pq sou lendário!");
        yield return new WaitForSeconds(PhasesTransitionTime);

        while(Health > 33)
        {
        yield return new WaitForSeconds(1.5f);
        SpawnREDShockwave();
        yield return new WaitForSeconds(1.5f);
        SpawnREDShockwave();
        yield return new WaitForSeconds(1.5f);

        IsMovementTime = true;

        yield return new WaitForSeconds(5f);

        IsMovementTime = false;

        yield return new WaitForSeconds(1f);
        SpawnGREENShockwave();
        yield return new WaitForSeconds(1.5f);
        SpawnGREENShockwave();
        yield return new WaitForSeconds(2.5f);

        IsMovementTime = true;

        yield return new WaitForSeconds(5f);

        IsMovementTime = false;

        }
        StopCoroutine("SecondPhase");
        StartCoroutine("LastPhase");
    }

    void SpawnREDShockwave()
    {
        Instantiate(REDShockwave, ExplosionPoint.position, ExplosionPoint.rotation);
    }

    void SpawnGREENShockwave()
    {
        Instantiate(GREENShockwave, ExplosionPoint.position, ExplosionPoint.rotation);
    }
    IEnumerator LastPhase()
    {
        Debug.Log("Comecei a ULTIMA fase pq sou lendário!");

        Coroutine FinalShootingRef;

        while (Health <= 33)
        {
            IsMovementTime = false;

            yield return new WaitForSeconds(1f);
            SpawnGREENShockwave();
            yield return new WaitForSeconds(1.5f);

            IsMovementTime = true;
            FinalShootingRef = StartCoroutine(FirstShooting());

            yield return new WaitForSeconds(5f);

            StopCoroutine(FinalShootingRef);
            IsMovementTime = false;

            yield return new WaitForSeconds(1f);
            //spawnar Inimigos aqui
            yield return new WaitForSeconds(1.5f);

            SpawnREDShockwave();
            yield return new WaitForSeconds(1.5f);

            IsMovementTime = true;
            FinalShootingRef = StartCoroutine(SecondShooting());

            yield return new WaitForSeconds(5f);

            StopCoroutine(FinalShootingRef);
            IsMovementTime = false;

        }
        
        yield return null;
        StopCoroutine("LastPhase");
    }


}

//OQ O BOSS TEM QUE FAZER?

//o boss vai girar e atirar balas em cores de diferentes padrões

        //esses aqui serão os três padrões:

        //antes de começar, o boss vai cair no chão com a câmera tremendo ao impacto e o lvl começa

        //======PRIMEIRO PADRÃO========

        //Indo de (100% até 67% de vida)

        //boss anda lentamente até o player, enquanto vai alternando entre tiros do tipo 1 e tiros do tipo 2

        //==========SEGUNDO PADRÃO=======

        //Indo de (67% até 33% de vida)

        //o boss dá quicadas que criam shockwaves, que seria basicamente um torus que aumentaria de tamanho 

        // uma hora, torus de uma cor, outra hora, torus de outra cor

        //==========TERCEIRO PADRÃO=========

        //Indo de (33% até o fim de sua vida)

        //inimigo cria shockwaves alternadas e mais rápido, enquanto spawna inimigos explosivos nos cantos do quarto

        //Depois disso, só resta o churrascamento

        //AO TOMAR TIROS, DEIXAR MATERIAL VERMELHO E DAR INVULNERABILIDADE DE POUCOS FRAMES, COISA DE 0.5 SEG