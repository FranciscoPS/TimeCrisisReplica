using UnityEngine;
using UnityEngine.InputSystem; // Mouse.current y InputAction
// Asegúrate de haber generado la clase InputSystem_Actions desde tu .inputactions

[DisallowMultipleComponent]
public class PlayerShooter : MonoBehaviour
{
    [Header("Refs")]
    public Camera mainCamera;              // Déjalo vacío y tomará Camera.main
    public LayerMask raycastMask;          // Marca Environment (y luego Enemy)

    [Header("Gun")]
    public int magazineSize = 12;
    public float fireRate = 6f;            // balas/seg
    public float damagePerShot = 34f;
    public float maxRange = 150f;
    public float reloadTime = 1.0f;

    [Header("Cover")]
    public bool isInCover = false;         // True mientras mantienes la acción Cover
    public float coverDebounce = 0.05f;    // Evita rebotes de input

    [Header("Debug/Tracer In-Game")]
    public bool drawTracerInGame = true;   // Línea visible en GAME view
    public float tracerDuration = 0.06f;
    public float tracerWidth = 0.02f;
    public Color tracerColorHit = Color.red;
    public Color tracerColorMiss = Color.yellow;

    // ─────────────────────────────────────────────────────────────────────────────

    private InputSystem_Actions _inputs;   // Clase generada desde tu .inputactions
    private InputAction _actFire;          // Resueltas dinámicamente por nombre
    private InputAction _actCover;
    private InputAction _actReload;        // opcional

    private int _currentAmmo;
    private float _nextShootTime;
    private bool _isReloading;
    private float _lastCoverToggle;
    private Health _health;

    void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;
        _health = GetComponent<Health>();
        _currentAmmo = magazineSize;

