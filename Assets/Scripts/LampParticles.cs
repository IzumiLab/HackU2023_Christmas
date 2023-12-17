using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using ARFoundationWithOpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils.Helper;
using System.Linq;
using System.Buffers;
using DataStructures.ViliWonka.KDTree;

public struct Lamp
{
    public Vector3 Position;

    public float Area;

    public Color Color;

    public bool Removed;
}

[RequireComponent(typeof(LampDetectionHelper))]
[RequireComponent(typeof(ARFoundationCameraToMatHelper))]
public class LampParticles : MonoBehaviour
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
    static ParticleSystem.Particle[] m_newParticleBuffer = new ParticleSystem.Particle[0];

    /// <summary>
    /// ランプの状態
    /// </summary>
    static List<Lamp> m_lamps = new List<Lamp>();

    /// <summary>
    /// イルミネーションのランプを保存するKD木
    /// </summary>
    KDTree m_lampKdTree = new KDTree();

    /// <summary>
    /// 検索時に使うクエリ
    /// </summary>
    KDQuery m_lampKdQuery;

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

    int m_particleCount = -1;

    private void Update()
    {
        if (m_particleSystem.particleCount != m_particleCount)
        {
            m_particleCount = m_particleSystem.particleCount;
            Debug.Log(m_particleCount);
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (m_lampKdQuery != null)
        {
            m_lampKdQuery.DrawLastQuery();
        }
    }

    public Camera GetCamera()
    {
        // ARCameraManagerからカメラを取得
        return m_cameraTextureToMatHelper.GetARCameraManager().GetComponent<Camera>();
    }

    public Lamp[] RemoveLamp(Vector2 screenPos, float radius)
    {
        var camera = GetCamera();

        // KD木を検索
        var queryCenter = camera.ScreenPointToRay(screenPos).GetPoint(LampDistance);
        
        List<int> lampIndicies = new List<int>();
        m_lampKdQuery ??= new KDQuery();
        m_lampKdQuery.Radius(m_lampKdTree, queryCenter, radius, lampIndicies);

        // 削除されたランプは無視
        lampIndicies.RemoveAll(i => m_lamps[i].Removed);

        // すでに検索結果が空の場合は空配列を返す
        if (lampIndicies.Count == 0)
        {
            return new Lamp[0];
        }

        // ノードの論理削除
        foreach (var index in lampIndicies)
        {
            Lamp lamp = m_lamps[index];
            lamp.Removed = true;
            m_lamps[index] = lamp;
        }

        // パーティクルを再構築
        // TODO: より効率の良いパーティクル削除を模索

        m_particleSystem.SetParticles(m_lamps.Where(lamp => !lamp.Removed).Select(lamp => CreateParticle(lamp)).ToArray());

        return lampIndicies.Select(i => m_lamps[i]).ToArray();
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
        var camera = GetCamera();

        // 画像からランプ位置を検出
        var detectedLamps = m_lampHelper.Run(image);

        // 必要に応じてバッファの大きさを変更
        if (detectedLamps.Length > m_newParticleBuffer.Length)
        {
            Array.Resize(ref m_newParticleBuffer, detectedLamps.Length);
        }

        var newLampBegin = m_lamps.Count;
        var newLampCount = 0;
        foreach (var lampInfo in detectedLamps)
        {
            Ray rayFromCamera = camera.ScreenPointToRay(lampInfo.Position);

            // マスクに衝突したらスキップ
            if (Physics.Raycast(rayFromCamera, LampDistance, m_raycastMask.RaycastMaskLayer))
            {
                continue;
            }

            var lampWorldPos = rayFromCamera.GetPoint(LampDistance);
            
            var lamp = new Lamp
            {
                Position = lampWorldPos,
                Area = lampInfo.Area,
                Color = lampInfo.Color,
                Removed = false,
            };

            // m_lampsとm_newParticleBufferに反映
            m_lamps.Add(lamp);
            m_newParticleBuffer[newLampCount] = CreateParticle(lamp);

            newLampCount++;
        }

        // m_lampsを利用してKD木の更新
        m_lampKdTree.SetCount(m_lamps.Count);
        foreach (var lampIdx in Enumerable.Range(newLampBegin, newLampCount))
        {
            m_lampKdTree.Points[lampIdx] = m_lamps[lampIdx].Position;
        }
        m_lampKdTree.Rebuild();

        // バッファをParticleSystemの末尾に挿入
        m_particleSystem.SetParticles(
            m_newParticleBuffer,
            size: newLampCount,
            offset: m_particleSystem.particleCount
        );

        // 最後にマスクを追加
        m_raycastMask.CreateMask(camera);
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

    static ParticleSystem.Particle CreateParticle(Lamp lamp) => new ParticleSystem.Particle
    {
        startColor = lamp.Color,
        startSize = Math.Min(lamp.Area * 0.1f, 0.5f),
        position = lamp.Position,
    };
}
