using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathAddTime : MonoBehaviour
{
    private Health _h;

    void Awake()
    {
        _h = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (_h != null)
            _h.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (_h != null)
            _h.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        GameEvents.EnemyKilled?.Invoke(); // GameTimer sumará +20s
    }
}
