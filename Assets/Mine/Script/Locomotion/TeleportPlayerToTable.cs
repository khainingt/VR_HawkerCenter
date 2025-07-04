using UnityEngine;

public class TeleportPlayerToTable : MonoBehaviour
{
    public Transform playerTransform;

    public void TeleportToOrigin()
    {
        if (playerTransform != null)
        {
            playerTransform.position = Vector3.zero;
        }
    }
}

