using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using UnityEngine.SceneManagement; 

public class SceneFader : MonoBehaviour
{
    // Variáveis que devem ser ligadas no Inspector
    [SerializeField] private CanvasGroup fadeCanvasGroup; 
    [SerializeField] private float fadeDuration = 1.0f; // Duração do fade em segundos

    private static SceneFader instance; // Singleton para fácil acesso

    public static SceneFader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneFader>();
                if (instance == null)
                {
                    // Caso o objeto não exista na cena inicial, cria-o
                    GameObject go = new GameObject("SceneFader");
                    instance = go.AddComponent<SceneFader>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        // === 1. Singleton e Persistência ===
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // CRÍTICO: Mantém o fader entre as cenas
        
        // === 2. Verificação de Componente ===
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("SceneFader: Fade Canvas Group não foi atribuído no Inspector!");
            // Lógica de busca opcional aqui
        }

        // === 3. Inscrição de Evento ===
        // Isso garante que o Fade In ocorra após QUALQUER cena ser carregada.
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // === 4. Fade In Inicial (para a cena que carregou o Fader) ===
        if (fadeCanvasGroup != null && fadeCanvasGroup.alpha == 1f)
        {
            StartCoroutine(FadeIn());
        }
    }

    // Garante que o evento seja desinscrito quando o objeto for destruído
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Método que inicia o Fade In assim que a nova cena é carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeCanvasGroup != null)
        {
            // Define o alpha como 1f (preto) e o bloqueia para garantir que a tela não pisque
            fadeCanvasGroup.alpha = 1f; 
            fadeCanvasGroup.blocksRaycasts = true;

            // Inicia o Fade In (clareamento)
            StartCoroutine(FadeIn());
        }
    }


    /// <summary>
    /// Faz a tela esmaecer para transparente (revela a cena).
    /// </summary>
    public IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            // Interpola de 1 (preto) para 0 (transparente)
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); 
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f; 
        fadeCanvasGroup.blocksRaycasts = false; // Permite cliques e interações
    }

    /// <summary>
    /// Faz a tela esmaecer para preto (prepara para a transição de cena) e carrega a cena.
    /// </summary>
    public IEnumerator FadeOutAndLoadScene(int sceneIndex)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("SceneFader: Falha no FadeOut, CanvasGroup é nulo. Carregando cena diretamente.");
            SceneManager.LoadScene(sceneIndex);
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true; // Bloqueia interações enquanto a tela escurece

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            // Interpola de 0 (transparente) para 1 (preto)
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration); 
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f; // Garante que a tela fique preta
        
        // Carrega a próxima cena DEPOIS que o fade estiver completo.
        // Isso dispara o OnSceneLoaded na cena de destino.
        SceneManager.LoadScene(sceneIndex);
    }
}