using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    [SerializeField] float speed;

    [Header("Gravidade")]
    [SerializeField] float gravity = -9.81f;

    private CharacterController cc;
    // criamos uma variável do tipo CharacterController chamada cc

    private Vector3 verticalVelocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        //pegamos o CharacterController do player e jogamos na variável chamada cc
        currentHealth = maxHealth;
        Debug.Log("Player Health: " + currentHealth);
    }

    void Update()
    {
        ApplyGravity();
        Movement();
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
        SceneManager.LoadScene(1);
        Destroy(gameObject);
    }

}
