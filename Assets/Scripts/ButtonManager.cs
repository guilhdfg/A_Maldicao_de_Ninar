using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonManager : MonoBehaviour
{


    public void Restart()
    {
        SceneManager.LoadScene("CenaMain");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
