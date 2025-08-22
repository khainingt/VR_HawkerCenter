using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StallSmellTrigger_Bitmask : MonoBehaviour
{
    [Header("Bit Index (0..15)")]
    [Range(0, 15)] public int bitIndex = 0;     // 每个摊位一个唯一 bit

    [Header("Rhythm (seconds)")]
    [Min(0.1f)] public float spraySeconds = 5f;
    [Min(0.1f)] public float pauseSeconds = 5f;
    public bool startWithSpray = true;         // 进入后先喷还是先停

    [Header("Trigger/Player Detection")]
    public string playerTag = "Player";
    public bool autoAddKinematicRigidbody = true;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugOverlay = true;
    public bool debugVerbose = false;

    private bool _inside = false;
    private bool _isSpraying = false;
    private float _phaseRemain = 0f;
    private Coroutine _loop;
    private BitmaskBus _bus;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (debugLogs) Debug.LogWarning($"[{name}] Collider.isTrigger 已设为 true", this);
        }
        if (autoAddKinematicRigidbody)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
    }

    void Start()
    {
        _bus = BitmaskBus.GetOrCreate();
        if (debugLogs && _bus == null) Debug.LogError($"[{name}] 未能获得 BitmaskBus。", this);
    }

    private bool IsPlayer(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag)) return true;
        if (other.GetComponent<CharacterController>() != null) return true;
        if (other.GetComponentInParent<CharacterController>() != null) return true;

        if (Camera.main != null)
        {
            var camRoot = Camera.main.transform.root;
            if (other.transform.IsChildOf(camRoot)) return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        if (_inside) return;

        _inside = true;
        if (debugLogs) Debug.Log($"[{name}] Enter by '{other.name}' (bit={bitIndex}). Start {(startWithSpray ? "SPRAY" : "PAUSE")} loop.", this);

        if (_loop == null) _loop = StartCoroutine(SprayPauseLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        _inside = false;
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }

        _isSpraying = false; _phaseRemain = 0f;

        // 退出：清除此摊位 bit 并发送
        _bus?.SetBit(bitIndex, false, this);
        if (debugLogs) Debug.Log($"[{name}] Exit. Bit {bitIndex} -> 0", this);
    }

    private System.Collections.IEnumerator SprayPauseLoop()
    {
        bool nextIsSpray = startWithSpray;

        while (_inside)
        {
            if (_bus == null) { yield return null; continue; }

            if (nextIsSpray)
            {
                _isSpraying = true;
                _phaseRemain = spraySeconds;

                _bus.SetBit(bitIndex, true, this);   // 置位=1（喷）
                if (debugVerbose) Debug.Log($"[{name}] SPRAY {spraySeconds:F1}s (bit {bitIndex}=1)", this);

                yield return WaitOrBreak(_phaseRemain);
            }
            else
            {
                _isSpraying = false;
                _phaseRemain = pauseSeconds;

                _bus.SetBit(bitIndex, false, this);  // 清零=0（停）
                if (debugVerbose) Debug.Log($"[{name}] PAUSE {pauseSeconds:F1}s (bit {bitIndex}=0)", this);

                yield return WaitOrBreak(_phaseRemain);
            }

            nextIsSpray = !nextIsSpray; // 交替
        }
    }

    private System.Collections.IEnumerator WaitOrBreak(float seconds)
    {
        float t = 0f;
        while (t < seconds && _inside)
        {
            t += Time.deltaTime;
            _phaseRemain = Mathf.Max(0f, seconds - t);
            yield return null;
        }
    }

    private void OnDisable()
    {
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }
        if (_inside && debugLogs) Debug.Log($"[{name}] OnDisable: leaving trigger.", this);

        // 禁用时也确保把该位清零
        _bus?.SetBit(bitIndex, false, this);
        _inside = false; _isSpraying = false; _phaseRemain = 0f;
    }

    // 简易悬浮面板（用于干跑可视）
    private void OnGUI()
    {
        if (!debugOverlay || Camera.main == null) return;

        Vector3 sp = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.2f);
        if (sp.z <= 0) return;

        float w = 220f, h = 80f;
        var rect = new Rect(sp.x + 10f, Screen.height - sp.y - h - 10f, w, h);
        GUI.Box(rect, $"Stall bit {bitIndex}\n" +
                      $"State: {(_inside ? (_isSpraying ? "SPRAY" : "PAUSE") : "IDLE")}\n" +
                      $"Remain: {_phaseRemain:F1}s");
    }
}
