using UnityEngine;
using System.Collections;
//System.Collections é pra coroutines

public class boss : MonoBehaviour
{
    [Header ("Stats")]
    public bool IsVulnerable = true;
    [SerializeField] public int MaxHealth = 1000;
    [SerializeField] public int Health = 100;
    [SerializeField] int BulletsDamage = 10;
    [SerializeField] int ShockwaveDamage = 30;

    [SerializeField] float speed = 10f;

    [SerializeField] float FireRate = 0.5f;

    [SerializeField] float ShootBreathing = 1f;

    [SerializeField] float PhasesTransitionTime = 1.5f;

    private Animator animator; //pra animação funcionar


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

    [Header("Pontos para spawn dos inimigos")]
    [SerializeField] Transform RightPoint;
    [SerializeField] Transform LeftPoint;

    [Header("Luzes do Boss")]
    [SerializeField] private Light bossSpotlight; //componente de luz vai aqui
    [SerializeField] private Color greenLightColor = Color.green; //cor verde de luz
    [SerializeField] private Color redLightColor = Color.red; // cor vermelha de luz

    [Header("Som de Dano")]
    [SerializeField] private AudioSource hitAudioSource;

    private ColorHandler bossColorHandler;

    //referências e var chatas

    private Rigidbody rb;

    private Player playerRef;

    private bool IsMovementTime = true;

    private Coroutine ActualShootingRef;

    void Start()
    {
        //colocar aqui as referências das coisas que ele vai usar, que nem os 8 shootpoints, etc
        //no caso, a ordem das coroutines que determinam o comportamento do boss vão aqui

        Health = MaxHealth;

        GameControllerScript.controller.BossRef = this;

        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();  //pra animação funcionar

        playerRef = GameControllerScript.controller.Player;

        bossColorHandler = GetComponent<ColorHandler>();

        StartCoroutine("FirstPhase");

        
    }

    void Update()
    {
        //sla pqp q bgl complexo

        //vou ter que colocar no fim de cada fase uma verificação pra ver a vida do boss, se atingir ao oq quero, passar pro próximo

        PlayerChasing();

        GameControllerScript.controller.GameUI.BossBarCondition();
    }
    
    // ====================== ATENÇÃO =======================
    //             fases do boss === INÍCIO
    //=======================================================

    #region Primeira Fase
    IEnumerator FirstPhase()
    {
        Debug.Log("Comecei a primeira fase!");

        while(Health >= 670)
        {
            //1. ciclo de tiros vermelhos, definindo a cor dele pra vermelho

            IsVulnerable = false;
            MovingAndShootingRed();

            yield return new WaitForSeconds(5f);

            //2. Parada depois de um tempo andando e atirando

            StoppingtoShoot();

            //3. tiros VERMELHOS de transição (VULNERÁVEL)

            InvokeRepeating("RedTransitionShooting", 0.1f, 1f);
            yield return new WaitForSeconds(2.4f);
            CancelInvoke("RedTransitionShooting");
            yield return new WaitForSeconds(0.5f);

            //4. ciclo de tiros verdes definindo antes a cor do boss pra verde
            
            IsVulnerable = false;
            MovingAndShootingGreen();

            yield return new WaitForSeconds(5f);

            //5. Depois de um tempo andando e atirando, paramos o tiro e paramos o movimento dele

            StoppingtoShoot();
            
            //6. tiros VERDES de transição (VULNERÁVEL)

            InvokeRepeating("GreenTransitionShooting", 0.1f, 1f);
            yield return new WaitForSeconds(2.4f);
            CancelInvoke("GreenTransitionShooting");
            yield return new WaitForSeconds(0.5f);
        }
        StopCoroutine("FirstPhase");
        IsVulnerable = false;
        StartCoroutine("SecondPhase");
    }
    #endregion

