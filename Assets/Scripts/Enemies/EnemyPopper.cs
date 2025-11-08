using UnityEngine;
using UnityEngine.Animations;

[DisallowMultipleComponent]
public class EnemyPopper : MonoBehaviour
{
    public enum State { Hidden, MovingToPop, Exposed, MovingToHidden }

    [Header("Poses (asigna en escena)")]
    public Transform hiddenPose;     // Dónde se oculta
    public Transform popOutPose;     // Dónde se asoma

    [Header("Tiempos aleatorios (segundos)")]
    public Vector2 coverWaitRange = new Vector2(0.7f, 1.2f);
    public Vector2 exposeWaitRange = new Vector2(0.6f, 1.0f);

    [Header("Duración del movimiento (segundos)")]
    public float popOutMoveTime = 0.22f;
    public float hideMoveTime   = 0.22f;

    [Header("Tween")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Opcional: respetar cover del jugador")]
    public bool respectPlayerCover = true;       // si true, solo se asoma si el jugador está expuesto
    public string playerTag = "Player";
    private PlayerShooter _playerShooter;
    private Transform _playerTr;

    [Header("Opcional: mirar al jugador (solo yaw)")]
    public bool facePlayerYaw = true;
    public Transform lookPivot;                  // si null, usa transform
    public float yawSpeed = 12f;

    [Header("Opcional: disparo simple mientras está expuesto")]
    public bool shootingEnabled = false;
    public Transform muzzlePoint;
    public float damagePerShot = 10f;
    public float shootInterval = 0.15f;         // cadencia durante el estado Exposed
    public float maxRange = 200f;
    public float spreadDegrees = 2.0f;
    
    [Header("Precisión del disparo")]
    [Range(0f, 1f)]
    [Tooltip("Probabilidad de que el disparo acierte (0.0 = nunca, 1.0 = siempre)")]
    public float accuracy = 0.7f; // 70% de probabilidad de dar
    
    public Animator animator;

    

    [Header("Debug")]
    public bool drawGizmos = true;
    public float poseSnapEpsilon = 0.003f;

    // ---- internos ----
    private State _state;
    private float _timer;
    private Coroutine _moveCR;
    private bool _moving;
    private float _shootTimer;
    private Health _health;

    void Start()
    {
        if (!hiddenPose || !popOutPose)
        {
            Debug.LogError($"[EnemyPopper] {name}: Asigna hiddenPose y popOutPose.");
            enabled = false;
            return;
        }

        if (!lookPivot) lookPivot = transform;

        // Obtener referencia al Health component para verificar si está muerto
        _health = GetComponent<Health>();

        var pGo = GameObject.FindGameObjectWithTag(playerTag);
        if (pGo)
        {
            _playerTr = pGo.transform;
            _playerShooter = pGo.GetComponent<PlayerShooter>();
        }

        // Estado inicial: oculto y clavado en hidden
        SnapToPose(hiddenPose);
        SetState(State.Hidden, RandomRange(coverWaitRange));
    }

    void Update()
    {
        // Si el enemigo está muerto, detener toda la lógica de movimiento y ataque
        if (IsDead())
        {
            // Detener movimiento si está en progreso
            if (_moveCR != null)
            {
                StopCoroutine(_moveCR);
                _moveCR = null;
                _moving = false;
            }
            return;
        }

        switch (_state)
        {
            case State.Hidden:
                _timer -= Time.deltaTime;
                if (_timer <= 0f && !_moving)
                {
                    if (!respectPlayerCover || IsPlayerExposed())
                    {
                        StartMove(hiddenPose, popOutPose, popOutMoveTime, State.MovingToPop, () =>
                        {
                            _shootTimer = 0f; // reset cadencia
                            SetState(State.Exposed, RandomRange(exposeWaitRange));
                            
                        });
                    }
                    else
                    {
                        _timer = 0.25f; // reintenta pronto
                    }
                }
                break;

            case State.Exposed:
                // mirar al player (opc)
                if (facePlayerYaw) FacePlayerYaw();
                
                // disparo simple (opc)
                if (shootingEnabled)
                {
                    _shootTimer -= Time.deltaTime;
                    if (_shootTimer <= 0f)
                    {
                        // Disparar sin animación requerida
                        if (animator) animator.SetTrigger("SHOOT");
                        _shootTimer = shootInterval;
                        FireOneShot();
                    }
                }

                _timer -= Time.deltaTime;
                if (_timer <= 0f && !_moving)
                {
                    StartMove(popOutPose, hiddenPose, hideMoveTime, State.MovingToHidden, () =>
                    {
                        SetState(State.Hidden, RandomRange(coverWaitRange));
                    });
                }
                break;

            case State.MovingToPop:
            case State.MovingToHidden:
                // lo maneja la coroutine
                // (puedes seguir haciendo face yaw aquí si te gusta, pero no es necesario)
                break;
        }
    }

    // ---------- helpers FSM ----------
    private void SetState(State s, float wait)
    {
        _state = s;
        _timer = wait;
        // Debug.Log($"[EnemyPopper] {name} -> {_state} (wait={wait:0.00}s)");
    }

    private float RandomRange(Vector2 range)
    {
        return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
    }

    // ---------- Tween seguro ----------
    private void StartMove(Transform from, Transform to, float duration, State movingState, System.Action onComplete)
    {
        if (_moving) return;
        if (!from || !to) return;
        if (IsDead()) return; // No iniciar movimientos si está muerto

        // Snap al origen del tween para evitar deriva
        if (!IsAtPose(from)) SnapToPose(from);

        if (_moveCR != null) StopCoroutine(_moveCR);
        _moveCR = StartCoroutine(MoveRoutine(from, to, duration, movingState, onComplete));
    }

    private System.Collections.IEnumerator MoveRoutine(Transform from, Transform to, float duration, State movingState, System.Action onComplete)
    {
        _moving = true;
        SetState(movingState, duration);

        Vector3 startPos = from.position;
        Quaternion startRot = from.rotation;
        Vector3 endPos = to.position;
        Quaternion endRot = to.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eval = moveCurve.Evaluate(k);

            transform.position = Vector3.Lerp(startPos, endPos, eval);
            transform.rotation = Quaternion.Slerp(startRot, endRot, eval);

            // (Opcional) yaw durante el movimiento:
            if (facePlayerYaw) FacePlayerYaw();

            yield return null;
        }

        SnapToPose(to);
        _moving = false;
        onComplete?.Invoke();
    }

