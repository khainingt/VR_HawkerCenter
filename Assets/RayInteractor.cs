using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RayInteractor : MonoBehaviour
{
    public float rayLength = 10f;
    public LayerMask interactableLayer; // Inspector 勾选 UI 层
    private LineRenderer lr;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.008f;
        lr.endWidth = 0.004f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 endPoint = origin + direction * rayLength;

        //Debug.Log($"[Raycast] Firing ray from {origin} in direction {direction}");

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, interactableLayer))
        {
            endPoint = hit.point;
            GameObject hitObj = hit.collider.gameObject;

            string layerName = LayerMask.LayerToName(hitObj.layer);
            Debug.Log($"[Raycast] Hit object: {hitObj.name}, Layer: {layerName}");

            Button button = hitObj.GetComponent<Button>();
            if (button != null)
            {
                lr.startColor = lr.endColor = Color.red;
                Debug.Log($"[Button] Button component FOUND on {hitObj.name}");

                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
                {
                    //Debug.Log("[Button] Trigger pressed, invoking Button.onClick");
                    button.onClick.Invoke();
                }
                else
                {
                    //Debug.Log("[Button] Button found, but trigger not pressed this frame");
                }
            }
            else
            {
                lr.startColor = lr.endColor = Color.yellow;
                //Debug.Log("[Button] No Button component found on hit object");
            }
        }
        else
        {
            lr.startColor = lr.endColor = Color.white;
            //Debug.Log("[Raycast] No object hit (Ray Missed)");
        }

        lr.SetPosition(0, origin);
        lr.SetPosition(1, endPoint);
    }

}
