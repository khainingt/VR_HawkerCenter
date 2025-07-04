using UnityEngine;

public class TeleportPlayerToOrigin : MonoBehaviour
{
    public Transform player;                    
    public GameObject mainMenuUI;             
    public ArmSwingLocomotion armSwingScript;

    public void TeleportToOrigin()
    {
        player.position = new Vector3(19f, 0f, -66f);
        mainMenuUI.SetActive(true);
        armSwingScript.enableMovement = false;
    }
}
