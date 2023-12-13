using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.UI;
using OpenCVForUnity.UnityUtils;
using ARFoundationWithOpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.Calib3dModule;
using Unity.Mathematics;

class ProcessImageArgs
{
    public Mat Image;

    public Matrix4x4 ProjectionMatrix;

    public Matrix4x4 CameraToWorldMatrix;
}

[RequireComponent(typeof(LampDetectionHelper))]
[RequireComponent(typeof(ARFoundationCameraToMatHelper))]
public class CameraImageProcessor : MonoBehaviour
{
    LampDetectionHelper m_lampHelper;

    ParticleSystem m_particleSystem;

    ARFoundationCameraToMatHelper m_cameraTextureToMatHelper;

    [SerializeField]
    Camera m_arCamera;

    [SerializeField]
    ARCameraManager m_cameraManager;

    ParticleSystem.Particle[] m_Particles;

    int m_NumParticles;

    static List<Vector3> s_Vertices = new List<Vector3>();
    
    /// <summary>
    /// 円のサイズの閾値
    /// </summary>
    [Range(0.0f, 1000.0f)]
    public double SizeThreshold = 500;

    void Start()
    {
        m_lampHelper = GetComponent<LampDetectionHelper>();
        m_particleSystem = GetComponent<ParticleSystem>();
        m_cameraTextureToMatHelper = GetComponent<ARFoundationCameraToMatHelper>();

        m_cameraTextureToMatHelper.frameMatAcquired += OnFrameMatAcquired;
        m_cameraTextureToMatHelper.Initialize();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        m_cameraTextureToMatHelper.frameMatAcquired -= OnFrameMatAcquired;
        m_cameraTextureToMatHelper.Dispose();
    }

    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log("OnWebCamTextureToMatHelperInitialized");
    }

    public void OnWebCamTextureToMatHelperDisposed()
    {
        Debug.Log("OnWebCamTextureToMatHelperDisposed");
    }

    /// <summary>
    /// Raises the webcam texture to mat helper error occurred event.
    /// </summary>
    /// <param name="errorCode">Error code.</param>
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }

    private void OnFrameMatAcquired(
        Mat mat, 
        Matrix4x4 projectionMatrix, 
        Matrix4x4 cameraToWorldMatrix, 
        XRCameraIntrinsics cameraIntrinsics, 
        long timestamp)
    {
        Mat tmp = new Mat();
        mat.copyTo(tmp);

        ProcessImageArgs args = new ProcessImageArgs
        {
            Image = tmp,
            ProjectionMatrix = projectionMatrix,
            CameraToWorldMatrix = cameraToWorldMatrix,
        };

        // スレッドプール(バックグラウンド)で画像処理
        UniTask.RunOnThreadPool(ProcessImage, args);
    }

    async UniTaskVoid ProcessImage(object input)
    {
        DetectedLampInfo[] lamps;
        Vector2Int imageSize;

        // !!!!![Thread Pool]!!!!!

        ProcessImageArgs args = (ProcessImageArgs)input;

        imageSize = new Vector2Int(args.Image.width(), args.Image.height());
        lamps = m_lampHelper.Run(args.Image);

        args.Image.Dispose();
        
        // !!!!![Main Thread]!!!!!

        await UniTask.SwitchToMainThread(PlayerLoopTiming.Update);

        s_Vertices.Clear();

        if (lamps.Length != 0)
        {
            foreach (var lamp in lamps)
            {
                var cameraPos = new Vector3(lamp.Position.x / imageSize.x - 0.5f, lamp.Position.y / imageSize.y - 0.5f, -10);
                var worldPos = args.CameraToWorldMatrix.MultiplyPoint(cameraPos);

                s_Vertices.Add(worldPos);
            }
        }

        int numParticles = s_Vertices.Count;
        if (m_Particles == null || m_Particles.Length < numParticles)
            m_Particles = new ParticleSystem.Particle[numParticles];

        var color = m_particleSystem.main.startColor.color;
        var size = m_particleSystem.main.startSize.constant;

        for (int i = 0; i < numParticles; ++i)
        {
            m_Particles[i].startColor = color;
            m_Particles[i].startSize = size;
            m_Particles[i].position = s_Vertices[i];
            m_Particles[i].remainingLifetime = 1f;
        }

        // Remove any existing particles by setting remainingLifetime
        // to a negative value.
        for (int i = numParticles; i < m_NumParticles; ++i)
        {
            m_Particles[i].remainingLifetime = -1f;
        }

        m_particleSystem.SetParticles(m_Particles, Math.Max(numParticles, m_NumParticles));
        m_NumParticles = numParticles;
    }
}
