using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void IniciarPartida()
    {
        SceneManager.LoadScene("Level1_Blockout");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
