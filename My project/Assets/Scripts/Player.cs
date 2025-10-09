using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
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
        Vector3 value = new Vector3(HorizMove, -10f, VertMove);

        //limitar mov diagonal para não ficar mais rápido
        value = Vector3.ClampMagnitude(value, speed);

        //depois disso, vamos colocar o charactercontroller para se movimentar por meio
        //vetor que criamos
        cc.Move(value);
    }
}
