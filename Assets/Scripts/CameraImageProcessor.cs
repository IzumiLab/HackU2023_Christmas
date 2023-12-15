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
    //------GetComponent���珉����------

    LampDetectionHelper m_lampHelper;

    ParticleSystem m_particleSystem;

    ARFoundationCameraToMatHelper m_cameraTextureToMatHelper;

    //------�G�f�B�^�Őݒ�------

    [SerializeField]
    RaycastMask m_raycastMask;

    //------�v���p�e�B------

    /// <summary>
    /// �����v��z�u���鋗��
    /// </summary>
    public float LampDistance = 5.0f;

    //------

    /// <summary>
    /// ParticleSystem.SetParticles�ɑ���O�̃p�[�e�B�N��
    /// Alloc�񐔍팸�̂��ߍė��p
    /// </summary>
    List<ParticleSystem.Particle> m_newParticleBuffer = new List<ParticleSystem.Particle>();

    /// <summary>
    /// KDTree.Build�ɑ���O�̍��W
    /// Alloc�񐔍팸�̂��ߍė��p
    /// </summary>
    List<Vector3> m_newKdNodes = new List<Vector3>();

    /// <summary>
    /// �C���~�l�[�V�����̃����v�̈ʒu��ۑ�����KD��
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
        // ARCameraManager����J�������擾
        var camera = m_cameraTextureToMatHelper.GetARCameraManager().GetComponent<Camera>();

        // �摜���烉���v�ʒu�����o
        var lamps = m_lampHelper.Run(image);

        m_newParticleBuffer.Clear();
        m_newKdNodes.Clear();

        foreach (var lamp in lamps)
        {
            Ray rayFromCamera = camera.ScreenPointToRay(lamp.Position);

            if (Physics.Raycast(rayFromCamera, LampDistance, m_raycastMask.RaycastMaskLayer))
            {
                // �}�X�N�ɏՓ˂�����X�L�b�v
                continue;
            }

            var lampWorldPos = rayFromCamera.GetPoint(LampDistance);

            // �p�[�e�B�N����ǉ�
            m_newParticleBuffer.Add(new ParticleSystem.Particle
            {
                startColor = lamp.Color,
                startSize = lamp.Area * 0.01f,
                position = lampWorldPos,
                remainingLifetime = float.PositiveInfinity
            });

            // KD�؂ɍ��W��ǉ�
            m_newKdNodes.Add(lampWorldPos);
        }

        // �o�b�t�@��ParticleSystem�̖����ɑ}��
        m_particleSystem.SetParticles(
            m_newParticleBuffer.ToArray(), 
            size: m_newParticleBuffer.Count, 
            offset: m_particleSystem.particleCount
        );

        // KD�؂��č\�z
        m_lampKdTree.Build(m_newKdNodes);

        // �p�[�e�B�N����KD�؂̃m�[�h�̃C���f�b�N�X�͘A�����Ă���K�v������̂ŔO�̂���
        Debug.Assert(m_particleSystem.particleCount == m_lampKdTree.Count);

        // �Ō�Ƀ}�X�N��ǉ�
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
            // �����Ă���������}�X�N�͈̔͊O
            // ���܂��B�e���Ă��Ȃ��̂ŎB�e

            TakeSnapshot(image);
        }
    }
}
