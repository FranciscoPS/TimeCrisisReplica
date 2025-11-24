using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja los botones del menú de Game Over
/// </summary>
public class GameOverMenu : MonoBehaviour
{
    private bool _isTransitioning = false;

    public void RestartGame()
    {
        if (_isTransitioning)
            return;
            
        _isTransitioning = true;
        
        // Detener música del juego
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBackgroundMusic();
        }
        
        // Usar fade y transición
        GameplayFadeManager.DoFadeToBlack(() =>
        {
            // Restaurar timeScale antes de cambiar de escena
            Time.timeScale = 1f;
            
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.RestartCurrentScene();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        });
    }

    public void ReturnToMainMenu()
    {
        if (_isTransitioning)
            return;
            
        _isTransitioning = true;
        
        // Detener música del juego
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBackgroundMusic();
        }
        
        // Usar fade y transición
        GameplayFadeManager.DoFadeToBlack(() =>
        {
            // Restaurar timeScale antes de cambiar de escena
            Time.timeScale = 1f;
            
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
}
