using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    [Header("Timer")]
    public TMP_Text timerText;              // arrastra un TMP_Text (mm:ss)

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
    public TMP_Text reloadText;             // “RELOAD!” (ocúltalo por defecto)

    void OnEnable()
    {
        GameEvents.TimerChanged     += OnTimerChanged;
        GameEvents.AmmoChanged      += OnAmmoChanged;
        GameEvents.PlayerHealthChanged += OnHealthChanged;
        GameEvents.ReloadAlert      += OnReloadAlert;
        GameEvents.GameOver         += OnGameOver;
    }
    void OnDisable()
    {
        GameEvents.TimerChanged     -= OnTimerChanged;
        GameEvents.AmmoChanged      -= OnAmmoChanged;
        GameEvents.PlayerHealthChanged -= OnHealthChanged;
        GameEvents.ReloadAlert      -= OnReloadAlert;
        GameEvents.GameOver         -= OnGameOver;
    }

    // --- Timer ---
    void OnTimerChanged(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        if (timerText) timerText.text = $"{m:00}:{s:00}";
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
        int segments = _healthSegments > 0 ? _healthSegments : 5; // por defecto 5
        if (_healthSegments == 0) _healthSegments = segments;

        EnsureHealthIcons(segments);

        float per = Mathf.Approximately(max, 0f) ? 0f : current / max;
        int active = Mathf.RoundToInt(per * segments);
        active = Mathf.Clamp(active, 0, segments);

        for (int i = 0; i < segments; i++)
            _healthIcons[i].color = (i < active) ? Color.white : new Color(1,1,1,0.2f);
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

    void OnGameOver()
    {
        if (reloadText) reloadText.text = "GAME OVER";
        if (reloadText) reloadText.gameObject.SetActive(true);
        // Aquí podrías pausar, etc.
    }
}
