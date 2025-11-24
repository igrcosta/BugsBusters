using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController controller;

    [Header("Referências")]
    public TutorialPlayer PlayerTutorialRef;

    [Header("Materiais que todos usam")]
    public Material MatFirst, MatSecond;

    [Header("Progresso do Tutorial")]
    [SerializeField] private int enemiesToKillForNextStep = 1;
    private int enemiesKilled = 0;

    private void Awake()
    {
        if (controller == null)
        {
            controller = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Chamado pelos inimigos ao morrer.
    /// </summary>
    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"[Tutorial] Inimigo morto: {enemiesKilled}/{enemiesToKillForNextStep}");

        if (enemiesKilled >= enemiesToKillForNextStep)
        {
            enemiesKilled = 0;
            AdvanceTutorialStep();
        }
    }

    /// <summary>
    /// Avança para a próxima fase.
    /// Aqui você ativa/destrói paredes, libera spawners etc.
    /// </summary>
    private void AdvanceTutorialStep()
    {
        Debug.Log("[Tutorial] Avançando para a próxima fase.");

        // TODO: Coloque aqui a lógica real do avanço:
        // - Destruir parede atual
        // - Ativar próxima parede/spawn
        // - Mudar UI se houver
        //
        // Exemplo:
        // currentWall.SetActive(false);
        // nextPhaseSpawner.SetActive(true);
    }
}
