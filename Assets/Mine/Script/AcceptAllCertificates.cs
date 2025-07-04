using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // 信任所有证书（仅用于开发阶段）
    }
}