    #region Segunda Fase
    IEnumerator SecondPhase()
    {
        Debug.Log("Comecei a SEGUNDA fase pq sou lendário!");
        yield return new WaitForSeconds(PhasesTransitionTime);

        while(Health > 330)
        {
            //vermelho, verde,

            //anda um cadim (VULNERÁVEL AQUI)

            //vermelho, verde, verde

            //anda um cadim de outra cor (VULNERÁVEL AQUI)

            IsVulnerable = false;
            IsMovementTime = false;

            StartCoroutine("SummonREDShockwave");
            //essa coroutina já se para sozinha, rlx

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonGREENShockwave");
            //essa coroutina já se para sozinha, rlx

            yield return new WaitForSeconds(1.5f);

            IsVulnerable = true;
            MovingAndShootingGreen();
            yield return new WaitForSeconds(5f);

            StoppingtoShockwave();

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonREDShockwave");

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonGREENShockwave");

            yield return new WaitForSeconds(0.6f);

            StartCoroutine("SummonGREENShockwave");

            yield return new WaitForSeconds(1.8f);

            IsVulnerable = true;
            MovingAndShootingRed();
            yield return new WaitForSeconds(5f);

            StoppingtoShockwave();
        }
        StopCoroutine("SecondPhase");
        IsVulnerable = false;
        StartCoroutine("LastPhase");
    }
    #endregion

    #region ÚLTIMA Fase
    IEnumerator LastPhase()
    {
        Debug.Log("Comecei a ULTIMA fase pq sou lendário!");
        yield return new WaitForSeconds(PhasesTransitionTime);


        while (Health <= 330)
        {

            // vermelho, vermelho, verde

            //para para spawnar inimigos (VULNERÁVEL AQUI)

            //vermelho, verde, vermelho, verde (de forma rápida)

            StoppingtoShockwave();

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonREDShockwave");

            yield return new WaitForSeconds(1.2f);

            StartCoroutine("SummonREDShockwave");

            yield return new WaitForSeconds(1.1f);

            StartCoroutine("SummonGREENShockwave");

            yield return new WaitForSeconds(1.4f);
            
            IsVulnerable = true;
            MovingAndShootingGreen();
            SpawnEnemies();

            yield return new WaitForSeconds(7f);

            StoppingtoShockwave();

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonREDShockwave");

            yield return new WaitForSeconds(1.1f);

            StartCoroutine("SummonGREENShockwave");

            yield return new WaitForSeconds(1.1f);

            StartCoroutine("SummonREDShockwave");

            yield return new WaitForSeconds(1f);

            StartCoroutine("SummonGREENShockwave");

            yield return new WaitForSeconds(1f);

            IsVulnerable = true;
            MovingAndShootingRed();
            SpawnEnemies();

            yield return new WaitForSeconds(10f);
        }
        
        yield return null;
        StopCoroutine("LastPhase");
    }

    #endregion

    // ====================== ATENÇÃO =======================
    //                fases do boss === FIM
    //=======================================================

    //FUNÇÕES PARA DIMINUIR O CÓDIGO DAS FASES: INÍCIO

    #region Funções das fases

    void MovingAndShootingRed()
    {
        SetPhaseColor(BulletColor.Red);
        ActualShootingRef = StartCoroutine(SecondShooting());
        IsMovementTime = true;
    }
    void MovingAndShootingGreen()
    {
        SetPhaseColor(BulletColor.Green);
        IsMovementTime = true;
        ActualShootingRef = StartCoroutine(FirstShooting());
    }

    //a função de parar para atirar mantêm a última cor aplicada, cuidado com isso

    void StoppingtoShoot()
    {
        StopCoroutine(ActualShootingRef);
        IsMovementTime = false;
        IsVulnerable = true;
    }

    void SpawnREDShockwave()
    {
        Instantiate(REDShockwave, ExplosionPoint.position, ExplosionPoint.rotation);
    }

    void SpawnGREENShockwave()
    {
        Instantiate(GREENShockwave, ExplosionPoint.position, ExplosionPoint.rotation);
    }

    IEnumerator SummonREDShockwave()
    {
        SetPhaseColor(BulletColor.Red);
        yield return new WaitForSeconds(0.8f); 
        SpawnREDShockwave();
        yield return new WaitForSeconds(1.5f); 
        yield break;
    }
    
    IEnumerator SummonGREENShockwave()
    {
        SetPhaseColor(BulletColor.Green);
        yield return new WaitForSeconds(0.8f); 
        SpawnGREENShockwave();
        yield return new WaitForSeconds(1.5f); 
        yield break;
    }

    void StoppingtoShockwave()
    {
        IsVulnerable = false;
        StopCoroutine(ActualShootingRef);
        IsMovementTime = false;
    }

