using UnityEngine;
using UnityEngine.InputSystem; // Mouse.current y InputAction

// Asegúrate de haber generado la clase InputSystem_Actions desde tu .inputactions

[DisallowMultipleComponent]
public class PlayerShooter : MonoBehaviour
{
    [Header("Refs")]
    public Camera mainCamera; // Déjalo vacío y tomará Camera.main
    public LayerMask raycastMask; // Marca Environment (y luego Enemy)

    [Header("Gun")]
    public int magazineSize = 12;
    public float fireRate = 6f; // balas/seg
    public float damagePerShot = 34f;
    public float maxRange = 150f;
    public float reloadTime = 1.0f;

    [Header("Cover")]
    public bool isInCover = false; // True mientras mantienes la acción Cover
    public float coverDebounce = 0.05f; // Evita rebotes de input

    [Header("Debug/Tracer In-Game")]
    public bool drawTracerInGame = true; // Línea visible en GAME view
    public float tracerDuration = 0.06f;
    public float tracerWidth = 0.02f;
    public Color tracerColorHit = Color.red;
    public Color tracerColorMiss = Color.yellow;

    // ─────────────────────────────────────────────────────────────────────────────

    private InputSystem_Actions _inputs; // Clase generada desde tu .inputactions
    private InputAction _actFire; // Resueltas dinámicamente por nombre
    private InputAction _actCover;
    private InputAction _actReload; // opcional

    private int _currentAmmo;
    private float _nextShootTime;
    private bool _isReloading;
    private float _lastCoverToggle;
    private Health _health;
    private bool _isGameOver;
    private bool _isTravelling; // true cuando está viajando entre zonas

    void Awake()
    {
        if (!mainCamera)
            mainCamera = Camera.main;
        _health = GetComponent<Health>();
        _currentAmmo = magazineSize;

        _inputs = new InputSystem_Actions();
    }

    void Start()
    {
        // Inicializar UI después de que todos los sistemas estén listos
        GameEvents.AmmoChanged?.Invoke(_currentAmmo, magazineSize);
        GameEvents.ReloadAlert?.Invoke(false);
        GameEvents.ReloadingStatus?.Invoke(false); // Asegurar que "Reloading..." esté oculto al inicio
    }

    void OnEnable()
    {
        _inputs.Enable();

        // Busca acciones por rutas comunes (ajusta si tus nombres difieren)
        _actFire = FindActionFlexible("Gameplay/Fire", "Player/Fire", "Fire", "Combat/Fire");
        _actCover = FindActionFlexible("Gameplay/Cover", "Player/Cover", "Cover", "Combat/Cover");
        _actReload = FindActionFlexible(
            "Gameplay/Reload",
            "Player/Reload",
            "Reload",
            "Combat/Reload"
        ); // opcional

        LogBindingResult("_actFire", _actFire);
        LogBindingResult("_actCover", _actCover);
        LogBindingResult("_actReload", _actReload, optional: true);

        if (_actFire != null)
        {
            _actFire.performed += OnFirePerformed;
            _actFire.Enable();
        }
        if (_actCover != null)
        {
            _actCover.performed += OnCoverPerformed;
            _actCover.canceled += OnCoverCanceled;
            _actCover.Enable();
        }
        if (_actReload != null)
        {
            _actReload.performed += OnReloadPerformed;
            _actReload.Enable();
        }

        // Si algo no se enlazó, lista mapas/acciones para que copies el nombre exacto
        if (_actFire == null || _actCover == null)
        {
            Debug.LogWarning("[PlayerShooter] No se encontraron algunas acciones.");
        }

        GameEvents.GameOver += OnGameOver;
        GameEvents.TravellingBetweenZones += OnTravellingChanged;
    }

    void OnDisable()
    {
        if (_actFire != null)
        {
            _actFire.performed -= OnFirePerformed;
            _actFire.Disable();
        }
        if (_actCover != null)
        {
            _actCover.performed -= OnCoverPerformed;
            _actCover.canceled -= OnCoverCanceled;
            _actCover.Disable();
        }
        if (_actReload != null)
        {
            _actReload.performed -= OnReloadPerformed;
            _actReload.Disable();
        }

        // IMPORTANTE: Desactivar completamente el InputSystem_Actions
        if (_inputs != null)
        {
            _inputs.Disable();
            _inputs.Dispose();
        }

        GameEvents.GameOver -= OnGameOver;
        GameEvents.TravellingBetweenZones -= OnTravellingChanged;
    }

