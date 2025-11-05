using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TESTPlayer : MonoBehaviour
{
    [Header("Cor")]
    public int currentColor;
    private Renderer myRenderer;

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    [SerializeField] float speed;
    private Slider HealthBarUI;

    [Header("Gravidade")]
    [SerializeField] float gravity = -9.81f;

    private CharacterController cc;
    // criamos uma variável do tipo CharacterController chamada cc

    private Vector3 verticalVelocity;

    public bool DummyMode = true;

    public void Initializing()
{
    // 1. Conexão e Componentes Essenciais (CC)
    cc = GetComponent<CharacterController>();
    currentHealth = maxHealth;
    Debug.Log("Player Health: " + currentHealth);

    if (cc == null)
    {
        Debug.LogError("CharacterController (CC) não encontrado! Player não vai funcionar.");
        enabled = false;
        return; // Sai se o CC não existe
    }
    
    // 2. Tentar se conectar ao Controller de Teste
    if (TESTGameController.controller != null)
    {
        TESTGameController.controller.Player = this; 
        currentColor = TESTGameController.controller.ColorLogic[0]; 
        Debug.Log("Player conectado ao TESTGameController.");
    }
    else
    {
        Debug.LogWarning("TESTGameController não encontrado. A lógica de cores pode falhar.");
    }

    // 3. Obter o Renderer (Essencial para mudar a cor)
    // Se o objeto Player tem o Renderer nele (melhor para testes rápidos)
    myRenderer = GetComponent<Renderer>(); 
    
    if (myRenderer == null)
    {
        // Tenta pegar no filho, se houver um
        Transform playerModelTransform = transform.Find("PlayerModel");
        if (playerModelTransform != null)
        {
            myRenderer = playerModelTransform.GetComponent<Renderer>();
        }
        else
        {
            Debug.LogWarning("Renderer não encontrado. Troca de cor visual desabilitada.");
        }
    }
}

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        Initializing();
    }

    void Update()
    {
        ApplyGravity();
        Movement();
        ColorLogic();
        HandleColorSwitchInput();
    }

    void ApplyGravity()
    {
        if (cc.isGrounded)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        cc.Move(verticalVelocity * Time.deltaTime);
    }
    
    public void Movement()
    {
        if(!DummyMode)
        {
            float VertMove = Input.GetAxis("Vertical");
            //quando apertar botões como W ou S, gerar um valor float

            float HorizMove = Input.GetAxis("Horizontal");
            //quando apertar botões como A ou D, gerar um valor float 

            //Esses valores são inseridos à um vector 3, cada float em seu devido eixo
            Vector3 direction = new Vector3(HorizMove, 0, VertMove);

            //limitar mov diagonal para não ficar mais rápido
            direction = Vector3.ClampMagnitude(direction, 1f);

            //agora com uma boa direção em vetor, vamos multiplicar por speed e Time.deltaTime
            Vector3 finalMovement = direction * speed * Time.deltaTime;

            //depois disso, vamos colocar o charactercontroller para se movimentar por meio
            //vetor que criamos
            cc.Move(finalMovement);
        }
        else
        {
            //fazer nada
        }
    }

    void ColorLogic()
    {
        // CORREÇÃO: Checar myRenderer e GameController antes de tentar acessar
        if (myRenderer != null && TESTGameController.controller != null)
        {
            if(currentColor == 1)
            {
                myRenderer.material = TESTGameController.controller.PlayerMatFirst;
            }
            else if (currentColor == 0)
            {
                myRenderer.material = TESTGameController.controller.PlayerMatSecond;
            }
        }
    }
    

    void HandleColorSwitchInput()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Alterna entre 0 e 1: Se for 1, vira 0. Se for 0, vira 1.
            currentColor = (currentColor == 1) ? 0 : 1;
        }
    }

    public void ReceiveDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Player recebeu " + damageAmount + "de dano. Vida restante:  " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        Debug.Log("Player morreu!");
        //Adicionar futuramente uma animação de morte, reiniciar a fase, etc
        SceneManager.LoadScene(2);
        Destroy(gameObject);
    }

    public void Curar(float quantidade)
    {
       currentHealth += (int)quantidade;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player curado! Vida atual: " + currentHealth);
    }

    public void DisableInputs()
    {
        DummyMode = true;
    }

    public void EnableInputs()
    {
        DummyMode = false;
    }
}
