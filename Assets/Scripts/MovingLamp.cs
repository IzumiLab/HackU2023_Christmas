using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class MovingLamp : MonoBehaviour
{
    public float MoveDuration = 0.3f;

    public float FadeInDuration = 0.02f;

    LampCollectionTarget m_target;

    RectTransform m_rectTransform;

    Image m_image;

    Vector2 m_startPos;

    float m_time;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_image = GetComponent<Image>();
    }

    void Update()
    {
        m_time += Time.deltaTime;

        float alphaT;
        float posT;

        if (m_time < 0)
        {
            return;
        }
        else if (m_time < FadeInDuration)
        {
            alphaT = m_time / FadeInDuration;
            posT = 0;
        }
        else if (m_time <= FadeInDuration + MoveDuration)
        {
            alphaT = 1;
            posT = Easing.InCirc((m_time - FadeInDuration) / MoveDuration);
        }
        else
        {
            m_target.StartAnimation();
            Destroy(gameObject);
            return;
        }

        Color color = m_image.color;
        color.a = alphaT;
        m_image.color = color;

        m_rectTransform.anchoredPosition = Vector2.Lerp(m_startPos, m_target.rectTransform.position, posT);
    }

    public void Initialise(Vector2 startPosition, LampCollectionTarget target, float delay, Lamp lamp)
    {
        m_startPos = startPosition;
        m_target = target;
        m_time = -Mathf.Max(delay, 0);

        Color floatColor = lamp.Color;
        m_image.color = new Color(
            floatColor.r,
            floatColor.g, 
            floatColor.b, 
            a: delay <= 0.0f ? 1.0f : 0.0f
        );
        m_rectTransform.anchoredPosition = startPosition;
    }
}