    void OnDestroy()
    {
        // Cleanup adicional cuando el GameObject se destruye
        if (_inputs != null)
        {
            _inputs.Disable();
            _inputs.Dispose();
            Debug.Log("[PlayerShooter] InputSystem_Actions disposed in OnDestroy");
        }
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        if (_inputs != null)
        {
            _inputs.Disable();
        }
        Debug.Log("[PlayerShooter] GAME OVER");
    }
    
    private void OnTravellingChanged(bool travelling)
    {
        _isTravelling = travelling;
        
        // Si empieza a viajar y está en cobertura, forzar salida
        if (travelling && isInCover)
        {
            isInCover = false;
        }
    }

    // ───────────── Input callbacks ─────────────

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        TryShoot();
    }

    private void OnCoverPerformed(InputAction.CallbackContext ctx)
    {
        SetCover(true);
    }

    private void OnCoverCanceled(InputAction.CallbackContext ctx)
    {
        SetCover(false);
    }

    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        // Bloquear recarga si está viajando entre zonas
        if (!_isReloading && _currentAmmo < magazineSize && !_isGameOver && !_isTravelling)
            StartCoroutine(ReloadRoutine());
    }

    // ───────────── Lógica jugador ─────────────

    private void SetCover(bool cover)
    {
        // Bloquear cobertura si está viajando entre zonas
        if (Time.time - _lastCoverToggle < coverDebounce || _isGameOver || _isTravelling)
            return;
        _lastCoverToggle = Time.time;

        isInCover = cover;
        if (_health)
            _health.invulnerable = isInCover;

        if (isInCover && _currentAmmo < magazineSize && !_isReloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    void TryShoot()
    {
        // Bloquear disparo si está en cobertura, recargando, game over o viajando
        if (isInCover || _isReloading || _isGameOver || _isTravelling)
        {
            return;
        }
        if (Time.time < _nextShootTime)
        {
            return;
        }
        if (_currentAmmo <= 0)
        {
            return;
        }

        _nextShootTime = Time.time + (1f / fireRate);
        _currentAmmo--;
        GameEvents.AmmoChanged?.Invoke(_currentAmmo, magazineSize);
        
        // Reproducir sonido de disparo del jugador
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerShoot();
        }
        
        if (_currentAmmo <= 0)
        {
            GameEvents.ReloadAlert?.Invoke(true); // Mostrar "Reload!"
            return;
        }

        // Ray DESDE EL CURSOR (no centro fijo)
        Ray ray = GetAimRay();

        // Línea de ayuda en Scene view (requiere Gizmos ON)
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxRange, Color.yellow, 0.15f);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, raycastMask))
        {
            // Tracer visible en Game view (hasta el impacto)
            if (drawTracerInGame)
                DrawTracer(ray.origin, hit.point, tracerColorHit);

            // Verificar si el disparo impactó directamente el layer "Cover" (escudo)
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Cover"))
            {
                // El escudo bloquea el disparo - no hacer daño
                return;
            }

            // Si impactó un enemigo, verificar si hay daño
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(damagePerShot, hit.point, hit.normal);
            }

            if (hit.collider.TryGetComponent<Barrel>(out var barrel))
            {
                barrel.DestroyBarrel();
            }
        }
        else
        {
            // Tracer en Game view (hasta alcance máximo)
            if (drawTracerInGame)
                DrawTracer(ray.origin, ray.origin + ray.direction * maxRange, tracerColorMiss);
        }
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        GameEvents.ReloadAlert?.Invoke(false); // Ocultar "Reload!"
        GameEvents.ReloadingStatus?.Invoke(true); // Mostrar "Reloading..."

        // Reproducir sonido de recarga
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayReload();
        }

        Debug.Log("[PlayerShooter] Recargando...");
        yield return new WaitForSeconds(reloadTime);
        _currentAmmo = magazineSize;
        _isReloading = false;
        GameEvents.AmmoChanged?.Invoke(_currentAmmo, magazineSize);
        GameEvents.ReloadingStatus?.Invoke(false); // Ocultar "Reloading..."
    }

    // Expuesto para que la IA sepa si puede dispararte
    public bool IsExposed => !isInCover;

    // ───────────── Utilidades ─────────────

    private Ray GetAimRay()
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        Vector2 screenPos =
            Mouse.current != null
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
            if (a != null)
                return a;
        }
        return null;
    }

    private void LogBindingResult(string label, InputAction action, bool optional = false)
    {
        if (action == null && !optional)
        {
            Debug.LogWarning($"[PlayerShooter] {label} NO encontrada.");
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
