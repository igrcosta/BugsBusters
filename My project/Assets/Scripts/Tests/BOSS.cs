using UnityEngine;
using System.Collections;
//System.Collections é pra coroutines

public class boss : MonoBehaviour
{
    [Header ("Stats")]
    [SerializeField] int Health = 100;
    [SerializeField] int BulletsDamage = 10;
    [SerializeField] int ShockwaveDamage = 30;

    [SerializeField] float FireRate = 0.5f;


    [Header ("Dados para Spins")]
    //angulações e coisas para o primeiro Spin
    private Quaternion InitialRotation;
    [SerializeField] float TotalAngle = 720f;
    [SerializeField] float RotationTime = 10f;

    private float AngleSpeedPerSecond; //x graus por segundo para o primeiro spin

    [SerializeField] float SecondTotalAngle = -720f;

    private float SecondAngleSpeedPerSecond; //x graus por segundo


    [Header ("Balas, inimigos que ele vai spawnar, etc")]
    [SerializeField] GameObject BigBullet;

    //provavelmente vou ter q colocar a merda de um bigbullet de outra cor
    [Header("Enemies Prefabs")]

    [SerializeField] GameObject REDBettleEnemy;
    [SerializeField] GameObject GREENBettleEnemy;
    //spawn na 2a fase

    [SerializeField] GameObject REDSmallEnemy;
    [SerializeField] GameObject GREENSmallEnemy;
    //spawn na 3a fase

    [Header ("Pontos que ele usa para atirar")]

    [SerializeField] Transform Front;
    [SerializeField] Transform FrontandRight;
    [SerializeField] Transform FrontandLeft;
    [SerializeField] Transform Right;
    [SerializeField] Transform Left;
    [SerializeField] Transform Back;
    [SerializeField] Transform BackandRight;
    [SerializeField] Transform BackandLeft;

    //referências chatas

    private Rigidbody rb;


    void Start()
    {
        //colocar aqui as referências das coisas que ele vai usar, que nem os 8 shootpoints, etc
        //no caso, a ordem das coroutines que determinam o comportamento do boss vão aqui

        rb = GetComponent<Rigidbody>();

        //coisas que vai usar para o primeiro e segundo spin:
        AngleSpeedPerSecond = TotalAngle / RotationTime;

        SecondAngleSpeedPerSecond = SecondTotalAngle / RotationTime;

        StartCoroutine("PhaseSwitcherCoroutine");

        
    }
    
    IEnumerator PhaseSwitcherCoroutine()
    {
        if (Health > 67)
        {
            //executar primeira fase
            StartCoroutine("FirstPhase");
        }
        else
        if (Health < 67)
        {
            //executar segunda fase
            StartCoroutine("SecondPhase");
        }
        else if (Health <= 33)
        {
            //executar última fase
            StartCoroutine("LastPhase");
        }
        yield break;

    }

    void Update()
    {
        //sla pqp q bgl complexo

        //vou ter que colocar no fim de cada fase uma verificação pra ver a vida do boss, se atingir ao oq quero, passar pro próximo
    }

    IEnumerator FirstPhase()
    {
        Coroutine ActualShootingRef;

        Debug.Log("Comecei a primeira fase!");

        while(Health >= 67)
        {
            ActualShootingRef = StartCoroutine(FirstSpinShooting());
            yield return StartCoroutine(FirstSpin());
            //Com esse yield return estamos dizendo que nada acontece antes do giro terminar

            StopCoroutine(ActualShootingRef);
            //paramos o tiro em paralelo

            yield return new WaitForSeconds (2f);
            //espera 2 segundos depois de cada girada

            ActualShootingRef = StartCoroutine(SecondSpinShooting());
            yield return StartCoroutine(SecondSpin());
            //enquanto osegundo giro não terminar, o código não continua

            StopCoroutine(ActualShootingRef);
            //para a rotina de tiros

            yield return new WaitForSeconds (2f);
            //espera 2 segundos depois de cada girada
        }
        StartCoroutine("SecondPhase");
    }

    IEnumerator FirstSpin()
    {
        float timeSpinning = 0f;
        Quaternion InitialRotation = rb.rotation;

        while(timeSpinning < RotationTime)
        {
            timeSpinning += Time.deltaTime;
            //para não ficar em looping infinito, o tempo vai aumentando gradativamente

            float currentAngle = timeSpinning * AngleSpeedPerSecond;
            //o angulo de rotação para onde o objeto vai girar, será definido pelo tempo que já está girando * quantos graus ele gira por segundo
            //assim, à cada frame, ele vai andando de pouco em pouco até onde a gente quer

            Quaternion targetRotation = InitialRotation * Quaternion.Euler(0, currentAngle, 0);
            //esse targetRotation simboliza isso, ele tá calculando quanto tem que se andar do ponto 0 até o angulo atual

            rb.MoveRotation(targetRotation);
            //agora, basta aplicar ao rigidbody essas pequenas e constantes mudanças na rotação

            yield return null;
            //espera o próximo frame para continuar até o loop acabar
        }
    }


