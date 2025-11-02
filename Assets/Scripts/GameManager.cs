using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public void WinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    public void LoseScene()
    {
        SceneManager.LoadScene("LoseScene");
    }
}
