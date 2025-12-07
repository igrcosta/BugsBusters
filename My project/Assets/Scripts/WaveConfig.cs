using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnRate
{
    [Tooltip("O Prefab do inimigo (Enemy1, BettleEnemy, etc.)")]
    public GameObject enemyPrefab;
    
    [Tooltip("A chance (peso) desse inimigo spawnar. Ex: 80 para Enemy1, 20 para Besouro.")]
    public int spawnWeight = 1;
}

[System.Serializable]
public class WaveConfig
{
    [Header("Configuração da Wave")]
    [Tooltip("Número de Spawners a ativar nesta wave")]
    public int numberOfSpawnersToActivate = 3;
    
    [Tooltip("Lista de inimigos que podem spawnar e sua chance de spawn")]
    public List<EnemySpawnRate> enemyRates;
    
    [Tooltip("Quantas vezes cada Spawner ativo vai tentar spawnar um inimigo (EnemyCounter)")]
    public int spawnAttemptsPerSpawner = 5; 
}