using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Tooltip("Segundos iniciales de la partida")]
    public float startSeconds = 120f; // 2:00
    [Tooltip("Segundos a sumar por enemigo eliminado")]
    public float bonusPerKill = 20f;

    public bool running = true;
    private float _timeLeft;

    void OnEnable()
    {
        _timeLeft = startSeconds;
        GameEvents.TimerChanged?.Invoke(_timeLeft);
        GameEvents.EnemyKilled += OnEnemyKilled;
    }

    void OnDisable()
    {
        GameEvents.EnemyKilled -= OnEnemyKilled;
    }

    void Update()
    {
        if (!running) return;
        _timeLeft -= Time.deltaTime;
        if (_timeLeft < 0f) _timeLeft = 0f;
        GameEvents.TimerChanged?.Invoke(_timeLeft);

        if (_timeLeft <= 0f)
        {
            running = false;
            GameEvents.GameOver?.Invoke();
        }
    }

    void OnEnemyKilled()
    {
        if (!running) return;
        _timeLeft += bonusPerKill;
        GameEvents.TimerChanged?.Invoke(_timeLeft);
    }
}
