using UnityEngine;

public class TutorialPlayer : MonoBehaviour
{
    [Header("Referências básicas")]
    private CharacterController cc;

    //variáveis extras

    public bool DummyMode = false;

    [Header("Gravidade, velocidade e afins")]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float speed;
    private Vector3 verticalVelocity;

    [Header("Cores, Materiais e afins")]
    public int currentColor;
    [SerializeField] Renderer MaterialRenderer;

    //como o material tá num array, precisa acessar essa posição usando variáveis auxiliares
    private const int INDEX_TARGET_MATERIAL = 1;

    //array pra guardar a sequência atual de materiais
    private Material[] ActualMaterials;

    void Initializing()
    {
        cc = GetComponent<CharacterController>();

    }

    void Awake()
    {
        Initializing();
    }

    void Start()
{
    if(TutorialController.controller != null)
    {
        TutorialController.controller.PlayerTutorialRef = this;
    }
    else
    {
        Debug.LogError("O Controller do tutorial te odeia!");
        return;
    }

    ApplyCurrentColor(); 
}

    void Update()
    {
        ApplyGravity();
        Movement();
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

    private void Movement()
    {
        if(!DummyMode)
        {
            float VertMove = Input.GetAxis("Horizontal");
            //quando apertar botões como W ou S, gerar um valor float

            float HorizMove = Input.GetAxis("Vertical");
            //quando apertar botões como A ou D, gerar um valor float 

            //Esses valores são inseridos à um vector 3, cada float em seu devido eixo
            Vector3 direction = new Vector3(HorizMove * -1, 0, VertMove);

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

    void ApplyCurrentColor()
{
    // O array deve ser inicializado APENAS UMA VEZ no Start.
    if (ActualMaterials == null)
    {
        if (MaterialRenderer == null) return;
        
        // Inicializa o array de materiais APENAS UMA VEZ
        ActualMaterials = MaterialRenderer.materials;
        
        // Verifica se o array foi criado corretamente
        if (ActualMaterials == null || ActualMaterials.Length <= INDEX_TARGET_MATERIAL)
        {
            Debug.LogError("MaterialRenderer ou Array de materiais é um corno manso e não quer funcionar");
            return;
        }
    }
    
    //lógica de troca das cores:
    Material NewMaterial = null;

    if (currentColor == 1)
    {
        NewMaterial = TutorialController.controller.MatFirst;
    }
    else if (currentColor == 0)
    {
        NewMaterial = TutorialController.controller.MatSecond;
    }
    
    if(NewMaterial != null)
    {
        // Troca APENAS o material no índice alvo
        ActualMaterials[INDEX_TARGET_MATERIAL] = NewMaterial;

        // Aplica a mudança
        MaterialRenderer.materials = ActualMaterials;
    }
    else
    {
        Debug.LogWarning("Material de cor não tem mãe e não foi atribuído no TutorialController!");
    }
}

    void HandleColorSwitchInput()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Alterna entre 0 e 1: Se for 1, vira 0. Se for 0, vira 1.
            currentColor = (currentColor == 1) ? 0 : 1;

            ApplyCurrentColor();
        }
    }
}
