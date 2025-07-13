using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GenerateBitmask : MonoBehaviour
{
    [Tooltip("最多10个子物体，按索引排列对应bit位置")]
    public GameObject[] prefabs;

    [Tooltip("用于发送bitmask的蓝牙控制脚本")]
    public BLE_nrf52840 bleSender;

    [Tooltip("参考的摄像头对象（如 XR Rig）")]
    public Transform cameraRig;
    /*
    [Tooltip("用于显示 Bitmask 和信息的 UI")]
    public Text bitmaskDisplay;

    [Tooltip("用于显示 Bitmask 数值的 UI")]
    public Text bitmaskStatusPanel;

    [Tooltip("用于显示碰撞系统状态的 UI")]
    public Text collisionStatusPanel;
    */
    private ushort currentMask = 0;
    private ushort lastSentMask = 0;
    private Dictionary<Collider, int> colliderToBitIndex = new();
    private bool[] colliderAdded = new bool[10];
    private bool cameraRigColliderReady = false;
    private HashSet<int> activeTriggers = new();
    private List<string> collisionLog = new();

    void Start()
    {
        // 初始化摄像头碰撞体
        if (cameraRig != null)
        {
            CapsuleCollider camCol = cameraRig.GetComponent<CapsuleCollider>();
            if (camCol == null)
            {
                camCol = cameraRig.gameObject.AddComponent<CapsuleCollider>();
                camCol.isTrigger = true;

                // 设置椭球体大小
                camCol.height = 3f;       // 椭球体高度（Z轴方向）
                camCol.radius = 0.75f;      // 半径用于 XZ 横截面

                // 默认 CapsuleCollider 的方向为 Y轴，可以设为 Z轴对齐（与用户朝向一致）
                camCol.direction = 1; // 0=X, 1=Y, 2=Z
            }

            Rigidbody camRb = cameraRig.GetComponent<Rigidbody>();
            if (camRb == null)
            {
                camRb = cameraRig.gameObject.AddComponent<Rigidbody>();
                camRb.isKinematic = true;
            }

            cameraRigColliderReady = (camCol != null && camCol.isTrigger && camRb != null);
            cameraRig.gameObject.AddComponent<TriggerListener>().Init(this);
        }
    }

    void Update()
    {
        // 动态注册和注销 Collider
        for (int i = 0; i < prefabs.Length && i < 10; i++)
        {
            GameObject obj = prefabs[i];
            if (obj == null) continue;

            Collider col = obj.GetComponent<Collider>();
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (obj.activeInHierarchy)
            {
                if (!colliderAdded[i] && col != null && col.isTrigger && rb != null && rb.isKinematic)
                {
                    colliderToBitIndex[col] = i;
                    colliderAdded[i] = true;
                }
            }
            else
            {
                // 注销并清除bit位
                if (colliderAdded[i] && col != null && colliderToBitIndex.ContainsKey(col))
                {
                    colliderToBitIndex.Remove(col);
                    colliderAdded[i] = false;
                    currentMask &= (ushort)~(1 << i); // 清除bit
                }
            }
        }

        /*// Bitmask文本 UI 更新
        if (bitmaskDisplay != null)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine("<b>对象触发状态：</b>");
            for (int i = 0; i < prefabs.Length && i < 10; i++)
            {
                GameObject obj = prefabs[i];
                if (obj != null)
                {
                    string status = obj.activeInHierarchy
                        ? ((currentMask & (1 << i)) != 0 ? " 已触发" : " 未触发")
                        : " 未激活";
                    sb.AppendLine($"[{i}] {obj.name}: {status}");
                }
                else
                {
                    sb.AppendLine($"[{i}] <无效对象>");
                }
            }
            bitmaskDisplay.text = sb.ToString();
        }

        if (bitmaskStatusPanel != null)
        {
            bitmaskStatusPanel.text = $"Bitmask: 0b{System.Convert.ToString(currentMask, 2).PadLeft(10, '0')}\nDecimal: {currentMask}";
        }

        // 碰撞信息面板更新
        if (collisionStatusPanel != null)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine("<b>实时碰撞信息：</b>");
            sb.AppendLine($"CameraRig Collider Ready: {(cameraRigColliderReady ? "yes" : "no")}");
            sb.AppendLine($"当前触发对象数: {activeTriggers.Count}");

            foreach (int i in activeTriggers)
            {
                if (i >= 0 && i < prefabs.Length && prefabs[i] != null)
                    sb.AppendLine($"➡️ 正在触发: [{i}] {prefabs[i].name}");
            }

            collisionStatusPanel.text = sb.ToString();
        }
        */

        // Bitmask变化后发送
        if (currentMask != lastSentMask)
        {
            lastSentMask = currentMask;
            bleSender.SendBitmask(currentMask);
        }
    }

    public void OnTriggerChanged(Collider col, bool entered)
    {
        if (colliderToBitIndex.TryGetValue(col, out int index))
        {
            GameObject obj = prefabs[index];
            if (obj == null || !obj.activeInHierarchy)
                return;

            if (entered)
            {
                currentMask |= (ushort)(1 << index);
                activeTriggers.Add(index);
                //collisionLog.Add($"➡️ {cameraRig.name} 进入 [{index}] {obj.name}");
                Debug.Log($"[Trigger Enter] CameraRig 进入 [{index}] {obj.name}");
            }
            else
            {
                currentMask &= (ushort)~(1 << index);
                activeTriggers.Remove(index);
                //collisionLog.Add($"⬅️ {cameraRig.name} 离开 [{index}] {obj.name}");
                Debug.Log($"[Trigger Exit] CameraRig 离开 [{index}] {obj.name}");
            }

            // 限制 log 条数
            if (collisionLog.Count > 10)
                collisionLog.RemoveAt(0);
        }
    }

}
