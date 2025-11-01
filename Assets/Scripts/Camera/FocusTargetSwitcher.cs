using Unity.Cinemachine;
using UnityEngine;

/// Cambia el LookAt de la vcam entre dos targets según el estado de cobertura del jugador.
[DisallowMultipleComponent]
public class FocusTargetSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public CinemachineCamera vcam;
    public PlayerShooter player;

    [Header("Targets")]
    public Transform focusExposed;
    public Transform focusCover;

    private bool _lastCover;
    private bool _isOverridden = false;
    private Transform _overrideTarget;

    void Reset()
    {
        if (!vcam)
            vcam = GetComponent<CinemachineCamera>();
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go)
                player = go.GetComponent<PlayerShooter>();
        }
    }

    void Awake()
    {
        if (!vcam)
            vcam = GetComponent<CinemachineCamera>();
    }

    void OnEnable()
    {
        ApplyLookAt(initial: true);
    }

    void LateUpdate()
    {
        if (player == null || vcam == null || _isOverridden)
            return;

        if (player.isInCover != _lastCover)
        {
            ApplyLookAt(initial: false);
            _lastCover = player.isInCover;
        }
    }

    private void ApplyLookAt(bool initial)
    {
        if (vcam == null) return;

        Transform target = (player != null && player.isInCover && focusCover != null)
            ? focusCover 
            : focusExposed;

        if (target != null && vcam.LookAt != target)
        {
            vcam.LookAt = target;
            Debug.Log($"[FocusTargetSwitcher] Applied LookAt: {target.name} (Cover={player?.isInCover})");
        }
    }
    
    // Métodos públicos para control externo (ZoneFlowController)
    public void SetTargets(Transform newExposed, Transform newCover)
    {
        focusExposed = newExposed;
        focusCover = newCover;
        
        Debug.Log($"[FocusTargetSwitcher] Updated targets: Exposed={newExposed?.name}, Cover={newCover?.name}");
        
        // FORZAR actualización inmediata siempre (sin importar override)
        if (!_isOverridden && player != null)
        {
            // Actualizar inmediatamente según el estado actual del jugador
            Transform target = (player.isInCover && focusCover != null) ? focusCover : focusExposed;
            if (target != null && vcam != null)
            {
                vcam.LookAt = target;
                Debug.Log($"[FocusTargetSwitcher] FORCED LookAt: {target.name} (Cover={player.isInCover})");
            }
            _lastCover = player.isInCover; // Sincronizar estado
        }
    }
    
    public void SetOverrideTarget(Transform target)
    {
        _overrideTarget = target;
        _isOverridden = target != null;
        
        if (_isOverridden && vcam != null)
        {
            vcam.LookAt = _overrideTarget;
        }
        else if (!_isOverridden)
        {
            // Volver al comportamiento normal
            ApplyLookAt(initial: false);
        }
    }
    
    public void ClearOverride()
    {
        _isOverridden = false;
        _overrideTarget = null;
        
        Debug.Log($"[FocusTargetSwitcher] Override cleared, current state: Cover={player?.isInCover}");
        
        // FORZAR actualización inmediata con los targets actuales
        if (player != null && vcam != null)
        {
            Transform target = (player.isInCover && focusCover != null) ? focusCover : focusExposed;
            if (target != null)
            {
                vcam.LookAt = target;
                Debug.Log($"[FocusTargetSwitcher] FORCED LookAt after override: {target.name} (Cover={player.isInCover})");
            }
            _lastCover = player.isInCover; // Sincronizar estado
        }
    }
    
    // Método de debug para verificar estado actual
    public void LogCurrentState()
    {
        Debug.Log($"[FocusTargetSwitcher] State: Override={_isOverridden}, " +
                 $"Player Cover={player?.isInCover}, " +
                 $"Current LookAt={vcam?.LookAt?.name}, " +
                 $"Exposed={focusExposed?.name}, Cover={focusCover?.name}");
    }
}
