using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class ZoneFlowController : MonoBehaviour
{
    [Header("Zonas en orden")]
    public List<ZoneController> zones = new List<ZoneController>();

    [Header("Rig")]
    public CinemachineSplineCart cart;   // Cinemachine Spline Cart
    public CinemachineCamera vcam;       // CM Camera (hija del cart)
    public FocusTargetSwitcher focusTargetSwitcher; // componente de switcher

    [Header("Inicio del juego")]
    public float startPosition = 0f;     // Posición inicial en el spline (0 = inicio)
    public bool travelToFirstZone = true; // Si hacer viaje inicial a zona 1

    [Header("Transición")]
    public float travelDuration = 1.2f;                    // tiempo fijo del salto
    public AnimationCurve travelCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private int _current = -1;
    private bool _isTravelling;

    void Start()
    {
        // Asegura que el Cart esté en modo manual:
        if (cart != null)
        {
            // En CM3, el movimiento se controla via AutomaticDolly.Enabled
            cart.AutomaticDolly.Enabled = false;  // no queremos movimiento automático
            // Position Units: ponlas en Normalized en el Inspector
        }

        // Apaga todas las zonas
        foreach (var z in zones) if (z) z.Deactivate();
        
        // Posición inicial del cart
        SetCartNormalized(startPosition);
        
        // Iniciar el juego
        if (zones.Count > 0)
        {
            if (travelToFirstZone)
            {
                // Viajar desde la posición inicial hasta la zona 1
                StartCoroutine(TravelToZone(0));
            }
            else
            {
                // Aparecer directamente en zona 1
                JumpToZone(0, snap: true);
            }
        }
    }

    public void JumpToZone(int index, bool snap)
    {
        if (index < 0 || index >= zones.Count || cart == null) return;

        if (_current >= 0) zones[_current].Deactivate();

        _current = index;

        if (snap)
        {
            SetCartNormalized(zones[_current].enterT);
        }
        else
        {
            SetCartNormalized(zones[_current].enterT);
        }

        zones[_current].Activate();

        // Actualizar los targets del FocusTargetSwitcher con los de esta zona
        UpdateFocusTargets();
        
        // Modo gameplay: quitar override para que funcione el switcher normal
        if (focusTargetSwitcher)
        {
            focusTargetSwitcher.ClearOverride();
        }
    }

    public void OnZoneCleared(ZoneController cleared)
    {
        if (_isTravelling) return;

        int idx = zones.IndexOf(cleared);
        if (idx < 0) return;

        int next = idx + 1;
        if (next >= zones.Count)
        {
            Debug.Log("[ZoneFlow] ¡Todas las zonas completadas!");
            StartCoroutine(TransitionToLevelClear());
            return;
        }

        StartCoroutine(TravelToZone(next));
    }
    
    /// <summary>
    /// Transición a la escena Level Clear cuando se completan todas las zonas
    /// </summary>
    IEnumerator TransitionToLevelClear()
    {
        // Detener música de gameplay
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBackgroundMusic();
        }
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Esperar un momento (opcional, puedes ajustar o quitar)
        yield return new WaitForSecondsRealtime(1f);
        
        // Hacer fade y cambiar de escena
        GameplayFadeManager.DoFadeToBlack(() =>
        {
            // Restaurar timeScale
            Time.timeScale = 1f;
            
            // Cargar Level Clear
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene("Level Clear");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Level Clear");
            }
        });
    }

    IEnumerator TravelToZone(int nextIndex)
    {
        _isTravelling = true;
        
        // Notificar que estamos viajando
        GameEvents.TravellingBetweenZones?.Invoke(true);

        // Apaga zona actual (ya está limpia)
        if (_current >= 0) zones[_current].Deactivate();

        var nextZone = zones[nextIndex];

        // Durante el viaje: mirar al foco de la zona destino
        if (focusTargetSwitcher)
        {
            focusTargetSwitcher.SetOverrideTarget(nextZone.travelFocus);
        }
        else
        {
            SetLookAt(nextZone.travelFocus);
        }

        float fromT = GetCartNormalized();
        float toT   = nextZone.enterT;

        float t = 0f;
        while (t < travelDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / travelDuration);
            float eval = travelCurve.Evaluate(k);
            float cur = Mathf.Lerp(fromT, toT, eval);
            SetCartNormalized(cur);
            yield return null;
        }
        SetCartNormalized(toT);

        // Llegamos: activar nueva zona
        _current = nextIndex;
        nextZone.Activate();
        
        // PRIMERO: Actualizar los targets del FocusTargetSwitcher con los de esta zona
        UpdateFocusTargets();
        
        // SEGUNDO: Quitar override para que funcione el switcher normal con los nuevos targets
        if (focusTargetSwitcher)
        {
            focusTargetSwitcher.ClearOverride();
            // Debug para verificar estado final
            focusTargetSwitcher.LogCurrentState();
        }

        _isTravelling = false;
        
        // Notificar que ya no estamos viajando
        GameEvents.TravellingBetweenZones?.Invoke(false);
    }

    // ---- Helpers específicos del Spline Cart ----
    float GetCartNormalized()
    {
        // En CM3 Spline Cart, usar SplinePosition (respeta PositionUnits configurado en Inspector)
        return cart != null ? cart.SplinePosition : 0f;
    }

    void SetCartNormalized(float t01)
    {
        if (cart == null) return;
        cart.SplinePosition = Mathf.Clamp01(t01);  // colocamos el cart en ese punto del spline
        // AutomaticDolly.Enabled sigue en false para que NO se mueva "solo"
    }

    void SetLookAt(Transform target)
    {
        if (!vcam) return;
        vcam.LookAt = target;
    }
    
    void UpdateFocusTargets()
    {
        if (!focusTargetSwitcher || _current < 0 || _current >= zones.Count) return;
        
        var currentZone = zones[_current];
        if (currentZone != null)
        {

            
            // Actualizar los targets exposed/cover con los de la zona actual
            focusTargetSwitcher.SetTargets(currentZone.focusExposed, currentZone.focusCover);
        }
    }
}
