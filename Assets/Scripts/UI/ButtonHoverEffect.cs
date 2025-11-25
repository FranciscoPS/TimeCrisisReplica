using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Agrega efectos visuales y de sonido a los botones.
/// - Hover: aumenta escala y cambia opacidad
/// - Click: reproduce sonido de disparo del jugador
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Efectos Visuales")]
    [Tooltip("Escala cuando se hace hover")]
    public float hoverScale = 1.1f;
    
    [Tooltip("Duración de la animación de hover")]
    public float hoverDuration = 0.2f;
    
    [Tooltip("Opacidad cuando se hace hover (si hay CanvasGroup)")]
    [Range(0.5f, 1f)]
    public float hoverOpacity = 0.8f;
    
    [Header("Sonido")]
    [Tooltip("Reproducir sonido al hacer click")]
    public bool playClickSound = true;

    private Button _button;
    private Vector3 _originalScale;
    private CanvasGroup _canvasGroup;
    private float _originalOpacity = 1f;
    private Tween _hoverTween;
    private Tween _opacityTween;

    void Awake()
    {
        _button = GetComponent<Button>();
        _originalScale = transform.localScale;
        _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_canvasGroup != null)
        {
            _originalOpacity = _canvasGroup.alpha;
        }
        
        // Agregar listener para el sonido de click
        if (playClickSound)
        {
            _button.onClick.AddListener(PlayClickSound);
        }
    }

    void OnDestroy()
    {
        _hoverTween?.Kill();
        _opacityTween?.Kill();
        
        if (playClickSound && _button != null)
        {
            _button.onClick.RemoveListener(PlayClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable)
            return;

        // Animación de escala
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(_originalScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Funciona durante pausa
        
        // Animación de opacidad
        if (_canvasGroup != null)
        {
            _opacityTween?.Kill();
            _opacityTween = _canvasGroup.DOFade(hoverOpacity, hoverDuration)
                .SetUpdate(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable)
            return;

        // Restaurar escala original
        _hoverTween?.Kill();
        _hoverTween = transform.DOScale(_originalScale, hoverDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
        
        // Restaurar opacidad original
        if (_canvasGroup != null)
        {
            _opacityTween?.Kill();
            _opacityTween = _canvasGroup.DOFade(_originalOpacity, hoverDuration)
                .SetUpdate(true);
        }
    }

    private void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerShoot();
        }
    }
}