    void RedTransitionShooting()
    {
        GameObject bulletToShoot = REDBigBullet;

        Instantiate(bulletToShoot,Front.position, Front.rotation);
        Instantiate(bulletToShoot,Right.position, Right.rotation);
        Instantiate(bulletToShoot,Left.position, Left.rotation);
        Instantiate(bulletToShoot,FrontandLeft.position, FrontandLeft.rotation);
        Instantiate(bulletToShoot,FrontandRight.position, FrontandRight.rotation);
    }


    void GreenTransitionShooting()
    {
        GameObject bulletToShoot = GREENBigBullet;

        Instantiate(bulletToShoot,Front.position, Front.rotation);
        Instantiate(bulletToShoot,Right.position, Right.rotation);
        Instantiate(bulletToShoot,Left.position, Left.rotation);
        Instantiate(bulletToShoot,FrontandLeft.position, FrontandLeft.rotation);
        Instantiate(bulletToShoot,FrontandRight.position, FrontandRight.rotation);
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

    void PlayerChasing()
    {
        if (playerRef == null)
        {
            // Se não há player, não faça nada.
            return;
        }

        // 1. Cálculo da direção para o Player
        Vector3 playerPos = playerRef.transform.position;
        Vector3 targetPos = new Vector3(playerPos.x, transform.position.y, playerPos.z);
        Vector3 direction = (targetPos - transform.position).normalized;

        // 2. Rotação (Olhar para o Player)
        // O Boss deve olhar para o Player SEMPRE que houver um Player.
        if (direction.sqrMagnitude > 0)
        {
            // Usa Quaternion.LookRotation para fazer o Boss girar na direção do Player
            transform.rotation = Quaternion.LookRotation(direction);
        }
    
        // 3. Movimento (se IsMovementTime for true)
        if (IsMovementTime)
       {
            // Aplica uma velocidade para perseguir
            rb.linearVelocity = direction * speed;

            // Define a velocidade para animação baseado no movimento real
            float currentMoveSpeed = direction.sqrMagnitude > 0.01f ? speed : 0f;
            animator.SetFloat("enemySpeed", currentMoveSpeed);

        }
        else // Se não está em tempo de movimento, para.
        {
            rb.linearVelocity = Vector3.zero;
            animator.SetFloat("enemySpeed", 0);
        }
    }  

    void SpawnEnemies()
    {
        Instantiate(GREENSmallEnemy, RightPoint.position, RightPoint.rotation);
        Instantiate(REDSmallEnemy, LeftPoint.position, LeftPoint.rotation);
    }
    #endregion

    //FUNÇÕES PARA DIMINUIR O CÓDIGO DAS FASES: FIM

    //LÓGICA DE COR E DANO APLICADA DAQUI PRA BAIXO

    public void TakingDamage(int bulletDamage)
    {
        if (IsVulnerable)
        {
            //PARTE VISUAL === INÍCIO

            //tempo que vai durar a piscada
            const float flashDuration = 0.1f;

            TurnSpotlightOff();
            //desliga a spotlight do boss

            CancelInvoke("TurnSpotlightOn");
            //cancela outros invokes de ligar aluz pra n dar conflito

            Invoke("TurnSpotlightOn", flashDuration);

            //PARTE VISUAL === FIM

            GameControllerScript.controller.GameUI.UpdateBossBar();

            Health -= bulletDamage;

            hitAudioSource.Play();

            if (Health <= 0)
            {
                GameControllerScript.controller.TheGameIsOver();
            }
        }
    }

    void SetPhaseColor(BulletColor newColor)
    {
        if (bossColorHandler != null)
        {
            bossColorHandler.SetColorAndMaterial(newColor, this);
            //chama o método do colorhandler, passando a referência do boss
        }
    }

    public void OnBossColorChange(BulletColor newColor)
    {
        //boss vai receber uma cor nova pelo parâmetro

        if (bossSpotlight == null) return;

        Color targetLightColor = (newColor == BulletColor.Green) ? greenLightColor : redLightColor;
        bossSpotlight.color = targetLightColor;
    }

    //métodos para apagar e acender a luz da frente do boss, indicando dano

    private void TurnSpotlightOff()
    {
        if (bossSpotlight != null)
        {
            bossSpotlight.enabled = false;
        }
    }

    private void TurnSpotlightOn()
    {
        if (bossSpotlight != null)
        {
            bossSpotlight.enabled = true;
        }
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