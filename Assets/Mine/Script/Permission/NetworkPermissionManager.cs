using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using System.Collections;

public class NetworkPermissionManager : MonoBehaviour
{
    [Header("Server Settings")]
    public string serverIP = "10.249.157.127"; // ⚠️ 替换为你的电脑局域网 IP
    public int port = 5006;
    public float delayBeforeRequest = 1.0f;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(CheckAndRequestPermissionThenInit());
#else
        StartCoroutine(InitNetworkTest());
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator CheckAndRequestPermissionThenInit()
    {
        Debug.Log("当前 ACCESS_FINE_LOCATION 授权状态: " + Permission.HasUserAuthorizedPermission(Permission.FineLocation));

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) ||
            !Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Debug.Log("请求 ACCESS_FINE_LOCATION 与 COARSE 权限...");
            Permission.RequestUserPermissions(new string[] {
                Permission.FineLocation,
                Permission.CoarseLocation
            });
            yield return new WaitForSeconds(2f); // 等待用户响应
        }
        else
        {
            Debug.Log("位置权限已授予");
        }

        yield return new WaitForSeconds(delayBeforeRequest);
        StartCoroutine(InitNetworkTest());
    }
#endif

    IEnumerator InitNetworkTest()
    {
        string url = $"https://{serverIP}:{port}/ping";
        Debug.Log("测试连接地址: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // ✅ 跳过自签名 HTTPS 证书校验
            request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("连接成功：" + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"网络请求失败: {request.error}");
            }
        }
    }

    // ✅ 自签证书处理器（仅用于开发）
    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
