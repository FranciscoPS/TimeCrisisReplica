using System.Collections.Generic;
using UnityEngine;

public class ZoneController : MonoBehaviour
{
    [Header("Identidad")]
    public string zoneName = "Zone 1";

    [Header("Entrada sobre el Spline (Normalized 0..1)")]
    public float enterT = 0f;

    [Header("Foco solo durante el viaje hacia esta zona")]
    public Transform travelFocus;

    [Header("Focos para gameplay (exposed/cover)")]
    public Transform focusExposed;  // Punto normal de mira
    public Transform focusCover;    // Punto de mira al cubrirse

    [Header("Enemigos (raíz de cada prefab)")]
    public List<GameObject> enemies = new List<GameObject>();

    public bool IsActive { get; private set; }
    private int _alive;

    public void Activate()
    {
        IsActive = true;
        // NO desactivar el gameObject principal para que los focus points sigan disponibles
        // gameObject.SetActive(true); // Los focus points siempre deben estar activos

        _alive = 0;
        foreach (var e in enemies)
        {
            if (!e) continue;
            e.SetActive(true);

            var h = e.GetComponentInChildren<Health>(true);
            if (h != null)
            {
                _alive++;
                h.OnDeath += HandleEnemyDeath;
            }
        }
        Debug.Log($"[Zone] {zoneName} activada - Enemigos: {_alive}");
    }

    public void Deactivate()
    {
        IsActive = false;
        foreach (var e in enemies)
        {
            if (!e) continue;
            var h = e.GetComponentInChildren<Health>(true);
            if (h != null) h.OnDeath -= HandleEnemyDeath;
            e.SetActive(false);
        }
        // NO desactivar el gameObject principal para que los focus points sigan disponibles
        // gameObject.SetActive(false); // Los focus points siempre deben estar activos
    }

    void HandleEnemyDeath()
    {
        _alive = Mathf.Max(0, _alive - 1);
        if (_alive == 0)
        {
            Debug.Log($"[Zone] {zoneName} COMPLETADA");
            var flow = FindAnyObjectByType<ZoneFlowController>();
            flow?.OnZoneCleared(this);
        }
    }
}
