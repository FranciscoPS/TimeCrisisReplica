using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public Pausa pausaScript;
    
    public void RegresarAMenu()
    {
        if (pausaScript != null)
        {
            pausaScript.TogglePause(false);
        }
        else
        {
            Debug.LogError("[MenuPausa] pausaScript no asignado!");
        }
        
        // Usar fade y transición
        GameplayFadeManager.DoFadeToBlack(() =>
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToMainMenu();
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        });
    }

    public void ReiniciarPartida()
    {
        if (pausaScript != null)
        {
            pausaScript.TogglePause(false);
        }
        else
        {
            Debug.LogError("[MenuPausa] pausaScript no asignado!");
        }
        
        // Usar fade y transición
        GameplayFadeManager.DoFadeToBlack(() =>
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.RestartCurrentScene();
            }
            else
            {
                SceneManager.LoadScene("Level1_Blockout");
            }
        });
    }

    public void RegresarAJuego()
    {
        if (pausaScript != null)
        {
            pausaScript.TogglePause(false);
        }
        else
        {
            Debug.LogError("[MenuPausa] pausaScript no asignado!");
        }
    }
}
