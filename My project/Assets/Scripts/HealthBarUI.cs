using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBarFillImage;
    [SerializeField] private Player player;
    [SerializeField]
    [Min(0.1f)]
    private float speed = 2;
    private void LateUpdate()
    {
        float healthPercent = (float)player.CurrentHealth / 100f;
        healthBarFillImage.fillAmount = Mathf.Lerp(healthBarFillImage.fillAmount, healthPercent, Time.deltaTime * speed);
        healthBarFillImage.fillAmount = healthPercent;
    }
}
