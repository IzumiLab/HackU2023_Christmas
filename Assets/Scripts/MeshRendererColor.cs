using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MeshRendererColor : MonoBehaviour
{
    MeshRenderer m_renderer;

    Color m_color;

    public float SaturationMultiplier = 0.0f;

    public Color MeshColor
    {
        get => m_color;
        set
        {
            m_color = value;
            if (SaturationMultiplier == 0.0f)
            {
                m_renderer.material.color = m_color;
            }
            else
            {
                Color.RGBToHSV(m_color, out float h, out float s, out float v);
                m_renderer.material.color = Color.HSVToRGB(h, Mathf.Clamp01(s * SaturationMultiplier), v);
            }
        }
    }

    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_color = m_renderer.material.color;
    }
}
