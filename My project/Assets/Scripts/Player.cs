using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float speed;
    private float gravity = -9.81f;
    private float jumpHeight = 1.5f;
    //depois fazer player pulando e sistema de gravidade

    private CharacterController cc;
    // criamos uma variável do tipo CharacterController chamada cc

    void Start()
    {
        cc = GetComponent<CharacterController>();
        //pegamos o CharacterController do player e jogamos na variável chamada cc
    }

    void Update()
    {
        Movement();
    }
    
    public void Movement()
    {
        float VertMove = Input.GetAxis("Vertical") * Time.deltaTime * speed;
        //quando apertar botões como W ou S, gerar um valor float * time.deltatime * speed

        float HorizMove = Input.GetAxis("Horizontal") * Time.deltaTime * speed; 
        //quando apertar botões como A ou D, gerar um valor float * time.deltatime * speed

        //Esses valores são inseridos à um vector 3, cada float em seu devido eixo
        Vector3 value = new Vector3(HorizMove, 0, VertMove);

        //depois disso, vamos colocar o charactercontroller para se movimentar por meio
        //vetor que criamos
        cc.Move(value);
    }
}
