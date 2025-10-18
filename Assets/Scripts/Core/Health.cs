using UnityEngine;
using System;

[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public bool invulnerable = false;
    public Action OnDeath;
    public Action<float> OnDamage;

    float _current;

    void Awake() => _current = maxHealth;

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (invulnerable || _current <= 0f) return;
        _current -= amount;
        OnDamage?.Invoke(amount);
        if (_current <= 0f)
        {
            _current = 0f;
            OnDeath?.Invoke();
        }
    }
}