    private void SnapToPose(Transform pose)
    {
        transform.SetPositionAndRotation(pose.position, pose.rotation);
    }

    private bool IsAtPose(Transform pose)
    {
        if (!pose) return false;
        return (transform.position - pose.position).sqrMagnitude <= poseSnapEpsilon * poseSnapEpsilon &&
               Quaternion.Angle(transform.rotation, pose.rotation) <= 0.25f;
    }

    // ---------- Opcionales ----------
    private bool IsPlayerExposed()
    {
        if (_playerShooter == null) return true; // si no hay referencia, asumimos expuesto
        return _playerShooter.IsExposed;
    }

    private bool IsDead()
    {
        return _health != null && _health.Current <= 0f;
    }

    private void FacePlayerYaw()
    {
        if (_playerTr == null || lookPivot == null) return;

        Vector3 toPlayer = _playerTr.position - lookPivot.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        lookPivot.rotation = Quaternion.Slerp(lookPivot.rotation, target, Time.deltaTime * yawSpeed);
    }

    private void FireOneShot()
    {
        if (!muzzlePoint) return;

        // Buscar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        // Verificar primero si el disparo va a dar según accuracy
        bool willHit = Random.Range(0f, 1f) <= accuracy;
        
        Debug.Log($"[EnemyPopper] {name} disparo - Accuracy: {accuracy}, WillHit: {willHit}");
        
        // Dirección hacia el jugador
        Vector3 targetPos = player.transform.position + Vector3.up * 1.5f; // Apuntar al torso
        Vector3 dir = (targetPos - muzzlePoint.position).normalized;
        
        // Si no va a dar, agregar spread extra para fallar intencionalmente
        if (!willHit)
        {
            // Spread extra para fallar a propósito
            float missSpread = spreadDegrees + Random.Range(5f, 15f);
            dir = Quaternion.Euler(
                Random.Range(-missSpread, missSpread),
                Random.Range(-missSpread, missSpread),
                0f) * dir;
        }
        else
        {
            // Spread normal para disparos que pueden dar
            dir = Quaternion.Euler(
                Random.Range(-spreadDegrees, spreadDegrees),
                Random.Range(-spreadDegrees, spreadDegrees),
                0f) * dir;
        }

        Ray ray = new Ray(muzzlePoint.position, dir);
        
        // Debug visual del disparo
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxRange, willHit ? Color.red : Color.yellow, 0.2f);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            Debug.Log($"[EnemyPopper] {name} raycast hit: {hit.collider.name} (Tag: {hit.collider.tag})");
            
            if (hit.collider.CompareTag("Player") &&
                hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                // AQUÍ aplicamos el accuracy - solo hace daño si willHit es true
                if (willHit)
                {
                    dmg.TakeDamage(damagePerShot, hit.point, hit.normal);
                    Debug.Log($"[EnemyPopper] {name} HIT Player! Daño: {damagePerShot} (Accuracy success)");
                }
                else
                {
                    Debug.Log($"[EnemyPopper] {name} impactó Player pero FALLÓ por accuracy ({accuracy})");
                }
            }
            else
            {
                Debug.Log($"[EnemyPopper] {name} hit algo que no es Player o no tiene IDamageable");
            }
        }
        else
        {
            Debug.Log($"[EnemyPopper] {name} raycast MISS - no hit nada");
        }
    }

    // ---------- Gizmos ----------
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.cyan;
        if (hiddenPose) Gizmos.DrawSphere(hiddenPose.position, 0.06f);
        Gizmos.color = Color.magenta;
        if (popOutPose) Gizmos.DrawSphere(popOutPose.position, 0.06f);
        if (hiddenPose && popOutPose)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hiddenPose.position, popOutPose.position);
        }
    }
}
