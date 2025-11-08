using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public Pausa pausaScript;
    
    public void RegresarAMenu()
    {
        pausaScript.TogglePause(false);
        
        // Usar transición si está disponible
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ReiniciarPartida()
    {
        pausaScript.TogglePause(false);
        
        // Usar transición si está disponible
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.RestartCurrentScene();
        }
        else
        {
            SceneManager.LoadScene("Level1_Blockout");
        }
    }

    public void RegresarAJuego()
    {
        pausaScript.TogglePause(false);
    }
}
