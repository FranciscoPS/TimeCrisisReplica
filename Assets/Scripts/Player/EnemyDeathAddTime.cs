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
        // NOTA: Este script es redundante ya que Health.Die() dispara EnemyKilled
        // Comentado para evitar doble invocación
        // GameEvents.EnemyKilled?.Invoke(false);
    }
}
