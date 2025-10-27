using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealthBridge : MonoBehaviour
{
    public int uiHealthSegments = 5; // cuántos íconos quieres en UI

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void Start()
    {
        // Ahora Health ya ejecutó su Awake(), _current está inicializado
        Debug.Log(
            "PlayerHealthBridge: maxHealth = "
                + _health.maxHealth
                + ", current = "
                + _health.Current
        );
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
