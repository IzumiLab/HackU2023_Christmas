using Cysharp.Threading.Tasks;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct DetectedLampInfo
{
    public Vector2 Position;

    public float Area;

    public Color Color;
}

public class LampDetectionHelper : MonoBehaviour
{
    /// <summary>
    /// イルミネーションの閾値
    /// </summary>
    [Range(0, 255)]
    public int ValueThreshold = 200;

    [Space(10)]

    [SerializeField]
    RawImage m_debugImage;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float m_debugImageScale = 1 / 3f;

    Texture2D m_debugTexture;

    Mat m_grayscaledMat = new Mat();

    Mat m_burredMat = new Mat();

    Mat m_illuminationMask = new Mat();

    Mat m_labelPattern = new Mat();

    public DetectedLampInfo[] Run(Mat rgb24Image)
    {

        Imgproc.cvtColor(rgb24Image, m_grayscaledMat, Imgproc.COLOR_RGB2GRAY);
        Imgproc.GaussianBlur(m_grayscaledMat, m_burredMat, new(11, 11), 0);
        Imgproc.threshold(m_burredMat, m_illuminationMask, ValueThreshold, 255, Imgproc.THRESH_BINARY);

        using var stats = new Mat();
        using var centroids = new Mat();

        int count = Imgproc.connectedComponentsWithStats(
            m_illuminationMask,
            m_labelPattern,
            stats,
            centroids,
            4
        );

        if (m_debugImage != null)
        {
            // UpdateDebugImageによってDisposeする
            Mat internalImg = new Mat(); 

            Mat source = m_illuminationMask;
            Size scaledSize = new Size(source.width() * m_debugImageScale, source.height() * m_debugImageScale);

            Imgproc.resize(source, internalImg, scaledSize, 0, 0, Imgproc.INTER_NEAREST);

            UpdateDebugImage(internalImg).Forget();
        }

        var lamps = Enumerable.Range(1, count - 1)
            .Select(i => new DetectedLampInfo
            {
                Position = new(
                    (float)centroids.get(i, 0)[0], 
                    rgb24Image.height() - (float)centroids.get(i, 1)[0] // Unityの座標系に変換
                ),
                Area = (float)stats.get(i, Imgproc.CC_STAT_AREA)[0],
                Color = GetColorAtCoordinate(rgb24Image, (int)centroids.get(i, 0)[0], (int)centroids.get(i, 1)[0])
            })
            .ToArray();

        return lamps;
    }

    private Color GetColorAtCoordinate(Mat image, int x, int y)
    {
        x = Mathf.Clamp(x, 0, image.width() - 1);
        y = Mathf.Clamp(y, 0, image.height() - 1);

        double[] colorValues = image.get(y, x);

        Color color = new Color((float)colorValues[0] / 255f, (float)colorValues[1] / 255f, (float)colorValues[2] / 255f);

        return color;
    }


    void OnDestroy()
    {
        DisposeWhenNonNull(m_grayscaledMat);
        DisposeWhenNonNull(m_burredMat);
        DisposeWhenNonNull(m_illuminationMask);
        DisposeWhenNonNull(m_labelPattern);
    }

    static void DisposeWhenNonNull(IDisposable obj)
    {
        if (obj != null) obj.Dispose();
    }

    async UniTaskVoid UpdateDebugImage(Mat image)
    {
        await UniTask.SwitchToMainThread();

        var imageSize = new Vector2Int(image.width(), image.height());

        if (m_debugTexture == null)
        {
            m_debugTexture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.R8, false);
        }
        else if (m_debugTexture.width != imageSize.x || m_debugTexture.height != imageSize.y)
        {
            m_debugTexture.Reinitialize(imageSize.x, imageSize.y, TextureFormat.R8, false);
        }

        Utils.fastMatToTexture2D(image, m_debugTexture);
        m_debugImage.rectTransform.sizeDelta = new Vector2(image.width(), image.height());
        m_debugImage.texture = m_debugTexture;

        image.Dispose();
    }
}
