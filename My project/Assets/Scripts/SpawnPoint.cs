using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyMantis;
    [SerializeField] GameObject enemyBeetle;
    [SerializeField] GameObject enemyLadyBug;

    [Header("Spawn Settings")]
    [SerializeField] float spawnInterval = 1f;   // tempo entre cada spawn
    [SerializeField] float spawnRangeX = 0.1f;     // limite horizontal

    [Header("Limits")]
    [SerializeField] int maxEnemies = 10;        // número máximo permitido
    private int currentEnemies = 0;              // contador interno

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    private void SpawnEnemy()
    {
        // Impede spawn acima do limite
        if (currentEnemies >= maxEnemies)
            return;

        // Escolhe tipo de inimigo aleatoriamente
        float roll = Random.value;
        GameObject prefabToSpawn;

        if (roll < 0.4f) prefabToSpawn = enemyMantis;      // (0.4) = 40%
        else if (roll < 0.75f) prefabToSpawn = enemyBeetle; // (0.75f) = 35%
        else prefabToSpawn = enemyLadyBug;                  // 25%

        if (prefabToSpawn == null) return;

        // Posição do Empty Object (SpawnPoint)
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //Vector3 spawnPos = new Vector3(0, 0, 0);

        // Instancia o inimigo
        //GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        GameObject enemy = Instantiate(prefabToSpawn, transform.position, transform.rotation); //instancia um prefab do inimigo na posição do nosso spawner

        // sempre caindo para baixo
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, -1f, 0); 
        }

        currentEnemies++;
    }
}