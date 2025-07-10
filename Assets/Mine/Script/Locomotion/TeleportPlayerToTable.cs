using UnityEngine;

public class TeleportPlayerToTable : MonoBehaviour
{
    public Transform playerTransform;

    public void TeleportToOrigin()
    {
        if (playerTransform != null)
        {
            ArmSwingLocomotion locomotion = playerTransform.GetComponent<ArmSwingLocomotion>();
            locomotion.enableMovement = false;
            playerTransform.position = Vector3.zero;
        }
    }
}

