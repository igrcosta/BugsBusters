using UnityEngine;
using UnityEngine.UI; // Garante que a classe Text esteja acessível

public class GameUI : MonoBehaviour
{
    internal static int value;
    [SerializeField] private Image healthBarFillImage;
    private Player player;
    [SerializeField]
    [Min(0.1f)]
    private float speed = 2;

    public Image pauseMenu;

    [SerializeField] Text InimigosMortosText; // NOVO: Referência para o componente Text na HUD

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
        if (GameControllerScript.controller != null)
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
            InimigosMortosText.text = "Inimigos Mortos: " + kills;
        }
    }
}