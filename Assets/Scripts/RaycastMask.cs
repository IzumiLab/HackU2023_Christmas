using UnityEngine;

public class RaycastMask : MonoBehaviour
{
    /// <summary>
    /// �}�X�N�̒��S����̋���
    /// </summary>
    [Range(0.1f, 5f)]
    public float Distance = 1;

    /// <summary>
    /// RaycastMask�̃��C���[
    /// </summary>
    public int RaycastMaskLayer = 7;

    [SerializeField]
    private GameObject m_colliderPrefab;

    public void AppendMask(Camera camera)
    {
        // �C���X�^���X���쐬
        var maskObject = Instantiate(m_colliderPrefab, transform);
        var colliderTransform = maskObject.transform.GetChild(0).transform;

        // �Q�l�F�J��������̋����ŋ��߂鐍��̃T�C�Y (Unity�}�j���A��)
        // https://docs.unity3d.com/ja/current/Manual/FrustumSizeAtDistance.html
        var frustumHalfHeight = Distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumHalfWidth = frustumHalfHeight * camera.aspect;

        // ��]��ݒ�
        maskObject.transform.localRotation = camera.transform.localRotation;

        // �}�X�N�̋����A�T�C�Y��ݒ�
        colliderTransform.localPosition = new Vector3(0, 0, Distance);
        colliderTransform.localScale = new Vector3(frustumHalfWidth, frustumHalfHeight, 1);
    }
}