    IEnumerator FirstSpinShooting()
    {
        while(true)
        //só pra ficar rodando "infinitamente" até que a Coroutine mande parar
        {
            Instantiate(BigBullet,Front.position, Front.rotation);
            Instantiate(BigBullet,Back.position, Back.rotation);
            Instantiate(BigBullet,Right.position, Right.rotation);
            Instantiate(BigBullet,Left.position, Left.rotation);
            Instantiate(BigBullet,FrontandLeft.position, FrontandLeft.rotation);
            Instantiate(BigBullet,FrontandRight.position, FrontandRight.rotation);
            Instantiate(BigBullet,BackandLeft.position, BackandLeft.rotation);
            Instantiate(BigBullet,BackandRight.position, BackandRight.rotation);

            yield return new WaitForSeconds(FireRate);
        }
    }

    IEnumerator SecondSpin()
    {
        Debug.Log("Vou dar a segunda girada pq sou sigma!");

        float timeSpinning = 0f;
        Quaternion InitialRotation = rb.rotation;

        while(timeSpinning < RotationTime)
        {
            timeSpinning += Time.deltaTime;
            //para não ficar em looping infinito, o tempo vai aumentando gradativamente

            float currentAngle = timeSpinning * SecondAngleSpeedPerSecond;
            //o angulo de rotação para onde o objeto vai girar, será definido pelo tempo que já está girando * quantos graus ele gira por segundo
            //assim, à cada frame, ele vai andando de pouco em pouco até onde a gente quer

            Quaternion targetRotation = InitialRotation * Quaternion.Euler(0, currentAngle, 0);
            //esse targetRotation simboliza isso, ele tá calculando quanto tem que se andar do ponto 0 até o angulo atual

            rb.MoveRotation(targetRotation);
            //agora, basta aplicar ao rigidbody essas pequenas e constantes mudanças na rotação

            yield return null;
            //espera o próximo frame para continuar até o loop acabar
        }
    }

    IEnumerator SecondSpinShooting()
    {
        while(true)
        //só pra ficar rodando "infinitamente" até que a Coroutine mande parar
        {
            //depois mudar para segundo prefab de bala, criar uma bala na cor x e outra na cor y
            Instantiate(BigBullet,Front.position, Front.rotation);
            Instantiate(BigBullet,Back.position, Back.rotation);
            Instantiate(BigBullet,Right.position, Right.rotation);
            Instantiate(BigBullet,Left.position, Left.rotation);
            Instantiate(BigBullet,FrontandLeft.position, FrontandLeft.rotation);
            Instantiate(BigBullet,FrontandRight.position, FrontandRight.rotation);
            Instantiate(BigBullet,BackandLeft.position, BackandLeft.rotation);
            Instantiate(BigBullet,BackandRight.position, BackandRight.rotation);

            yield return new WaitForSeconds(FireRate);
        }
    }

    IEnumerator SecondPhase()
    {
        Debug.Log("Comecei a SEGUNDA fase pq sou lendário!");
        yield break;
    }

    IEnumerator LastPhase()
    {
        Debug.Log("Comecei a ULTIMA fase pq sou lendário!");
        yield return null;
        StopCoroutine("LastPhase");
    }


}

//OQ O BOSS TEM QUE FAZER?

//o boss vai girar e atirar balas em cores de diferentes padrões

        //esses aqui serão os três padrões:

        //======PRIMEIRO PADRÃO========

        //Indo de (100% até 67% de vida)

        //balas grandes sendo atiradas por 10 segundos (colocar esse tempo numa variável), enquanto o boss gira em sentido horário, colocar um tempo de tiro que dê a impressão de onda, de curva e forçe o player a sair correndo
        //depois desse tempo, ele troca o sentido que gira e a cor do tiro

        //==========SEGUNDO PADRÃO=======

        //Indo de (67% até 33% de vida)

        //o boss dá quicadas que criam shockwaves, que seria basicamente um torus que aumentaria de tamanho 

        //alternando entre dois tipos de quicada (LÁ ELE KKKK)

        //quicada lenta (de 15 em 15 segundos, ele dá uma sentada que manda uma shockwave em velocidade baixa)
        //spawna inimigos besouros no momento que ele dá a sentada (LÁ ELE 2x KKKKK)

        //quicada mais rápida (de 7 em 7 segundos, ele dá a a sentada que manda uma shockwave em velocidade rápida)
        //essa não vai spawnar inimigos, mas tô pensando em talvez fazer algo mais ritmado tipo 1, 2, 3!
        //audio no bloco de notas explica esse ritmo, mas a ideia seria fazer com que o player esteja preocupado em trocar de cor à tempo, essas ondas alternariam de cor rapidamente

        //==========TERCEIRO PADRÃO=========

        //Indo de (33% até o fim de sua vida)

        //cópia do primeiro padrão + spawn de inimigos explosivos de tempos em tempos

        //Depois disso, só resta o churrascamento
