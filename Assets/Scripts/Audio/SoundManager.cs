using UnityEngine;

/// <summary>
/// Sistema centralizado de audio para el juego.
/// Agrega este script a un GameObject en la escena y arrastra los AudioClips en el Inspector.
/// Usa SoundManager.Instance.PlayX() desde cualquier script para reproducir sonidos.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Música de fondo")]
    [Tooltip("Música que se reproduce en loop durante el nivel")]
    public AudioClip backgroundMusic;
    
    [Header("SFX - Disparos")]
    [Tooltip("Sonido cuando el jugador dispara")]
    public AudioClip playerShootSFX;
    
    [Tooltip("Sonido cuando los enemigos disparan")]
    public AudioClip enemyShootSFX;
    
    [Header("SFX - Explosiones")]
    [Tooltip("Sonido de explosión de barriles")]
    public AudioClip explosionSFX;
    
    [Header("SFX - UI/Timer")]
    [Tooltip("Sonido que se reproduce cada segundo en la cuenta regresiva")]
    public AudioClip timerTickSFX;
    
    [Header("SFX - Arma")]
    [Tooltip("Sonido de recarga del arma")]
    public AudioClip reloadSFX;
    
    [Header("Configuración de Audio")]
    [Range(0f, 1f)]
    [Tooltip("Volumen de la música de fondo")]
    public float musicVolume = 0.5f;
    
    [Range(0f, 1f)]
    [Tooltip("Volumen de los efectos de sonido")]
    public float sfxVolume = 1f;

    // AudioSources internos
    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private float _originalMusicVolume;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Reproducir música de fondo automáticamente
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }

    /// <summary>
    /// Inicializa los AudioSources necesarios
    /// </summary>
    private void InitializeAudioSources()
    {
        // AudioSource para música
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = musicVolume;

        // AudioSource para SFX (one-shots)
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// Reproduce la música de fondo en loop
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (_musicSource != null && backgroundMusic != null)
        {
            _musicSource.clip = backgroundMusic;
            _musicSource.volume = musicVolume;
            _musicSource.loop = true; // Asegurar que esté en loop
            _originalMusicVolume = musicVolume;
            _musicSource.Play();
        }
    }

    /// <summary>
    /// Detiene la música de fondo
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    /// <summary>
    /// Reproduce el sonido de disparo del jugador
    /// </summary>
    public void PlayPlayerShoot()
    {
        PlaySFX(playerShootSFX);
    }

    /// <summary>
    /// Reproduce el sonido de disparo de enemigos
    /// </summary>
    public void PlayEnemyShoot()
    {
        PlaySFX(enemyShootSFX);
    }

    /// <summary>
    /// Reproduce el sonido de explosión
    /// </summary>
    public void PlayExplosion()
    {
        PlaySFX(explosionSFX);
    }

    /// <summary>
    /// Reproduce el sonido de tick del timer
    /// </summary>
    public void PlayTimerTick()
    {
        PlaySFX(timerTickSFX);
    }

    /// <summary>
    /// Reproduce el sonido de recarga del arma
    /// </summary>
    public void PlayReload()
    {
        PlaySFX(reloadSFX);
    }

    /// <summary>
    /// Atenúa la música de fondo (para menú de pausa)
    /// </summary>
    public void DuckMusic(float duckAmount = 0.3f)
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _originalMusicVolume * duckAmount;
        }
    }

    /// <summary>
    /// Restaura el volumen original de la música
    /// </summary>
    public void RestoreMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _originalMusicVolume;
        }
    }

    /// <summary>
    /// Reproduce un efecto de sonido genérico
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (_sfxSource != null && clip != null)
        {
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Cambia el volumen de la música
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (_musicSource != null)
        {
            _musicSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Cambia el volumen de los SFX
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (_sfxSource != null)
        {
            _sfxSource.volume = sfxVolume;
        }
    }
}
