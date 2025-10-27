using System;
using UnityEngine;
using UnityEngine.Animations;

[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public bool invulnerable = false;

    [Header("Muerte")]
    [Tooltip("Qué objeto destruir al morir (si es null, se destruye este mismo GO).")]
    public GameObject rootToDestroy;

    [Tooltip("Retraso (s) antes de destruir rootToDestroy.")]
    public float deathDestroyDelay = 3f;

    [Tooltip("Scripts a deshabilitar al morir (opcional).")]
    public MonoBehaviour[] toDisableOnDeath; // Ej: EnemyPopper

    public Animator animator;

    public Action OnDeath;
    public Action<float> OnDamage;
    public float Current => _current;

    float _current;

    void Awake()
    {
        _current = maxHealth;
        if (!rootToDestroy)
            rootToDestroy = gameObject; // fallback
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (invulnerable || _current <= 0f)
            return;

        _current -= amount;
        OnDamage?.Invoke(amount);

        if (_current <= 0f)
        {
            _current = 0f;
            OnDeath?.Invoke();
            Die(); // ← importante
        }
    }

    private void Die()
    {
        animator.SetTrigger("DEATH");
        // 1) deshabilitar lógica (opcional)
        if (toDisableOnDeath != null)
        {
            foreach (var mb in toDisableOnDeath)
                if (mb)
                    mb.enabled = false;
        }

        // 2) quitar colliders y renderers para que “desaparezca” ya
        if (rootToDestroy)
        {
            foreach (var col in rootToDestroy.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            foreach (var r in rootToDestroy.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

            // 3) destruir todo el árbol (EnemyParent) tras el delay
            Destroy(rootToDestroy, deathDestroyDelay);
        }
        else
        {
            Destroy(gameObject, deathDestroyDelay);
        }
    }
}
