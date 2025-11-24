//(anexe à CameraAnchor)
using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    // Armazena a rotação local desejada (normalmente Quaternion.identity, ou seja, 0, 0, 0)
    private Quaternion initialLocalRotation;

    void Awake()
    {
        // Pega a rotação local inicial (do Prefab).
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // Força a rotação local a ser sempre a inicial a cada frame.
        // Isso anula a rotação que a CameraAnchor herdaria do Player quando ele vira (transform.rotation).
        transform.localRotation = initialLocalRotation;
    }
}