using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class VoiceTextSender : MonoBehaviour
{
    public AIChatManager chatManager;

    public void SendTextToBackend(string recognizedText)
    {
        Debug.Log("识别到的文本：" + recognizedText);
        string url = $"https://{chatManager.serverIP}:5006/receive";
        StartCoroutine(PostRequest(url, recognizedText));
    }

    IEnumerator PostRequest(string url, string text)
    {
        string json = "{\"text\":\"" + text + "\"}";
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.certificateHandler = new AcceptAllCertificates();  // 如果你还在用 HTTPS
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("✅ AI 回复返回：" + response);

            if (chatManager != null)
            {
                chatManager.UpdateAIReply(response); // 🔄 实时更新
            }
        }
        else
        {
            Debug.LogError("❌ 发送文本失败: " + request.error);
        }
    }


    // 自定义证书处理器（跳过验证）
    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
