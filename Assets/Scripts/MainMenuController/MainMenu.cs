using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Cena do Jogo")]
    [Tooltip("Nome EXATO da cena do jogo como aparece em File > Build Settings > Scenes In Build.")]
    [SerializeField] private string gameSceneName = "";

    [Tooltip("Se verdadeiro e 'gameSceneName' estiver vazio, tenta carregar a próxima cena do Build Settings.")]
    [SerializeField] private bool fallbackToNextScene = true;

#if UNITY_EDITOR
    // Qualquer cena do projeto; o nome é copiado para 'gameSceneName' automaticamente (evita erro de digitação)
    [SerializeField] private UnityEditor.SceneAsset gameSceneAsset;

    private void OnValidate()
    {
        if (gameSceneAsset != null)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(gameSceneAsset);
            string name = Path.GetFileNameWithoutExtension(path);
            if (gameSceneName != name)
                gameSceneName = name;
        }
    }
#endif

    // Botão "Jogar"
    public void Play()
    {
        // Se um nome foi definido, tenta carregar por nome (desde que esteja nas Scenes In Build)
        if (!string.IsNullOrWhiteSpace(gameSceneName))
        {
            if (IsSceneInBuildSettings(gameSceneName))
            {
                Debug.Log($"Carregando cena '{gameSceneName}'...");
                SceneManager.LoadSceneAsync(gameSceneName);
                return;
            }
            else
            {
                Debug.LogError($"A cena '{gameSceneName}' não está em File > Build Settings > Scenes In Build. Adicione-a para carregar corretamente.");
                return;
            }
        }

        // Sem configuração: comportamento simples (log) ou fallback para próxima cena
        if (fallbackToNextScene)
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int total = SceneManager.sceneCountInBuildSettings;
            if (currentIndex + 1 < total)
            {
                string nextPath = SceneUtility.GetScenePathByBuildIndex(currentIndex + 1);
                string nextName = Path.GetFileNameWithoutExtension(nextPath);
                Debug.Log($"Carregando próxima cena em Build Settings: '{nextName}' (índice {currentIndex + 1}).");
                SceneManager.LoadSceneAsync(currentIndex + 1);
                return;
            }
        }

        // Caso não haja cena configurada nem próxima no Build Settings
        Debug.Log("Jogo iniciado (sem cena configurada). Adicione a cena do jogo ao Build Settings ou preencha 'gameSceneName' no Inspector.");
    }

    // Botão "Sair do Jogo"
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Para o Play Mode no Editor
#elif UNITY_WEBGL
        Debug.Log("Application.Quit() não é suportado no WebGL.");
#else
        Application.Quit(); // Fecha o aplicativo no build
#endif
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        int total = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < total; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}