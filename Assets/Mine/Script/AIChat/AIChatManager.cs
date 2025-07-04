using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AIChatManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI userUtteranceText;   // 从 "User Utterance" 获取的文本
    public TextMeshProUGUI aiAnswerText;        // 显示 AI 返回结果
    public TextMeshProUGUI debugText;           // 可选调试信息输出
    
    [Header("AI Animation Control")]
    public AIAnimatorManager animatorManager;

    [Header("Server Settings")]
    public string serverIP = "10.249.157.127";   // 🧠 替换为你自己的 IP
    public int port = 5006;

    public void UpdateAIReply(string json)
    {
        var reply = JsonUtility.FromJson<ReplyData>(json);
        if (aiAnswerText != null)
            StartCoroutine(TypeTextEffect(reply.reply));
        if (debugText != null)
            debugText.text = "Response has updated";
    }

    public void OnSendClicked()
    {
        string prompt = userUtteranceText != null ? userUtteranceText.text.Trim() : "";
        if (!string.IsNullOrEmpty(prompt))
        {
            StartCoroutine(SendPrompt(prompt));
        }
        else
        {
            Debug.LogWarning("⚠️ 用户输入为空，未发送。");
            if (debugText != null) debugText.text = "Input is empty";
        }
    }

    IEnumerator SendPrompt(string prompt)
    {
        string url = $"https://{serverIP}:{port}/ask";
        Debug.Log("🌐 请求：" + url);
        if (debugText != null) debugText.text = $"Rquirement：{url}";

        string json = JsonUtility.ToJson(new PromptData { prompt = prompt });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 🔐 跳过证书验证
            request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var reply = JsonUtility.FromJson<ReplyData>(request.downloadHandler.text);
                if (aiAnswerText != null)
                    StartCoroutine(TypeTextEffect(reply.reply));
                if (debugText != null)
                    debugText.text = "Rely successfully";
            }
            else
            {
                Debug.LogError("❌ 网络请求失败：" + request.error);
                if (debugText != null)
                    debugText.text = "Error" + request.error;
            }
        }
    }

    IEnumerator GetLatestReply()
    {
        string url = $"https://{serverIP}:{port}/latest";
        Debug.Log("🔄 获取历史回复：" + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 🔐 跳过证书验证
            request.certificateHandler = new AcceptAllCertificates();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var reply = JsonUtility.FromJson<ReplyData>(request.downloadHandler.text);
                if (aiAnswerText != null)
                    aiAnswerText.text = reply.reply;
                if (debugText != null)
                    debugText.text = "Initial reply loads successfully";
            }
            else
            {
                Debug.LogWarning("⚠️ 无法加载历史回复：" + request.error);
                if (debugText != null)
                    debugText.text = "Loading is fail：" + request.error;
            }
        }
    }

    [System.Serializable]
    public class PromptData
    {
        public string prompt;
    }

    [System.Serializable]
    public class ReplyData
    {
        public string reply;
    }

    // 🔐 自签名 HTTPS 跳过证书校验器（仅用于开发）
    private class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    IEnumerator TypeTextEffect(string text)
    {
        if (animatorManager != null)
            animatorManager.PlayTalkingAnimation();

        aiAnswerText.text = "";
        foreach (char c in text)
        {
            aiAnswerText.text += c;
            yield return new WaitForSeconds(0.03f); // 打字速度，可调整
        }

        if (animatorManager != null)
            animatorManager.PlayIdle();
    }

}
