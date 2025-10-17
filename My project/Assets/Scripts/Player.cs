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

    [SerializeField] PlayerInput InputsComponent;
    public bool DummyMode = true;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        //pegamos o CharacterController do player e jogamos na variável chamada cc
        currentHealth = maxHealth;
        Debug.Log("Player Health: " + currentHealth);

        HealthBarUI = FindFirstObjectByType<Slider>();

        currentColor = GameControllerScript.controller.ColorLogic[0];
        
        //acessamos o PlayerModel pra mudar seu material
        Transform playerModelTransform = transform.Find("PlayerModel");

        myRenderer = playerModelTransform.GetComponent<Renderer>();
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
            float VertMove = Input.GetAxis("Vertical") * Time.deltaTime * speed;
            //quando apertar botões como W ou S, gerar um valor float * time.deltatime * speed

            float HorizMove = Input.GetAxis("Horizontal") * Time.deltaTime * speed; 
            //quando apertar botões como A ou D, gerar um valor float * time.deltatime * speed

            //Esses valores são inseridos à um vector 3, cada float em seu devido eixo
            Vector3 value = new Vector3(HorizMove, 0, VertMove);

            //limitar mov diagonal para não ficar mais rápido
            value = Vector3.ClampMagnitude(value, speed);

            //depois disso, vamos colocar o charactercontroller para se movimentar por meio
            //vetor que criamos
            cc.Move(value);
        }
        else
        {
            //fazer nada
        }
    }

    void ColorLogic()
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
        InputsComponent.DeactivateInput();
        DummyMode = true;
    }

    public void EnableInputs()
    {
        InputsComponent.ActivateInput();
        DummyMode = false;
    }
}
