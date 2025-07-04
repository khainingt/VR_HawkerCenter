using UnityEngine;

public class ArmSwingLocomotion : MonoBehaviour
{
    public OVRCameraRig cameraRig;
    public CharacterController characterController;
    public float sensitivity = 1.5f;
    public float maxSpeed = 2.0f;
    public float swingThreshold = 0.01f;

    public bool enableMovement = false; // 新增控制字段

    private Transform leftHand;
    private Transform rightHand;
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;

    void Start()
    {
        leftHand = cameraRig.leftControllerAnchor != null ? cameraRig.leftControllerAnchor : cameraRig.leftHandAnchor;
        rightHand = cameraRig.rightControllerAnchor != null ? cameraRig.rightControllerAnchor : cameraRig.rightHandAnchor;

        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
    }

    void Update()
    {
        if (!enableMovement) return; // 控制是否允许移动

        Vector3 leftDelta = leftHand.position - prevLeftPos;
        Vector3 rightDelta = rightHand.position - prevRightPos;

        float leftZ = Vector3.Dot(leftDelta, cameraRig.centerEyeAnchor.forward);
        float rightZ = Vector3.Dot(rightDelta, cameraRig.centerEyeAnchor.forward);
        float swingPower = (Mathf.Abs(leftZ) + Mathf.Abs(rightZ)) * 0.5f;

        if (swingPower > swingThreshold)
        {
            Vector3 moveDir = cameraRig.centerEyeAnchor.forward;
            moveDir.y = 0f;
            moveDir.Normalize();

            float speed = Mathf.Clamp(swingPower * sensitivity * 50f, 0, maxSpeed);
            characterController.Move(moveDir * speed * Time.deltaTime);
            cameraRig.transform.position = characterController.transform.position;
        }

        prevLeftPos = leftHand.position;
        prevRightPos = rightHand.position;
    }
}
