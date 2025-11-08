using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Panel negro para transiciones")]
    public Image fadePanel;
    
    [Tooltip("Duración del fade")]
    public float fadeDuration = 1f;
    
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "Level1_Blockout";

    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance => _instance;

    void Awake()
    {
        // Singleton pattern para acceso global
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Fade in al iniciar cualquier escena
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            fadePanel.DOFade(0f, fadeDuration);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (fadePanel != null)
        {
            // Fade out y cargar escena
            fadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                // Forzar limpieza antes del cambio de escena
                CleanupBeforeSceneChange();
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            // Fallback sin transición
            CleanupBeforeSceneChange();
            SceneManager.LoadScene(sceneName);
        }
    }

    private void CleanupBeforeSceneChange()
    {
        // Asegurar que todos los tweens se limpien
        DOTween.KillAll();
        
        // Log para debug
        Debug.Log("[SceneTransition] Cleanup completed before scene change");
    }

    public void TransitionToMainMenu()
    {
        TransitionToScene(mainMenuScene);
    }

    public void TransitionToGameplay()
    {
        TransitionToScene(gameplayScene);
    }

    public void RestartCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        TransitionToScene(currentScene);
    }

    // Método para otros scripts que necesiten hacer transiciones
    public static void LoadSceneWithTransition(string sceneName)
    {
        if (Instance != null)
        {
            Instance.TransitionToScene(sceneName);
        }
        else
        {
            // Fallback si no hay TransitionManager
            SceneManager.LoadScene(sceneName);
        }
    }
}
