using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3 (Unity 6)

/// Baja/sube la vista en cobertura modificando CinemachineCameraOffset.Offset.y.
/// Diseñado para convivir con LookAt/FocusTargetSwitcher sin generar jitter.
/// En CM3, el espacio de coordenadas se maneja automáticamente por el componente.
[DisallowMultipleComponent]
public class CinemachineCoverOffsetDriver : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Jugador con PlayerShooter (expone isInCover).")]
    public PlayerShooter player;
    [Tooltip("Extensión CinemachineCameraOffset en esta vcam.")]
    public CinemachineCameraOffset camOffset;

    [Header("Offsets (metros)")]
    [Tooltip("Altura normal (sin cobertura).")]
    public float exposedY = 0f;
    [Tooltip("Altura al cubrirse (negativo = baja).")]
    public float coverY = -1.0f;

    [Header("Transición")]
    [Tooltip("Velocidad del lerp (8–12 se siente arcade).")]
    public float lerpSpeed = 10f;

    void Reset()
    {
        if (!camOffset) camOffset = GetComponent<CinemachineCameraOffset>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.GetComponent<PlayerShooter>();
        }
    }

    void Awake()
    {
        if (!camOffset) camOffset = GetComponent<CinemachineCameraOffset>();
        // Note: CoordinateSpace property doesn't exist in Cinemachine 3
        // The coordinate space is handled automatically by the component
        // In the Inspector, make sure "Apply After" is set appropriately
    }

    void LateUpdate()
    {
        if (!player || !camOffset) return;

        float targetY = player.isInCover ? coverY : exposedY;

        // Lerp exponencial suave: sin pops, estable con FocusTarget activo
        var off = camOffset.Offset;
        off.y = Mathf.Lerp(off.y, targetY, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));
        camOffset.Offset = off;
    }
}
