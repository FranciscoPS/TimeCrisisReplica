using UnityEngine;

/// <summary>
/// Gestor de audio simple para el menú principal.
/// Solo reproduce música de fondo en loop.
/// </summary>
public class MenuAudioManager : MonoBehaviour
{
    [Header("Música del Menú")]
    [Tooltip("Música que se reproduce en loop en el menú")]
    public AudioClip menuMusic;
    
    [Range(0f, 1f)]
    [Tooltip("Volumen de la música del menú")]
    public float volume = 0.5f;
    
    [Header("SFX del Menú (Opcional)")]
    [Tooltip("Sonido cuando haces hover sobre un botón")]
    public AudioClip buttonHoverSFX;
    
    [Tooltip("Sonido cuando haces click en un botón (usa el mismo que el disparo del jugador)")]
    public AudioClip buttonClickSFX;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    void Awake()
    {
        // IMPORTANTE: Este manager NO debe persistir entre escenas
        // Se destruirá automáticamente al cambiar de escena
        
        // Crear AudioSource para música
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = volume;
        _musicSource.clip = menuMusic;
        
        // Crear AudioSource para SFX
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = 1f;
    }
    
    void OnDestroy()
    {
        // Limpiar cuando se destruya el objeto
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    void Start()
    {
        // Reproducir música si hay clip asignado
        if (menuMusic != null)
        {
            _musicSource.Play();
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de hover sobre un botón
    /// </summary>
    public void PlayButtonHover()
    {
        if (buttonHoverSFX != null && _sfxSource != null)
        {
            _sfxSource.PlayOneShot(buttonHoverSFX);
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de click en un botón (usa el sonido de disparo del jugador)
    /// </summary>
    public void PlayButtonClick()
    {
        // Intentar usar el SoundManager si existe (para usar el sonido de disparo)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerShoot();
        }
        // Si no existe SoundManager, usar el clip local si está asignado
        else if (buttonClickSFX != null && _sfxSource != null)
        {
            _sfxSource.PlayOneShot(buttonClickSFX);
        }
    }

    /// <summary>
    /// Cambiar el volumen de la música
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_musicSource != null)
        {
            _musicSource.volume = volume;
        }
    }

    /// <summary>
    /// Detener la música
    /// </summary>
    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }
}
