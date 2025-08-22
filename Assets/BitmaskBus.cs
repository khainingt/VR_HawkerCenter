using UnityEngine;

public class BitmaskBus : MonoBehaviour
{
    public static BitmaskBus Instance { get; private set; }

    [Header("BLE")]
    public BLE_nrf52840 ble;              // 可手动拖拽；为空时会尝试自动查找
    public bool dryRunNoBLE = true;       // 干跑：不连硬件，只打印/调试
    public bool debugLogs = true;

    [Header("Bit Mapping")]
    public bool msbIndexing = false;      // false: bit0=LSB(0x0001)；true: bit0=MSB(0x8000)
    public bool exclusiveMode = false;    // 设 true 时同一时刻只允许一个摊位置位（可选）

    [Header("State")]
    [SerializeField] private ushort currentMask = 0;
    [SerializeField] private ushort lastSentMask = 0;
    [SerializeField] private float lastSendAt = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (ble == null) ble = FindObjectOfType<BLE_nrf52840>();
        if (ble == null && !dryRunNoBLE)
            LogError("未找到 BLE_nrf52840 且 dryRunNoBLE = false，将无法真正发送。");
    }

    public static BitmaskBus GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("BitmaskBus");
        return go.AddComponent<BitmaskBus>();
    }

    public void SetBit(int bitIndex, bool value, Object ctx = null)
    {
        ushort bit = MaskForIndex(bitIndex);
        if (bit == 0) { LogError($"非法 bitIndex={bitIndex}（应在 0..15）"); return; }

        ushort newMask = currentMask;

        if (exclusiveMode && value)
            newMask = bit;                 // 只保留该位
        else
            newMask = value ? (ushort)(newMask | bit) : (ushort)(newMask & ~bit);

        if (newMask != currentMask)
        {
            currentMask = newMask;
            SendMask(ctx);
        }
    }

    public void ClearAll(Object ctx = null)
    {
        if (currentMask != 0)
        {
            currentMask = 0;
            SendMask(ctx);
        }
    }

    private ushort MaskForIndex(int idx)
    {
        if (idx < 0 || idx > 15) return 0;
        int bitPos = msbIndexing ? (15 - idx) : idx;
        return (ushort)(1 << bitPos);
    }

    private void SendMask(Object ctx)
    {
        lastSendAt = Time.time;
        if (dryRunNoBLE || ble == null)
        {
            if (debugLogs) Debug.Log($"[BitmaskBus] [DRY RUN] SendBitmask: 0x{currentMask:X4}", this);
            lastSentMask = currentMask;
            return;
        }

        try
        {
            ble.SendBitmask(currentMask);
            if (debugLogs) Debug.Log($"[BitmaskBus] SendBitmask: 0x{currentMask:X4}", this);
            lastSentMask = currentMask;
        }
        catch (System.Exception ex)
        {
            LogError($"发送失败: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public ushort GetCurrentMask() => currentMask;
    public ushort GetLastSentMask() => lastSentMask;

    private void LogError(string msg) =>
        Debug.LogError($"[BitmaskBus] {msg}", this);
}
