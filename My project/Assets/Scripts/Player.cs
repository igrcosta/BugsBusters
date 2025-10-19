using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
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
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.Player = this;
        }
        
        cc = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        Debug.Log("Player Health: " + currentHealth);

        HealthBarUI = FindFirstObjectByType<Slider>();

        // Checagem de Renderer (Pode falhar se o filho não existir)
        Transform playerModelTransform = transform.Find("PlayerModel");
        if (playerModelTransform != null)
        {
            myRenderer = playerModelTransform.GetComponent<Renderer>();
        }
        else
        {
            Debug.LogError("PlayerModel não encontrado. As cores não serão aplicadas.");
        }
        
        // Lógica de Cores depende do GameController (agora seguro pelo while)
        if (GameControllerScript.controller != null)
        {
            currentColor = GameControllerScript.controller.ColorLogic[0];
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
        if (myRenderer != null && GameControllerScript.controller != null)
        {
            if(currentColor == 1)
            {
                myRenderer.material = GameControllerScript.controller.PlayerMatFirst;
            }
            else if (currentColor == 0)
            {
                myRenderer.material = GameControllerScript.controller.PlayerMatSecond;
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
