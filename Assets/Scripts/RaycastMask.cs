using UnityEngine;

public class RaycastMask : MonoBehaviour
{
    /// <summary>
    /// マスクの中心からの距離
    /// </summary>
    [Range(0.1f, 5f)]
    public float Distance = 1;

    /// <summary>
    /// RaycastMaskのレイヤー
    /// </summary>
    public int RaycastMaskLayer = 7;

    [SerializeField]
    private GameObject m_colliderPrefab;

    public void AppendMask(Camera camera)
    {
        // インスタンスを作成
        var maskObject = Instantiate(m_colliderPrefab, transform);
        var colliderTransform = maskObject.transform.GetChild(0).transform;

        // 参考：カメラからの距離で求める錐台のサイズ (Unityマニュアル)
        // https://docs.unity3d.com/ja/current/Manual/FrustumSizeAtDistance.html
        var frustumHalfHeight = Distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumHalfWidth = frustumHalfHeight * camera.aspect;

        // 回転を設定
        maskObject.transform.localRotation = camera.transform.localRotation;

        // マスクの距離、サイズを設定
        colliderTransform.localPosition = new Vector3(0, 0, Distance);
        colliderTransform.localScale = new Vector3(frustumHalfWidth, frustumHalfHeight, 1);
    }
}
