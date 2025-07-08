using UnityEngine;

public class MarkerChildSwitcher : MonoBehaviour
{
    public GameObject markerVirtualObject;
    public GameObject TrayVirtualObject;
    public void ShowOnlyChildAtIndex(int index)
    {
        if (markerVirtualObject == null) return;

        int count = markerVirtualObject.transform.childCount;
        int count_Tray = TrayVirtualObject.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = markerVirtualObject.transform.GetChild(i);
            child.gameObject.SetActive(i == index);
        }
        for (int i = 0; i < count_Tray; i++)
        {
            Transform child = TrayVirtualObject.transform.GetChild(i);
            child.gameObject.SetActive(i == index);
        }
    }
}

