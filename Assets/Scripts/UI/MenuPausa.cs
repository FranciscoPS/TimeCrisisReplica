using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public Pausa pausaScript;
    public void RegresarAMenu()
    {
        pausaScript.TogglePause(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void ReiniciarPartida()
    {
        pausaScript.TogglePause(false);
        SceneManager.LoadScene("Level1_Blockout");
    }

    public void RegresarAJuego()
    {
        pausaScript.TogglePause(false);
    }
}
