using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageOverlay : MonoBehaviour
{
    [Header("Damage Overlay")]
    [Tooltip("La imagen que se pondrá roja cuando recibas daño")]
    public Image damageOverlay;
    
    [Tooltip("Color del overlay al recibir daño")]
    public Color damageColor = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente
    
    [Tooltip("Duración del efecto de fade")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("Velocidad del parpadeo inicial")]
    public float flashSpeed = 10f;
    
    [Tooltip("Duración del parpadeo inicial")]
    public float flashDuration = 0.1f;

    private Coroutine _damageCoroutine;
    private Color _transparentColor;

    void Awake()
    {
        _transparentColor = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);
        
        if (!damageOverlay)
        {
            Debug.LogError("[DamageOverlay] No hay imagen asignada para el overlay!");
            return;
        }
        
        // Empezar transparente
        damageOverlay.color = _transparentColor;
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
        if (!damageOverlay) return;
        
        // Detener efecto anterior si existe
        if (_damageCoroutine != null)
        {
            StopCoroutine(_damageCoroutine);
        }
        
        _damageCoroutine = StartCoroutine(DamageEffectRoutine());
    }

    private IEnumerator DamageEffectRoutine()
    {
        // 1. Flash inicial rápido
        float flashTimer = 0f;
        while (flashTimer < flashDuration)
        {
            float flash = Mathf.Sin(flashTimer * flashSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
            damageOverlay.color = Color.Lerp(_transparentColor, damageColor, flash);
            flashTimer += Time.deltaTime;
            yield return null;
        }
        
        // 2. Fade out suave
        float fadeTimer = 0f;
        damageOverlay.color = damageColor;
        
        while (fadeTimer < fadeDuration)
        {
            float t = fadeTimer / fadeDuration;
            damageOverlay.color = Color.Lerp(damageColor, _transparentColor, t);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar que termine transparente
        damageOverlay.color = _transparentColor;
        _damageCoroutine = null;
    }
}