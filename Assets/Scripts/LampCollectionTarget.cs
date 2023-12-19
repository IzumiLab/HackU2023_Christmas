using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LampCollectionTarget : MonoBehaviour
{
    public float MaxScale = 1.1f;

    public float ScaleUpSpeed = 0.1f;

    public float ScaleDownSpeed = 0.2f;

    public RectTransform rectTransform { get; private set; }

    float m_scale = 1.0f;

    bool m_scalingUp = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (m_scalingUp)
        {
            m_scale += dt / ScaleUpSpeed;
            if (m_scale >= MaxScale)
            {
                m_scale = MaxScale;
                m_scalingUp = false;
            }
        }
        else
        {
            m_scale = Mathf.Max(1.0f, m_scale - dt / ScaleDownSpeed);
        }

        rectTransform.localScale = new Vector3(m_scale, m_scale, m_scale);
    }

    public void StartAnimation()
    {
        m_scalingUp = true;
    }
}
