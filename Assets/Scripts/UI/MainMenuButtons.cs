using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Fade Transition")]
    [Tooltip("Panel negro que se usará para el fade")]
    public Image fadePanel;
    
    [Tooltip("Duración del fade en segundos")]
    public float fadeDuration = 1f;
    
    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena del gameplay")]
    public string gameplayScene = "cutscene";

    void Start()
    {
        // Inicializar el fade panel completamente negro
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            // Fade in al menú al iniciar
            fadePanel.DOFade(0f, fadeDuration);
        }
        else
        {

        }
    }

    public void IniciarPartida()
    {
        // Verificar que el fadePanel esté asignado
        if (fadePanel != null)
        {
            // Desactivar botones para evitar clicks múltiples
            SetButtonsInteractable(false);
            
            // Fade out y cargar escena
            fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(gameplayScene);
            });
        }
        else
        {
            // Fallback sin transición
            SceneManager.LoadScene(gameplayScene);
        }
    }

    public void ExitGame()
    {
        if (fadePanel != null)
        {
            // Desactivar botones
            SetButtonsInteractable(false);
            
            // Fade out y salir
            fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                QuitApplication();
            });
        }
        else
        {
            // Fallback sin transición
            QuitApplication();
        }
    }

    private void QuitApplication()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetButtonsInteractable(bool interactable)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.interactable = interactable;
        }
    }
}
