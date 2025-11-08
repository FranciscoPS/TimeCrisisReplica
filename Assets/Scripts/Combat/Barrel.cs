using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem Explosion;
    public GameObject Bar;

    [Header("Explosion Damage")]
    [Tooltip("Radio de la explosión en metros")]
    public float explosionRadius = 5f;
    
    [Tooltip("Daño que hace la explosión a los enemigos")]
    public float explosionDamage = 100f;
    
    [Tooltip("Layers que pueden recibir daño de la explosión")]
    public LayerMask damageableLayers = -1; // Por defecto todos los layers
    
    [Header("Debug")]
    public bool showExplosionGizmo = true;

    public void DestroyBarrel()
    {
        // Hacer daño en área ANTES de la explosión visual
        ExplodeAndDamage();
        
        // Efectos visuales
        if (Explosion) Explosion.Play();
        
        // Destruir el barril físico
        if (Bar) Destroy(Bar);
        
        // Destruir este script component después de un delay (para que termine la explosión)
        Destroy(this, 3f);
    }
    
    private void ExplodeAndDamage()
    {
        Vector3 explosionCenter = transform.position;
        
        // Buscar todos los colliders en el radio de explosión
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, explosionRadius, damageableLayers);
        
        Debug.Log($"[Barrel] Explosión! Radio: {explosionRadius}m, Objetivos detectados: {hitColliders.Length}");
        
        foreach (Collider hitCollider in hitColliders)
        {
            // Verificar que el GameObject esté activo y tenga componente IDamageable
            if (hitCollider.gameObject.activeInHierarchy && 
                hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                // Calcular distancia para damage falloff (opcional)
                float distance = Vector3.Distance(explosionCenter, hitCollider.transform.position);
                float damageMultiplier = Mathf.Clamp01(1f - (distance / explosionRadius));
                float finalDamage = explosionDamage * damageMultiplier;
                
                // Aplicar daño
                Vector3 hitPoint = hitCollider.ClosestPoint(explosionCenter);
                Vector3 hitNormal = (hitCollider.transform.position - explosionCenter).normalized;
                
                damageable.TakeDamage(finalDamage, hitPoint, hitNormal);
                
                Debug.Log($"[Barrel] Daño explosión: {hitCollider.name} recibió {finalDamage:F1} daño (distancia: {distance:F1}m)");
            }
        }
    }
    
    // Gizmo para visualizar el radio de explosión en el editor
    private void OnDrawGizmosSelected()
    {
        if (showExplosionGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}
