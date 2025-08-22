using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GenerateBitmask : MonoBehaviour
{
    public GameObject[] prefabs;
    public BLE_nrf52840 bleSender;
    public Text bitmaskDisplay;
    public Text bitmaskStatusPanel;
    public Text collisionStatusPanel;

    public float colliderHeight = 3f;
    public float colliderRadius = 0.75f;

    private ushort currentMask = 0;
    private ushort lastSentMask = 0;
    private Dictionary<Collider, int> colliderToBitIndex = new();
    private bool[] colliderAdded = new bool[10];
    private bool cameraColliderReady = false;
    private HashSet<int> activeTriggers = new();
    private GameObject dynamicColliderObject;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            dynamicColliderObject = new GameObject("CameraCollider");
            dynamicColliderObject.tag = "Player";
            dynamicColliderObject.transform.position = cam.transform.position;

            CapsuleCollider col = dynamicColliderObject.AddComponent<CapsuleCollider>();
            col.height = colliderHeight;
            col.radius = colliderRadius;
            col.direction = 2; // Z轴方向
            col.isTrigger = true;

            Rigidbody rb = dynamicColliderObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            cameraColliderReady = true;
            dynamicColliderObject.AddComponent<TriggerListener>().Init(this);
        }
    }

    void Update()
    {
        if (Camera.main != null && dynamicColliderObject != null)
        {
            dynamicColliderObject.transform.position = Camera.main.transform.position;
        }

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
                if (colliderAdded[i] && col != null && colliderToBitIndex.ContainsKey(col))
                {
                    colliderToBitIndex.Remove(col);
                    colliderAdded[i] = false;
                    currentMask &= (ushort)~(1 << i);
                    activeTriggers.Remove(i);
                }
            }
        }
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
            }
            else
            {
                currentMask &= (ushort)~(1 << index);
                activeTriggers.Remove(index);
            }
        }
    }
}
