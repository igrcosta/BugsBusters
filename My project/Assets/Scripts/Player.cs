using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections; 

public class Player : MonoBehaviour
{
    // ====================================================================
    // 1. REFERÊNCIAS E COMPONENTES
    // ====================================================================

    [Header("Componentes e Referências")]
    private CharacterController cc; // Variável CharacterController
    private Slider HealthBarUI;
    [SerializeField] private Transform CameraTarget; 

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    [SerializeField] float speed = 10f; // 💡 GARANTIR UM VALOR PADRÃO

    public bool DummyMode = true;

    [Header("Gravidade")]
    [SerializeField] float gravity = -9.81f;
    private Vector3 verticalVelocity;

    // ====================================================================
    // 2. CORES E MATERIAIS
    // ====================================================================

    [Header("Cores, Materiais e afins")]
    public int currentColor;
    [SerializeField] Renderer MaterialRenderer; 
    
    private const int INDEX_TARGET_MATERIAL = 1; 
    private Material[] ActualMaterials;

    [Header("Referência de Rotação")]
    [SerializeField] private Transform PlayerCabecaTransform;

    // ====================================================================
    // 3. INICIALIZAÇÃO E REGISTRO (CRÍTICO)
    // ====================================================================

    void Initializing()
    {
        // ❌ REMOVIDA A CHAMADA cc = GetComponent<CharacterController>(); (Feito no Awake)
        currentHealth = maxHealth;
        Debug.Log("Player Health: " + currentHealth);

        // Busca o Slider. 
        HealthBarUI = FindFirstObjectByType<Slider>(); 

        // Lógica de Materiais
        if (MaterialRenderer != null)
        {
            ActualMaterials = MaterialRenderer.materials;
            if (ActualMaterials.Length <= INDEX_TARGET_MATERIAL)
            {
                Debug.LogError("O Renderer do PlayerCorpo não possui materiais suficientes.");
            }
        }
        else
        {
            Debug.LogError("MaterialRenderer (corpo) não atribuído no Inspector do Player.");
        }
        
        // Lógica de Cores depende do GameController
        if (GameControllerScript.controller != null)
        {
            currentColor = GameControllerScript.controller.ColorLogic[0];
            ApplyCurrentColor(); 
        }
    }

    void Awake()
    {
        // 🚨 CORREÇÃO CRÍTICA 1: Tenta pegar o CharacterController aqui
        // (Use GetComponentInParent se o script estiver em um filho e o CC no pai)
        cc = GetComponent<CharacterController>(); 
        
        if (cc == null) 
        {
            Debug.LogError("Player.cs: FATAL: CharacterController não encontrado! O Player não andará.");
        }

        // 🚨 CORREÇÃO CRÍTICA 2: REGISTRO IMEDIATO
        // Garante que o GameController encontre o Player antes de iniciar a wave.
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.Player = this;
            Debug.Log("Player registrado no GameController (Awake).");
        }
    }

    void Start()
    {
        // Chama a inicialização de stats e cores, após o registro no Awake.
        Initializing();
    }

    // ====================================================================
    // 4. LÓGICA DE JOGO (UPDATE/MOVEMENT)
    // ====================================================================

    void Update()
    {
        // 🚨 CHECAGEM DE SEGURANÇA PARA CC
        if (cc == null) return; 
        
        ApplyGravity();
        Movement();
        HandleColorSwitchInput();
    }

    void ApplyGravity()
    {
        if (cc == null) return;

        // cc != null já é checado no Update
        if (cc.isGrounded)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += gravity * Time.deltaTime;
        cc.Move(verticalVelocity * Time.deltaTime);
    }
    
    public void Movement()
    {
        if (Camera.main == null)
    {
        Debug.LogError("FATAL: Camera principal (tag MainCamera) não encontrada.");
        return;
    }

        if(!DummyMode)
        {
            float VertMove = Input.GetAxis("Vertical");
            float HorizMove = Input.GetAxis("Horizontal");

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 finalDirection = (cameraForward * VertMove) + (cameraRight * HorizMove);
            finalDirection = Vector3.ClampMagnitude(finalDirection, 1f);

            // Linha 129 Antiga (Agora é a nova linha do cc.Move)
            Vector3 finalMovement = finalDirection * speed * Time.deltaTime; 

            cc.Move(finalMovement);
            
        }
    }

    // ... (O resto do código HandleColorSwitchInput, ReceiveDamage, etc. permanece o mesmo)
    
    // ====================================================================
    // 5. LÓGICA DE CORES E COMBATE
    // ====================================================================
    
    void ApplyCurrentColor()
    {
        // Checa se o ambiente e as referências estão prontas
        if (GameControllerScript.controller == null || MaterialRenderer == null || ActualMaterials == null || ActualMaterials.Length <= INDEX_TARGET_MATERIAL)
        {
            return;
        }
        
        Material NewMaterial = null;

        if (currentColor == 1)
        {
            NewMaterial = GameControllerScript.controller.PlayerMatFirst;
        }
        else if (currentColor == 0)
        {
            NewMaterial = GameControllerScript.controller.PlayerMatSecond;
        }
        
        if(NewMaterial != null)
        {
            ActualMaterials[INDEX_TARGET_MATERIAL] = NewMaterial;
            MaterialRenderer.materials = ActualMaterials;
        }
    }

    void HandleColorSwitchInput()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentColor = (currentColor == 1) ? 0 : 1;
            ApplyCurrentColor();
        }
    }
    
    public void TakingDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Player recebeu " + damageAmount + " de dano. Vida restante:  " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        Debug.Log("Player morreu!");
        SceneManager.LoadScene(2);
        Destroy(gameObject);
    }

    public void DisableInputs()
    {
        DummyMode = true;
    }

    public void EnableInputs()
    {
        DummyMode = false;
        
        // 🚨 REMOVIDA A ROTAÇÃO AQUI: Ela é sobrescrita a cada frame pelo GunScript.
        // Quaternion correctRotation = Quaternion.Euler(90f, 0f, 0f);
        // PlayerCabecaTransform.localRotation = correctRotation;
    }
}