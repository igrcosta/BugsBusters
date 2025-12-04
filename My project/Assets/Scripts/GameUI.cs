using UnityEngine;
using UnityEngine.UI; // Garante que a classe Text esteja acessível
using System.Collections; // para usar a MASTERPIECE dos games, A.K.A coroutines!

public class GameUI : MonoBehaviour
{
    internal static int value;
    [SerializeField] private Image healthBarFillImage;
    private Player player;
    [SerializeField]
    [Min(0.1f)]
    private float speed = 2;

    public GameObject pauseMenu;

    [SerializeField] Text InimigosMortosText; // NOVO: Referência para o componente Text na HUD
    [Header ("Barra de vida PLAYER")]
    public GameObject HPContainer;
    public GameObject HPHeart;

    [Header("Barras de vida do boss")]

    private boss BossRef;
    public RawImage BossGreenBar;
    public RawImage BossRedBar;

    private void LateUpdate()
    {
        // Certifique-se de que o player não é null antes de tentar acessar CurrentHealth
        if (player != null)
        {
            float healthPercent = (float)player.CurrentHealth / 100f;
            
            // Note: Você está aplicando a interpolação (Lerp) e depois sobrescrevendo imediatamente.
            // Para ter a animação suave, você deve usar SOMENTE o Lerp:
            // healthBarFillImage.fillAmount = Mathf.Lerp(healthBarFillImage.fillAmount, healthPercent, Time.deltaTime * speed);
            // OU, para ter a atualização imediata:
            healthBarFillImage.fillAmount = healthPercent;
            // Vou manter o seu código para atualização imediata por enquanto.
        }
    }

    private void Awake()
    {
        if(GameControllerScript.controller != null)
        {
            // Registra o Canvas do jogo para o GameController
            GameControllerScript.controller.GameUI = this;
        }
    }

    private void Start()
    {
        // Pega o player do GameController
        player = GameControllerScript.controller.Player;
    }

    public void AlterarInimigosMortosnaHUD(int kills)
    {
        if (InimigosMortosText != null)
        {
            InimigosMortosText.text = "Enemies Defeated: " + kills;
        }
    }

    //daqui pra frente vai ser tudo que envolve a barra de vida do boss na última fase

    public void UpdateBossBar()
    {
        BossRef = GameControllerScript.controller.BossRef;

        Vector3 lifebarScale = BossGreenBar.rectTransform.localScale;
        //pegamos a escala da barra verde e jogamos para um vector 3

        lifebarScale.x = (float)BossRef.Health/ BossRef.MaxHealth;
        //definimos a escala da barra de vida em x para um valor float da divisão entre a vida atual do boss com a vida máxima dele (esse cálculo serve pra qualquer ser vivo com health bar)

        BossGreenBar.rectTransform.localScale = lifebarScale;
        //aplicamos o tamanho local da barra verde de vida com o cálculo da redução de x em base da sua vida

        StartCoroutine(DecreasingRedBar(lifebarScale));
        //iniciamos a coroutina para diminuir a barra vermelha de vida um pouco depois da barra vermelha e de forma gradual
    }

    public void BossBarCondition()
    {
        BossRef = GameControllerScript.controller.BossRef;

        if (BossRef.IsVulnerable)
        {
            BossGreenBar.enabled = true;
        }
        else
        {
            BossGreenBar.enabled = false;
        }
    }

    IEnumerator DecreasingRedBar(Vector3 newScale)
    {
        yield return new WaitForSeconds(0.5f);

        Vector3 redBarScale = BossRedBar.transform.localScale;
        //pegamos a escala da barra vermelha e colocamos um vector3 para guardar seus valores

        //enquanto a escala em x da barra vermelha de vida, estiver maior que a escala em x da barra de vida verde...
        while(BossRedBar.transform.localScale.x > newScale.x)
        {
            redBarScale.x -= Time.deltaTime * 0.25f;
            BossRedBar.transform.localScale = redBarScale;

            //a gente faz a redBarScale em x se reduzir de pouco em pouco conforme passa o tempo
            //e aplica isso no transform da barra vermelha

            yield return null;
        }

        BossRedBar.transform.localScale = newScale;
        //definimos o tamanho para ficar exatamente igual o da escala do verde (isso evita bugs)
    }
}