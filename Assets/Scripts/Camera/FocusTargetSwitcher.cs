using UnityEngine;
using Unity.Cinemachine; // CM3

/// Cambia el LookAt de la vcam entre dos targets según el estado de cobertura del jugador.
/// Pon este componente en tu *Cinemachine Camera* (la Dolly Camera).
[DisallowMultipleComponent]
public class FocusTargetSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public CinemachineCamera vcam;     // arrastra tu CM Dolly Camera (este mismo GO)
    public PlayerShooter player;       // arrastra tu PlayerRig (con PlayerShooter)

    [Header("Targets")]
    public Transform focusExposed;     // a dónde mirar normalmente (expuesto)
    public Transform focusCover;       // a dónde mirar al cubrirse (opcional)

    [Header("Debug")]
    public bool logSwitches = true;

    private bool _lastCover;

    void Reset()
    {
        if (!vcam) vcam = GetComponent<CinemachineCamera>();
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.GetComponent<PlayerShooter>();
        }
    }

    void Awake()
    {
        if (!vcam) vcam = GetComponent<CinemachineCamera>();
    }

    void OnEnable()
    {
        ApplyLookAt(initial:true);
    }

    void LateUpdate()
    {
        if (player == null || vcam == null) return;

        if (player.isInCover != _lastCover)
        {
            ApplyLookAt(initial:false);
            _lastCover = player.isInCover;
        }
    }

    private void ApplyLookAt(bool initial)
    {
        if (vcam == null) return;

        // Si hay foco de cobertura y estamos cubiertos, úsalo; si no, usa el expuesto.
        Transform target = (player != null && player.isInCover && focusCover != null)
            ? focusCover
            : focusExposed;

        if (target != null && vcam.LookAt != target)
        {
            vcam.LookAt = target;
            if (logSwitches)
                Debug.Log($"[FocusTargetSwitcher] {(initial ? "Init" : "Switch")} LookAt -> {target.name} (cover={player?.isInCover})");
        }
    }
}
