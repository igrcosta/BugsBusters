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
    private ColorHandler playerColorHandler;
    private Slider HealthBarUI;

    private bool IsPaused = false;

    [SerializeField] private Transform CameraTarget; 

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    [SerializeField] float speed = 10f; // 💡 GARANTIR UM VALOR PADRÃO

    [Header("Luz do Player")]
    [SerializeField] private Light playerSpotlight; 
    [SerializeField] private Color greenLightColor = Color.green;
    [SerializeField] private Color redLightColor = Color.red;

    public bool DummyMode = true;

    [Header("Gravidade")]
    [SerializeField] float gravity = -9.81f;
    private Vector3 verticalVelocity;

    [Header("Tutorial")]
    private TutorialController tutorialControllerRef;
    private TutorialManager tutorialManagerRef;

    [Header("Som de Dano")]
    [SerializeField] private AudioSource hitAudioSource;

    // ====================================================================
    // 2. Rotações
    // ====================================================================

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
        playerColorHandler = GetComponent<ColorHandler>();

        UpdatePlayerSpotlightColor();

        if (playerColorHandler == null)
        {
            Debug.LogError("Player tá sem ColorHandler para funcionar. VAI SE FUDE");
            enabled = false;
            return;
        }

        Initializing();

        tutorialControllerRef = FindObjectOfType<TutorialController>();
        tutorialManagerRef = FindObjectOfType<TutorialManager>();

        if (tutorialControllerRef != null)
        {
            tutorialControllerRef.PlayerTutorialRef = this;
        }
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

        Pause();
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
    
    
    void HandleColorSwitchInput()
    {
        // 💡 NOVO: Verifica o input.
        if (!DummyMode && (Input.GetKeyDown(KeyCode.LeftShift)))
        {
             if (playerColorHandler == null) return;
    
            // 1. Alterna o estado do enum
            if (playerColorHandler.currentColor == BulletColor.Green)
            {
                playerColorHandler.currentColor = BulletColor.Red;
            }
            else
            {
                playerColorHandler.currentColor = BulletColor.Green;
            }
        
            // 2. Aplica o visual (o ColorHandler faz a troca de material)
            playerColorHandler.UpdateVisualMaterial();

            UpdatePlayerSpotlightColor();
            //atualiza a cor do fum~e embaixo do player pra ele ficar tunado fi slc pai tá chave, ixquece
        }
    }

    public void UpdatePlayerSpotlightColor()
    {
        if (playerSpotlight == null || playerColorHandler == null) return;

        BulletColor currentColor = playerColorHandler.currentColor;
        //pega ref da cor atual que o player tá usando

        Color targetLightColor = (currentColor == BulletColor.Green) ? greenLightColor : redLightColor;
        //define a cor que o player vai usar, se tu for daltônico, faz o L

        playerSpotlight.color = targetLightColor;
        //aplica a cor que foi escolhida
    }
    
    public void TakingDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (hitAudioSource != null)
            hitAudioSource.Play();

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

    public void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == false)
        {
            IsPaused = true;
            DummyMode = true;
            Time.timeScale = 0;

            GameControllerScript.controller.GameUI.pauseMenu.gameObject.SetActive(true);
            
            
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == true)
        {
            IsPaused = false;
            DummyMode = false;
            Time.timeScale = 1;
            GameControllerScript.controller.GameUI.pauseMenu.gameObject.SetActive(false);
            
        }
    }
}