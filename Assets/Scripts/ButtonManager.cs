using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonManager : MonoBehaviour
{


    public void Restart()
    {
        SceneManager.LoadScene("CenaMain");
        Debug.Log("Penis");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
        Debug.Log("Penis");
    }
}