        _inputs = new InputSystem_Actions(); // requiere que hayas hecho Generate C# Class
        Debug.Log("[PlayerShooter] InputSystem_Actions creado.");
    }

    void OnEnable()
    {
        _inputs.Enable();
        Debug.Log("[PlayerShooter] _inputs.Enable()");

        // Busca acciones por rutas comunes (ajusta si tus nombres difieren)
        _actFire   = FindActionFlexible("Gameplay/Fire", "Player/Fire", "Fire", "Combat/Fire");
        _actCover  = FindActionFlexible("Gameplay/Cover", "Player/Cover", "Cover", "Combat/Cover");
        _actReload = FindActionFlexible("Gameplay/Reload", "Player/Reload", "Reload", "Combat/Reload"); // opcional

        LogBindingResult("_actFire",  _actFire);
        LogBindingResult("_actCover", _actCover);
        LogBindingResult("_actReload", _actReload, optional: true);

        if (_actFire != null)
        {
            _actFire.performed += OnFirePerformed;
            _actFire.Enable();
            Debug.Log("[PlayerShooter] Subscribed to Fire.performed");
        }
        if (_actCover != null)
        {
            _actCover.performed += OnCoverPerformed;
            _actCover.canceled  += OnCoverCanceled;
            _actCover.Enable();
            Debug.Log("[PlayerShooter] Subscribed to Cover.performed/canceled");
        }
        if (_actReload != null)
        {
            _actReload.performed += OnReloadPerformed;
            _actReload.Enable();
            Debug.Log("[PlayerShooter] Subscribed to Reload.performed");
        }

        // Si algo no se enlazó, lista mapas/acciones para que copies el nombre exacto
        if (_actFire == null || _actCover == null)
        {
            Debug.LogWarning("[PlayerShooter] No se encontraron algunas acciones. Mapas/acciones disponibles:");
            foreach (var map in _inputs.asset.actionMaps)
            {
                Debug.Log($"  Map: {map.name}");
                foreach (var a in map.actions)
                    Debug.Log($"    Action: {a.name}");
            }
        }
    }

    void OnDisable()
    {
        if (_actFire != null)
        {
            _actFire.performed -= OnFirePerformed;
            _actFire.Disable();
            Debug.Log("[PlayerShooter] Unsubscribed Fire");
        }
        if (_actCover != null)
        {
            _actCover.performed -= OnCoverPerformed;
            _actCover.canceled  -= OnCoverCanceled;
            _actCover.Disable();
            Debug.Log("[PlayerShooter] Unsubscribed Cover");
        }
        if (_actReload != null)
        {
            _actReload.performed -= OnReloadPerformed;
            _actReload.Disable();
            Debug.Log("[PlayerShooter] Unsubscribed Reload");
        }

        _inputs.Disable();
        Debug.Log("[PlayerShooter] _inputs.Disable()");
    }

    // ───────────── Input callbacks ─────────────

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PlayerShooter] Fire.performed");
        TryShoot();
    }

    private void OnCoverPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PlayerShooter] Cover.performed (DOWN)");
        SetCover(true);
    }

    private void OnCoverCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PlayerShooter] Cover.canceled (UP)");
        SetCover(false);
    }

    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("[PlayerShooter] Reload.performed");
        if (!_isReloading && _currentAmmo < magazineSize)
            StartCoroutine(ReloadRoutine());
    }

    // ───────────── Lógica jugador ─────────────

    private void SetCover(bool cover)
    {
        if (Time.time - _lastCoverToggle < coverDebounce) return;
        _lastCoverToggle = Time.time;

        isInCover = cover;
        if (_health) _health.invulnerable = isInCover;
        Debug.Log($"[PlayerShooter] COVER = {isInCover}");

        if (isInCover && _currentAmmo < magazineSize && !_isReloading)
        {
            Debug.Log("[PlayerShooter] Recarga automática al entrar en cover.");
            StartCoroutine(ReloadRoutine());
        }
    }

    private void TryShoot()
    {
        if (isInCover || _isReloading) { Debug.Log("[PlayerShooter] No dispara (cover o reloading)."); return; }
        if (Time.time < _nextShootTime) { Debug.Log("[PlayerShooter] Rate limit."); return; }
        if (_currentAmmo <= 0)          { Debug.Log("[PlayerShooter] Sin balas."); return; }

        _nextShootTime = Time.time + (1f / fireRate);
        _currentAmmo--;

        // Ray DESDE EL CURSOR (no centro fijo)
        Ray ray = GetAimRay();

        // Línea de ayuda en Scene view (requiere Gizmos ON)
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxRange, Color.yellow, 0.15f);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, raycastMask))
        {
            Debug.Log($"[PlayerShooter] Hit: {hit.collider.name}");

            // Tracer visible en Game view (hasta el impacto)
            if (drawTracerInGame) DrawTracer(ray.origin, hit.point, tracerColorHit);

            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damagePerShot, hit.point, hit.normal);
                Debug.Log($"[PlayerShooter] Damage {damagePerShot} aplicado.");
            }
        }
        else
        {
            Debug.Log("[PlayerShooter] Miss.");
            // Tracer en Game view (hasta alcance máximo)
            if (drawTracerInGame) DrawTracer(ray.origin, ray.origin + ray.direction * maxRange, tracerColorMiss);
        }
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        Debug.Log("[PlayerShooter] Reload start...");
        yield return new WaitForSeconds(reloadTime);
        _currentAmmo = magazineSize;
        _isReloading = false;
        Debug.Log("[PlayerShooter] Reload done.");
    }

    // Expuesto para que la IA sepa si puede dispararte
    public bool IsExposed => !isInCover;

    // ───────────── Utilidades ─────────────

    private Ray GetAimRay()
    {
        if (!mainCamera) mainCamera = Camera.main;

        Vector2 screenPos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        return mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
    }

    private InputAction FindActionFlexible(params string[] paths)
    {
        // Busca "Map/Action" o solo "Action"
        foreach (var path in paths)
        {
            var a = _inputs.asset.FindAction(path, throwIfNotFound: false);
            if (a != null) return a;
        }
        return null;
    }

    private void LogBindingResult(string label, InputAction action, bool optional = false)
    {
        if (action != null)
        {
            Debug.Log($"[PlayerShooter] {label} enlazada a '{action.actionMap?.name}/{action.name}'");
        }
        else
        {
            var msg = $"[PlayerShooter] {label} NO encontrada. Ajusta nombres en FindActionFlexible(...) o renombra en tu .inputactions.";
            if (optional) Debug.Log(msg);
            else Debug.LogWarning(msg);
        }
    }

    private void DrawTracer(Vector3 start, Vector3 end, Color color)
    {
        var go = new GameObject("ShotTracer");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Material simple incorporado
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = tracerWidth;
        lr.endWidth = tracerWidth;
        lr.startColor = color;
        lr.endColor = color;
        lr.numCapVertices = 2;
        lr.useWorldSpace = true;

        Destroy(go, tracerDuration);
    }
}