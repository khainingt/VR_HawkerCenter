using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public Transform playerCamera;

    void LateUpdate()
    {
        if (playerCamera == null) return;
        transform.forward = (transform.position - playerCamera.position).normalized;
    }
}
