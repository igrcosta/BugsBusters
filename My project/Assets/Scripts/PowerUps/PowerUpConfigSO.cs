using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PowerUpConfigSO", menuName = "Scriptable Objects/PowerUpConfigSO")]
public class PowerUpConfigSO : ScriptableObject
{
    [Header("Configuração Visual e Prefab")]
    [Tooltip("O prefab real do Power-Up a ser instanciado.")]
    public GameObject itemPrefab;
    
    [Header("Configuração de Spawn e Balanceamento")]
    [Tooltip("O peso (chance relativa) deste item aparecer. Ex: 50 é 5x mais comum que 10.")]
    public int spawnWeight = 1;

    [Tooltip("O tempo de espera (em segundos) para este item reaparecer, se coletado.")]
    public float respawnTime = 10f;

    [Header("Detalhes do Efeito (Exclusivo para o item)")]
    [Tooltip("Usado por scripts de Power-Up para configurar o efeito. Ex: 25 para cura, 10 para segundos de efeito.")]
    public float effectValue = 1f;
}
