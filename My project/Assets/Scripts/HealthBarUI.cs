using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    internal static int value;
    [SerializeField] private Image healthBarFillImage;
    private Player player;
    [SerializeField]
    [Min(0.1f)]
    private float speed = 2;
    private void LateUpdate()
    {
        float healthPercent = (float)player.CurrentHealth / 100f;
        healthBarFillImage.fillAmount = Mathf.Lerp(healthBarFillImage.fillAmount, healthPercent, Time.deltaTime * speed);
        healthBarFillImage.fillAmount = healthPercent;
    }

    private void Start()
    {
        // Encontra o Player na cena
        player = FindFirstObjectByType<Player>(); 

        if (player == null)
        {
            Debug.LogError("O script HealthBarUI não encontrou o Player na cena. A barra não funcionará.");
        }
    }
}
