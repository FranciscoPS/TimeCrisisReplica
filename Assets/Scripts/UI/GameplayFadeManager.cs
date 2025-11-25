using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameplayFadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Panel negro para fade en gameplay")]
    public Image fadePanel;
    
    [Tooltip("Duración del fade")]
    public float fadeDuration = 1f;

    private static GameplayFadeManager _instance;
    public static GameplayFadeManager Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Fade in al iniciar la escena de gameplay
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            fadePanel.raycastTarget = false; // Desactivar raycast para no bloquear clicks
            fadePanel.DOFade(0f, fadeDuration);
        }
    }

    public void FadeToBlack(System.Action onComplete = null)
    {
        if (fadePanel != null)
        {
            // PAUSAR EL JUEGO INMEDIATAMENTE
            Time.timeScale = 0f;
            
            // Bloquear raycast para prevenir clicks durante transición
            fadePanel.raycastTarget = true;
            
            // Usar SetUpdate(true) para que funcione durante pausa
            fadePanel.DOFade(1f, fadeDuration)
                .SetUpdate(true) // Funciona con Time.timeScale = 0
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }
        else
        {
            Debug.LogWarning("[GameplayFadeManager] FadePanel no asignado");
            Time.timeScale = 0f; // Pausar de todas formas
            onComplete?.Invoke();
        }
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        if (fadePanel != null)
        {
            fadePanel.raycastTarget = false; // No bloquear clicks después del fade
            
            fadePanel.DOFade(0f, fadeDuration)
                .SetUpdate(true) // Funciona con Time.timeScale = 0
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    // Método estático para acceso fácil desde otros scripts
    public static void DoFadeToBlack(System.Action onComplete = null)
    {
        if (Instance != null)
        {
            Instance.FadeToBlack(onComplete);
        }
        else
        {
            Debug.LogWarning("[GameplayFadeManager] No hay instancia disponible");
            onComplete?.Invoke();
        }
    }
}