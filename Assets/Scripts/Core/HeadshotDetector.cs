using UnityEngine;

/// <summary>
/// Coloca este componente en el Collider de la cabeza del enemigo.
/// Cuando el jugador dispare a este collider, se detectará como headshot.
/// </summary>
public class HeadshotDetector : MonoBehaviour
{
    [Tooltip("Referencia al componente Health del enemigo (usualmente en el padre)")]
    public Health parentHealth;

    void Awake()
    {
        // Si no se asignó manualmente, intentar encontrarlo en el padre
        if (parentHealth == null)
        {
            parentHealth = GetComponentInParent<Health>();
            if (parentHealth == null)
            {
                Debug.LogWarning($"[HeadshotDetector] No se encontró Health en el padre de {gameObject.name}");
            }
        }
    }
}
