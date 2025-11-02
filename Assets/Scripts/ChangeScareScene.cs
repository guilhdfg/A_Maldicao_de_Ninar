using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScareScene : MonoBehaviour
{
    float tempoRestante = 2.3f;

    void Update()
    {
        if (tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;
            if (tempoRestante <= 0)
            {
                SceneManager.LoadScene("LoseScene");
            }
        }
    }
}
