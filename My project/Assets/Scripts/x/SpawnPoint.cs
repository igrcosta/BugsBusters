using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] GameObject enemyMantis;
    [SerializeField] GameObject enemyBeetle;
    [SerializeField] GameObject enemyLadyBug;

    [Header("Spawn Settings")]
    [SerializeField] float spawnInterval = 1f;   // tempo entre cada spawn
    [SerializeField] float spawnRangeX = 1f;     // limite horizontal

    [Header("Limits")]
    [SerializeField] int maxEnemies = 10;        // n�mero m�ximo permitido
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

        // 1. Posição base do SpawnPoint (this.transform.position)
    Vector3 basePos = transform.position;

    // 2. Cria deslocamentos aleatórios (jitter) em X e Z
    // Usamos spawnRangeX como o limite máximo do deslocamento (o raio)
    float jitterX = Random.Range(-spawnRangeX, spawnRangeX);
    // Usa um jitterRange semelhante para Z
    float jitterZ = Random.Range(-spawnRangeX, spawnRangeX); 
    
    // 3. Aplica o deslocamento à posição base, mantendo o Y inalterado (para a queda)
    Vector3 finalSpawnPos = new Vector3(
        basePos.x + jitterX,
        basePos.y,
        basePos.z + jitterZ
    );
    
    // --- FIM DA LÓGICA DO JITTER ---

        // Instancia o inimigo na POSIÇÃO FINAL CALCULADA
    GameObject enemy = Instantiate(prefabToSpawn, finalSpawnPos, transform.rotation); 

    // sempre caindo para baixo
    Rigidbody rb = enemy.GetComponent<Rigidbody>();
    if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, -1f, 0); 
        }

        currentEnemies++;
    }
}