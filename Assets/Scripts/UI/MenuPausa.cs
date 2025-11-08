using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public Pausa pausaScript;
    
    public void RegresarAMenu()
    {
        Debug.Log("[MenuPausa] RegresarAMenu clicked!");
        
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
        Debug.Log("[MenuPausa] ReiniciarPartida clicked!");
        
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
        Debug.Log("[MenuPausa] RegresarAJuego clicked!");
        
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
