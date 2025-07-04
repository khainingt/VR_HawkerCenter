// MyMarkerPool.cs
using System.Collections.Generic;
using UnityEngine;

public class MyMarkerPool : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    private Dictionary<string, MyMarkerController> markerMap = new Dictionary<string, MyMarkerController>();

    /// <summary>
    /// 获取已存在或新建的二维码标记对象
    /// </summary>
    /// <param name="id">二维码内容作为唯一标识</param>
    /// <returns>可用的 MyMarkerController 实例</returns>
    public MyMarkerController GetOrCreateMarker(string id)
    {
        if (!markerMap.ContainsKey(id))
        {
            GameObject markerObj = Instantiate(markerPrefab, transform);
            MyMarkerController marker = markerObj.GetComponent<MyMarkerController>();
            if (marker == null)
            {
                Debug.LogError("MarkerPrefab 上缺少 MyMarkerController 脚本组件！");
                return null;
            }
            markerMap[id] = marker;
        }

        var resultMarker = markerMap[id];
        resultMarker.gameObject.SetActive(true);
        return resultMarker;
    }

    /// <summary>
    /// 隐藏所有二维码标记
    /// </summary>
    public void HideAll()
    {
        foreach (var kvp in markerMap)
        {
            if (kvp.Value != null)
                kvp.Value.gameObject.SetActive(false);
        }
    }
}
