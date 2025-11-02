using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonesMenu : MonoBehaviour
{
    
    public void IniciarPartida()
    {
        SceneManager.LoadScene("Level1_Blockout");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
