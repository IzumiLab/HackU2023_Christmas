using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Fukidashi : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_textMesh;

    private RectTransform m_rectTransform;

    public float Timeout = 10f;

    public string Text
    {
        get { return m_textMesh.text; }
        set { m_textMesh.text = value; }
    }

    float m_time = float.PositiveInfinity;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        m_time += Time.deltaTime;
        float scale = m_time <= Timeout ? 1.0f : 0.0f;
        m_rectTransform.localScale = new Vector3(scale, scale, 1);
    }

    public void Show()
    {
        m_time = 0;
    }

    public void Hide()
    {
        m_time = float.PositiveInfinity;
    }
}
