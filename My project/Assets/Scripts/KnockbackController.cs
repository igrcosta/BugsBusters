using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KnockbackController : MonoBehaviour
{
    private CharacterController PlayerController;
    private Vector3 impact = Vector3.zero;

    public float knockbackDecay = 5.0f;
    //velocidade que a força do knockback vai diminuir

    void Start()
    {
        PlayerController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (impact.magnitude > 0.2f)
        {
            PlayerController.Move(impact * Time.deltaTime);
        }

        impact = Vector3.Lerp(impact, Vector3.zero, knockbackDecay * Time.deltaTime);
    }

    public void AddImpact(Vector3 direction, float force)
    {
        direction.Normalize();

        if (direction.y < 0)
        {
            direction.y = -direction.y;
        }

        impact += direction * force;
    }
}
