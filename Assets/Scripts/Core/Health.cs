using System;
using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

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
        // Disparar animación de muerte solo si hay animator asignado
        if (animator)
        {
            animator.SetTrigger("DEATH");
            // Se espera a que la animacion continue
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            // En caso de que no hubiera animacion, se salta directamente
            OnDeathAnimationEnd();
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // se espera un rato para que la animacion inicie
        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float waitTime = state.length;
        yield return new WaitForSeconds(waitTime);
        OnDeathAnimationEnd();
    }

    // Se apagan los colliders, renders y se desabilitan al terminar la animacion
    private void OnDeathAnimationEnd()
    {
        if (toDisableOnDeath != null)
        {
            foreach (var mb in toDisableOnDeath)
                if (mb) mb.enabled = false;
        }

        if (rootToDestroy)
        {
            foreach (var col in rootToDestroy.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            foreach (var r in rootToDestroy.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

            Destroy(rootToDestroy, deathDestroyDelay);
        }
        else
        {
            Destroy(gameObject, deathDestroyDelay);
        }
    }
}
