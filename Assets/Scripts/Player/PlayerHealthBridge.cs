using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealthBridge : MonoBehaviour
{
    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void Start()
    {
        // Ahora Health ya ejecutó su Awake(), _current está inicializado
        Debug.Log($"[VIDA] Jugador iniciado: {_health.Current}/{_health.maxHealth} HP");
        
        // Configurar automáticamente las vidas en la UI
        var gameUI = FindFirstObjectByType<GameUI>();
        if (gameUI)
        {
            int lives = Mathf.RoundToInt(_health.maxHealth);
            gameUI.SetHealthSegments(lives);
        }
        
        GameEvents.PlayerHealthChanged?.Invoke(_health.Current, _health.maxHealth);
    }

    void OnEnable()
    {
        _health.OnDamage += OnDamaged;
        _health.OnDeath += OnDead;
    }

    void OnDisable()
    {
        _health.OnDamage -= OnDamaged;
        _health.OnDeath -= OnDead;
    }

    void OnDamaged(float amount)
    {
        GameEvents.PlayerHealthChanged?.Invoke(_health.Current, _health.maxHealth);
    }

    void OnDead()
    {
        GameEvents.PlayerHealthChanged?.Invoke(0f, _health.maxHealth);
        GameEvents.GameOver?.Invoke(); // esto terminará el juego (PlayerShooter bloqueará input)
    }
}
