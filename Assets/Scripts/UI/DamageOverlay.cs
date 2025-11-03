using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DamageOverlay : MonoBehaviour
{
    [Header("Damage Overlay")]
    [Tooltip("El postproccesing")]
    public Volume volume;
    
    [Tooltip("La vignette que se pondrá roja cuando recibas daño")]
    public float flashIntensity = 0.60f; // Rojo semi-transparente
    
    [Tooltip("Duración del efecto de fade")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("Velocidad del parpadeo inicial")]
    public float flashSpeed = 10f;
    
    [Tooltip("Duración del parpadeo inicial")]
    public float flashDuration = 0.1f;

    private Vignette _vignette;
    private Coroutine _damageCoroutine;

    void Awake()
    {
        if (!volume)
            volume = GetComponent<Volume>();

        if (!volume || !volume.profile.TryGet(out _vignette))
        {
            Debug.LogError("[DamageVignette] Volume o Vignette no encontrado!");
            enabled = false;
            return;
        }

        _vignette.intensity.value = 0f;
    }

    void OnEnable()
    {
        // Suscribirse al evento de daño del jugador
        GameEvents.PlayerHealthChanged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        GameEvents.PlayerHealthChanged -= OnPlayerDamaged;
    }

    private float _lastHealth = -1f;
    
    private void OnPlayerDamaged(float currentHealth, float maxHealth)
    {
        // Solo activar efecto si la vida DISMINUYÓ
        if (_lastHealth > 0f && currentHealth < _lastHealth)
        {
            ShowDamageEffect();
        }
        _lastHealth = currentHealth;
    }

    public void ShowDamageEffect()
    {
        if (!volume || !_vignette) return;

        // Stop previous effect if running
        if (_damageCoroutine != null)
            StopCoroutine(_damageCoroutine);

        _damageCoroutine = StartCoroutine(DamageEffectRoutine());
    }

    private IEnumerator DamageEffectRoutine()
    {
        // 1. Flash pulse
        float flashTimer = 0f;
        while (flashTimer < flashDuration)
        {
            float flash = Mathf.Sin(flashTimer * flashSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
            _vignette.intensity.value = Mathf.Lerp(0f, flashIntensity, flash);
            flashTimer += Time.deltaTime;
            yield return null;
        }

        // 2. Smooth fade out
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            float t = fadeTimer / fadeDuration;
            _vignette.intensity.value = Mathf.Lerp(flashIntensity, 0f, t);
            fadeTimer += Time.deltaTime;
            yield return null;
        }

        _vignette.intensity.value = 0f;
    }
}