using UnityEngine;
using UnityEngine.UI; // Para Text
using TMPro;          // Caso use TextMeshPro
using UnityEngine.SceneManagement;

public class SurvivalTimer : MonoBehaviour
{
    [Header("Referências")]
    public Text uiText; // caso use Text normal
    public TextMeshProUGUI tmpText; // caso use TMP

    [Header("Configuração")]
    public float timeToSurvive = 360f; // 6 minutos (em segundos)
    private float elapsedTime = 0f;
    private bool hasWon = false;

    void Update()
    {
        if (hasWon) return;

        elapsedTime += Time.deltaTime;

        float remaining = timeToSurvive - elapsedTime;
        if (remaining <= 0f)
        {
            WinGame();
            return;
        }

        UpdateTimerUI(elapsedTime);
    }

    void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        string formatted = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (uiText != null)
            uiText.text = formatted;
        else if (tmpText != null)
            tmpText.text = formatted;
    }

    void WinGame()
    {
        hasWon = true;
        Debug.Log("?? Sobreviveu 6 minutos! Vitória!");

        // Aqui muda pra próxima cena
        SceneManager.LoadScene("CenaVitoria"); // coloque o nome certo da cena
    }
}
