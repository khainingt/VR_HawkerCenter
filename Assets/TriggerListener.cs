using UnityEngine;

public class TriggerListener : MonoBehaviour
{
    public GenerateBitmask controller;

    public void Init(GenerateBitmask ctrl)
    {
        controller = ctrl;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (controller != null)
        {
            controller.OnTriggerChanged(other, true);
            //controller.OnCollisionLog($"?? 进入触发: {gameObject.name} -> {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (controller != null)
        {
            controller.OnTriggerChanged(other, false);
            //controller.OnCollisionLog($"?? 离开触发: {gameObject.name} -> {other.name}");
        }
    }

    // 可选：添加持续触发检测
    private void OnTriggerStay(Collider other)
    {
        if (controller != null)
        {
            //controller.OnTriggerStayDetected(other);
        }
    }
}
