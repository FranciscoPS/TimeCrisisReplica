using UnityEngine;
using UnityEngine.Animations;

public class EnemyShield : MonoBehaviour
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
    public float hideMoveTime = 0.22f;

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

    [Header("Animator")]
    public Animator animator;

    [Header("Debug")]
    public bool drawGizmos = true;
    public float poseSnapEpsilon = 0.003f;

    // ---- internos ----
    private State _state;
    private float _timer;
    private Coroutine _moveCR;
    private bool _moving;

    void Start()
    {
        if (!hiddenPose || !popOutPose)
        {
            Debug.LogError($"[EnemyPopper] {name}: Asigna hiddenPose y popOutPose.");
            enabled = false;
            return;
        }

        if (!lookPivot) lookPivot = transform;

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
                break;
        }
    }

    // ---------- helpers FSM ----------
    private void SetState(State s, float wait)
    {
        _state = s;
        _timer = wait;
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

    private void FacePlayerYaw()
    {
        if (_playerTr == null || lookPivot == null) return;

        Vector3 toPlayer = _playerTr.position - lookPivot.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        lookPivot.rotation = Quaternion.Slerp(lookPivot.rotation, target, Time.deltaTime * yawSpeed);
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
