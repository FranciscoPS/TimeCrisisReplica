using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    [Header("Timer")]
    public TMP_Text timerText;              // arrastra un TMP_Text (mm:ss)
    public TMP_Text timeAddedText;          // texto para mostrar "+20s" cuando se agrega tiempo
    
    private float _lastTimerValue;

    [Header("Ammo")]
    public Transform ammoContainer;         // un GameObject con HorizontalLayoutGroup
    public Image ammoIconPrefab;            // prefab de sprite de bala
    private List<Image> _ammoIcons = new();

    [Header("Health")]
    public Transform healthContainer;       // otro HLG para vida
    public Image healthIconPrefab;          // prefab sprite de vida (corazón/escudo)
    private List<Image> _healthIcons = new();
    private int _healthSegments = 0;

    [Header("Alerts")]
    public TMP_Text reloadText;             // "RELOAD!" cuando sin balas
    public TMP_Text reloadingText;          // "Reloading..." durante recarga

    [Header("GameOver")]
    public CanvasGroup GameOverGroup;
    private bool gameOver;
    private const float TWEEN_TIME = 0.3f;
    private Tween overTween;

    void Start()
    {
        // Inicializar textos ocultos
        if (reloadText) reloadText.gameObject.SetActive(false);
        if (reloadingText) reloadingText.gameObject.SetActive(false);
        if (timeAddedText) timeAddedText.gameObject.SetActive(false);
        // GameOver
        gameOver = false;
        GameOverGroup.alpha = 0f;
        GameOverGroup.interactable = false;
        GameOverGroup.blocksRaycasts = false;
    }

    void OnEnable()
    {
        GameEvents.TimerChanged     += OnTimerChanged;
        GameEvents.AmmoChanged      += OnAmmoChanged;
        GameEvents.PlayerHealthChanged += OnHealthChanged;
        GameEvents.ReloadAlert      += OnReloadAlert;
        GameEvents.ReloadingStatus  += OnReloadingStatus;
        GameEvents.GameOver         += OnGameOver;
    }
    void OnDisable()
    {
        GameEvents.TimerChanged     -= OnTimerChanged;
        GameEvents.AmmoChanged      -= OnAmmoChanged;
        GameEvents.PlayerHealthChanged -= OnHealthChanged;
        GameEvents.ReloadAlert      -= OnReloadAlert;
        GameEvents.ReloadingStatus  -= OnReloadingStatus;
        GameEvents.GameOver         -= OnGameOver;
    }

    // --- Timer ---
    void OnTimerChanged(float seconds)
    {
        // Detectar si se agregó tiempo (valor aumenta en vez de disminuir)
        if (_lastTimerValue > 0 && seconds > _lastTimerValue)
        {
            float timeAdded = seconds - _lastTimerValue;
            ShowTimeAddedFeedback(timeAdded);
        }
        
        _lastTimerValue = seconds;
        
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        if (timerText) timerText.text = $"{m:00}:{s:00}";
    }
    
    void ShowTimeAddedFeedback(float timeAdded)
    {
        if (!timeAddedText) return;
        
        // Configurar el texto
        timeAddedText.text = $"+{Mathf.RoundToInt(timeAdded)}s";
        timeAddedText.color = new Color(0f, 1f, 0f, 1f); // Verde brillante
        timeAddedText.gameObject.SetActive(true);
        
        // Animación: Escala + Fade out + Mover hacia arriba
        timeAddedText.transform.localScale = Vector3.zero;
        timeAddedText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
        
        // Fade out después de un momento
        DOVirtual.DelayedCall(1.5f, () =>
        {
            if (timeAddedText)
            {
                timeAddedText.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    if (timeAddedText) timeAddedText.gameObject.SetActive(false);
                });
            }
        });
        
        // Animar el timer principal con un "bounce"
        if (timerText)
        {
            timerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5, 0.5f);
        }
    }

    // --- Ammo ---
    void EnsureAmmoIcons(int max)
    {
        while (_ammoIcons.Count < max)
        {
            var icon = Instantiate(ammoIconPrefab, ammoContainer);
            _ammoIcons.Add(icon);
        }
        for (int i = 0; i < _ammoIcons.Count; i++)
            _ammoIcons[i].gameObject.SetActive(i < max);
    }
    void OnAmmoChanged(int current, int max)
    {
        EnsureAmmoIcons(max);
        for (int i = 0; i < max; i++)
            _ammoIcons[i].color = (i < current) ? Color.white : new Color(1,1,1,0.2f);
    }

    // --- Health (segmentos) ---
    // Mapea el float a "segmentos" visibles (ej. 5 corazones)
    void EnsureHealthIcons(int segments)
    {
        while (_healthIcons.Count < segments)
        {
            var icon = Instantiate(healthIconPrefab, healthContainer);
            _healthIcons.Add(icon);
        }
        for (int i = 0; i < _healthIcons.Count; i++)
            _healthIcons[i].gameObject.SetActive(i < segments);
    }
    void OnHealthChanged(float current, float max)
    {
        // Usar directamente la vida actual como número de vidas
        int maxLives = Mathf.RoundToInt(max);
        int currentLives = Mathf.RoundToInt(current);
        
        if (_healthSegments == 0) _healthSegments = maxLives;
        
        EnsureHealthIcons(maxLives);



        for (int i = 0; i < maxLives; i++)
            _healthIcons[i].color = (i < currentLives) ? Color.white : new Color(1,1,1,0.2f);
    }

    // Puedes establecerlo desde el Player al iniciar si quieres 3/5/10 segmentos exactos
    public void SetHealthSegments(int segments)
    {
        _healthSegments = segments;
        EnsureHealthIcons(segments);
    }

    // --- Alerts ---
    void OnReloadAlert(bool show)
    {
        if (reloadText) reloadText.gameObject.SetActive(show);
    }

    void OnReloadingStatus(bool show)
    {
        if (reloadingText) reloadingText.gameObject.SetActive(show);
    }

    void OnGameOver()
    {
        Time.timeScale = 0;

        GameOverGroup.alpha = 1f;

        overTween?.Kill();
        GameOverGroup.interactable = true;
        GameOverGroup.blocksRaycasts = true;

        overTween = GameOverGroup.DOFade(1f, TWEEN_TIME).SetUpdate(true);
        gameOver = true;
    }
}
