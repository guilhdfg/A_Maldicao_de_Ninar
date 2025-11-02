using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenu : MonoBehaviour
{
    [Header("Cena do jogo")]
    [Tooltip("Nome EXATO da cena do jogo (precisa estar em File > Build Settings > Scenes In Build).")]
    [SerializeField] private string gameSceneName = "CenaMain";

    [Header("Referências de UI (opcional, auto-detecta por nome se deixar vazio)")]
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonQuit;

    [Header("Áudio SFX (cliques)")]
    [Tooltip("Fonte 2D para SFX (Play On Awake = OFF, Loop = OFF). Se não houver, o script cria uma.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Som de clique padrão (para Play e, se exitClickClip estiver vazio, para Quit também).")]
    [SerializeField] private AudioClip clickClip;
    [Tooltip("Som ao clicar em Sair (se vazio, usa clickClip).")]
    [SerializeField] private AudioClip exitClickClip;
    [Tooltip("Espera o clipe completo antes de executar a ação (Play/Quit)?")]
    [SerializeField] private bool waitFullClickClip = false;
    [Tooltip("Atraso mínimo antes da ação quando não esperar o clipe inteiro.")]
    [SerializeField] private float minClickDelay = 0.05f;

    [Header("Música do menu (opcional)")]
    [Tooltip("Fonte de música (2D). Se quiser, o script faz fade-out ao trocar de cena/sair.")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool fadeOutMusicOnAction = true;
    [SerializeField] private float musicFadeTime = 0.3f;

    private bool quitting;

    private void Awake()
    {
        // Tenta auto-atribuir botões por nome se não foram informados
        if (buttonPlay == null)
        {
            var go = GameObject.Find("ButtonPlay");
            if (go) buttonPlay = go.GetComponent<Button>();
        }
        if (buttonQuit == null)
        {
            var go = GameObject.Find("ButtonQuit");
            if (go) buttonQuit = go.GetComponent<Button>();
        }

        // Garante um AudioSource para SFX (2D) se não houver
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f; // 2D
        }

        // Conecta os botões via código
        if (buttonPlay != null)
        {
            buttonPlay.onClick.RemoveListener(OnPlayClicked); // evita duplicar se já conectado
            buttonPlay.onClick.AddListener(OnPlayClicked);
        }
        else
        {
            Debug.LogWarning("MainMenu: ButtonPlay não encontrado. Arraste a referência no Inspector ou renomeie o botão para 'ButtonPlay'.");
        }

        if (buttonQuit != null)
        {
            buttonQuit.onClick.RemoveListener(OnQuitClicked);
            buttonQuit.onClick.AddListener(OnQuitClicked);
        }
        else
        {
            Debug.LogWarning("MainMenu: ButtonQuit não encontrado. Arraste a referência no Inspector ou renomeie o botão para 'ButtonQuit'.");
        }
    }

    private void OnPlayClicked()
    {
        // Som de clique (se houver)
        float delay = 0f;
        if (sfxSource != null && clickClip != null)
        {
            sfxSource.PlayOneShot(clickClip);
            delay = waitFullClickClip ? clickClip.length : Mathf.Max(minClickDelay, 0.01f);
        }

        // Fade na música (opcional)
        if (fadeOutMusicOnAction && musicSource != null)
            StartCoroutine(FadeOut(musicSource, musicFadeTime));

        // Carrega a cena após o delay (usa tempo real para não depender de timeScale)
        StartCoroutine(LoadSceneAfter(delay));
    }

    private void OnQuitClicked()
    {
        if (quitting) return;
        quitting = true;

        // Toca som de sair (ou o mesmo de clique)
        float delay = 0f;
        var clip = exitClickClip != null ? exitClickClip : clickClip;
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            delay = waitFullClickClip ? clip.length : Mathf.Max(minClickDelay, 0.01f);
        }

        if (fadeOutMusicOnAction && musicSource != null)
            StartCoroutine(FadeOut(musicSource, musicFadeTime));

        StartCoroutine(QuitAfter(delay));
    }

    private IEnumerator LoadSceneAfter(float delay)
    {
        float wait = Mathf.Max(delay, fadeOutMusicOnAction ? musicFadeTime : 0f);
        if (wait > 0f) yield return new WaitForSecondsRealtime(wait);

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("MainMenu: 'gameSceneName' não definido. Preencha com o nome exato da cena (ex.: CenaMain).");
            yield break;
        }

        // Tenta carregar a cena
        AsyncOperation op;
        try
        {
            op = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        }
        catch
        {
            Debug.LogError($"MainMenu: Falha ao carregar '{gameSceneName}'. Certifique-se de adicioná-la em File > Build Settings > Scenes In Build.");
            yield break;
        }

        if (op != null) op.allowSceneActivation = true;
    }

    private IEnumerator QuitAfter(float delay)
    {
        float wait = Mathf.Max(delay, fadeOutMusicOnAction ? musicFadeTime : 0f);
        if (wait > 0f) yield return new WaitForSecondsRealtime(wait);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Debug.Log("Application.Quit() não é suportado no WebGL.");
#else
        Application.Quit();
#endif
    }

    private IEnumerator FadeOut(AudioSource source, float time)
    {
        if (source == null || time <= 0f) yield break;

        float start = source.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
        source.volume = start; // restaura valor para usos futuros
    }
}