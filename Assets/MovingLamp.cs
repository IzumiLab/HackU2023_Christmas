using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class MovingLamp : MonoBehaviour
{
    public float Duration = 0.4f;

    RectTransform m_rectTransform;

    Vector2 m_startPos;

    Vector2 m_targetPos;

    float m_time = 0;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        m_time += Time.deltaTime;
        if (m_time > Duration)
        {
            Destroy(gameObject);
            return;
        }

        float t = Easing.InCirc(m_time / Duration);
        m_rectTransform.anchoredPosition = Vector2.Lerp(m_startPos, m_targetPos, t);
    }

    public void Initialise(Vector2 startPosition, RectTransform target, Lamp lamp)
    {
        m_rectTransform.anchoredPosition = startPosition;
        m_startPos = startPosition;

        m_targetPos = target.position;

        GetComponent<Image>().color = lamp.Color;
    }
}
