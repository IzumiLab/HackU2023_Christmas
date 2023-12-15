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
using Unity.Collections;
using DataStructures.ViliWonka.KDTree;
using UnityEngine.UIElements;

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
    //------GetComponentから初期化------

    LampDetectionHelper m_lampHelper;

    ParticleSystem m_particleSystem;

    ARFoundationCameraToMatHelper m_cameraTextureToMatHelper;

    //------エディタで設定------

    [SerializeField]
    RaycastMask m_raycastMask;

    //------プロパティ------

    /// <summary>
    /// ランプを配置する距離
    /// </summary>
    public float LampDistance = 5.0f;

    //------

    /// <summary>
    /// ParticleSystem.SetParticlesに送る前のパーティクル
    /// Alloc回数削減のため再利用
    /// </summary>
    List<ParticleSystem.Particle> m_newParticleBuffer = new List<ParticleSystem.Particle>();

    /// <summary>
    /// イルミネーションのランプの位置を保存するKD木
    /// </summary>
    KDTree m_lampKdTree = new KDTree();

    void Start()
    {
        m_lampHelper = GetComponent<LampDetectionHelper>();
        m_particleSystem = GetComponent<ParticleSystem>();
        m_cameraTextureToMatHelper = GetComponent<ARFoundationCameraToMatHelper>();

        m_cameraTextureToMatHelper.frameMatAcquired += OnFrameMatAcquired;
        m_cameraTextureToMatHelper.Initialize();
    }

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

    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }

    void TakeSnapshot(Mat image)
    {
        // ARCameraManagerからカメラを取得
        var camera = m_cameraTextureToMatHelper.GetARCameraManager().GetComponent<Camera>();

        // 画像からランプ位置を検出
        var lamps = m_lampHelper.Run(image);

        m_newParticleBuffer.Clear();

        foreach (var lamp in lamps)
        {
            Ray rayFromCamera = camera.ScreenPointToRay(lamp.Position);

            if (Physics.Raycast(rayFromCamera, LampDistance, m_raycastMask.RaycastMaskLayer))
            {
                // マスクに衝突したらスキップ
                continue;
            }

            var lampWorldPos = rayFromCamera.GetPoint(LampDistance);

            // パーティクルを追加
            m_newParticleBuffer.Add(new ParticleSystem.Particle
            {
                startColor = lamp.Color,
                startSize = Math.Min(lamp.Area * 0.1f, 0.5f),
                position = lampWorldPos,
                remainingLifetime = float.PositiveInfinity
            });
        }

        // バッファをParticleSystemの末尾に挿入
        m_particleSystem.SetParticles(
            m_newParticleBuffer.ToArray(), 
            size: m_newParticleBuffer.Count, 
            offset: m_particleSystem.particleCount
        );

        // KD木を再構築
        var kdTreeBegin = m_lampKdTree.Count;
        m_lampKdTree.SetCount(m_lampKdTree.Count + m_newParticleBuffer.Count);
        for (int i = 0; i < m_newParticleBuffer.Count; i++)
        {
            m_lampKdTree.Points[kdTreeBegin + i] = m_newParticleBuffer[i].position;
        }
        m_lampKdTree.Rebuild();


        // 最後にマスクを追加
        m_raycastMask.AppendMask(camera);
    }

    void OnFrameMatAcquired(
        Mat image, 
        Matrix4x4 projectionMatrix, 
        Matrix4x4 cameraToWorldMatrix, 
        XRCameraIntrinsics cameraIntrinsics, 
        long timestamp)
    {
        var cameraManager = m_cameraTextureToMatHelper.GetARCameraManager();
        var cameraForwardRay = new Ray(cameraManager.transform.position, cameraManager.transform.forward);
        if (!Physics.Raycast(cameraForwardRay, LampDistance, m_raycastMask.RaycastMaskLayer))
        {
            // 向いている方向がマスクの範囲外
            // →まだ撮影していないので撮影

            TakeSnapshot(image);
        }
    }
}
