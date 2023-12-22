using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SaveScreenShot : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;

    public void SaveScreenShotToGallery()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        SaveScreenShotToGalleryAsync(ct).Forget();
    }

    private async UniTask SaveScreenShotToGalleryAsync(CancellationToken cancelToken)
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        var renderTexture = new RenderTexture(m_camera.pixelWidth, m_camera.pixelHeight, 24);
        var prev = m_camera.targetTexture;
        m_camera.targetTexture = renderTexture;
        m_camera.Render();
        m_camera.targetTexture = prev;
        RenderTexture.active = renderTexture;

        var screenShot = new Texture2D(
            m_camera.pixelWidth,
            m_camera.pixelHeight,
            TextureFormat.RGB24,
            false);
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        var date = DateTime.Now.ToString("yyyyMMdd");

        NativeGallery.SaveImageToGallery(screenShot, "GalleryTest", $"{date}_Growtree.png");
    }
}
