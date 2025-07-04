// MyQrCodeDisplayManager.cs
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Calib3dModule;

public class MyQrCodeDisplayManager : MonoBehaviour
{
#if ZXING_ENABLED
    [SerializeField] private QrCodeScanner scanner;
    [SerializeField] private MyMarkerPool markerPool;
    [SerializeField] private float qrCodeSizeInMeters = 0.1f; // 二维码物理尺寸（米）

    private async void Update()
    {
        var qrResults = await scanner.ScanFrameAsync();
        if (qrResults == null) return;

        markerPool.HideAll();

        foreach (var qrResult in qrResults)
        {
            if (qrResult?.corners == null || qrResult.corners.Length < 4)
                continue;

            MatOfPoint3f objectPoints = new MatOfPoint3f(
                new Point3(-qrCodeSizeInMeters / 2, -qrCodeSizeInMeters / 2, 0),
                new Point3(qrCodeSizeInMeters / 2, -qrCodeSizeInMeters / 2, 0),
                new Point3(qrCodeSizeInMeters / 2, qrCodeSizeInMeters / 2, 0),
                new Point3(-qrCodeSizeInMeters / 2, qrCodeSizeInMeters / 2, 0)
            );

            MatOfPoint2f imagePoints = new MatOfPoint2f(
                new Point(qrResult.corners[0].x * Screen.width, qrResult.corners[0].y * Screen.height),
                new Point(qrResult.corners[1].x * Screen.width, qrResult.corners[1].y * Screen.height),
                new Point(qrResult.corners[2].x * Screen.width, qrResult.corners[2].y * Screen.height),
                new Point(qrResult.corners[3].x * Screen.width, qrResult.corners[3].y * Screen.height)
            );

            float fovDegrees = 90f;
            float fx = Screen.width / (2f * Mathf.Tan(0.5f * fovDegrees * Mathf.Deg2Rad));
            float fy = fx;
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            Mat cameraMatrix = new Mat(3, 3, CvType.CV_64F);
            cameraMatrix.put(0, 0, fx); cameraMatrix.put(0, 1, 0);  cameraMatrix.put(0, 2, cx);
            cameraMatrix.put(1, 0, 0);  cameraMatrix.put(1, 1, fy); cameraMatrix.put(1, 2, cy);
            cameraMatrix.put(2, 0, 0);  cameraMatrix.put(2, 1, 0);  cameraMatrix.put(2, 2, 1);

            Mat rvec = new Mat();
            Mat tvec = new Mat();
            bool success = Calib3d.solvePnP(objectPoints, imagePoints, cameraMatrix, new MatOfDouble(), rvec, tvec);
            if (!success) continue;

            Vector3 position = new Vector3(
                (float)tvec.get(0, 0)[0],
                (float)tvec.get(1, 0)[0],
                (float)tvec.get(2, 0)[0]
            );

            Mat rotMat = new Mat();
            Calib3d.Rodrigues(rvec, rotMat);
            Matrix4x4 m = new Matrix4x4();
            m.SetColumn(0, new Vector4((float)rotMat.get(0, 0)[0], (float)rotMat.get(1, 0)[0], (float)rotMat.get(2, 0)[0], 0));
            m.SetColumn(1, new Vector4((float)rotMat.get(0, 1)[0], (float)rotMat.get(1, 1)[0], (float)rotMat.get(2, 1)[0], 0));
            m.SetColumn(2, new Vector4((float)rotMat.get(0, 2)[0], (float)rotMat.get(1, 2)[0], (float)rotMat.get(2, 2)[0], 0));
            m.SetColumn(3, new Vector4(0, 0, 0, 1));
            Quaternion rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));

            Transform cam = Camera.main.transform;
            position = cam.TransformPoint(position);
            rotation = cam.rotation * rotation;

            var marker = markerPool.GetOrCreateMarker(qrResult.text);
            marker.UpdateMarker(position, rotation, Vector3.one * qrCodeSizeInMeters, qrResult.text);
        }
    }
#endif
}