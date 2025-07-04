// MyMarkerController.cs
using UnityEngine;
using TMPro;

public class MyMarkerController : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot; // 固定尺寸 Cube
    [SerializeField] private TextMeshPro textLabel;  // 可选标签显示

    /// <summary>
    /// 更新二维码标记位置和缩放，Cube始终显示在二维码中心，远小近大
    /// </summary>
    public void UpdateMarker(Vector3 position, Quaternion rotation, Vector3 scale, string labelText)
    {
        //  放置在二维码的位置
        transform.position = position;

        //  始终正立放置，碗开口朝上
        transform.rotation = Quaternion.Euler(0, 0, 0); // 或 Quaternion.identity

        if (visualRoot != null)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, position);
            float scaleFactor = Mathf.Clamp(0.05f * distance, 0.05f, 0.3f);
            visualRoot.transform.localScale = Vector3.one * scaleFactor;
        }

        if (textLabel != null)
        {
            textLabel.text = labelText;
            textLabel.transform.LookAt(Camera.main.transform);
            textLabel.transform.Rotate(0, 180f, 0);
        }
    }


}
