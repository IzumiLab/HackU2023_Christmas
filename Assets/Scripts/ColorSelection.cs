using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelection : MonoBehaviour
{
    public Color Color;

    public int Count;

    [SerializeField]
    private Selectable m_selectable;

    [SerializeField]
    private Image m_lampImage;

    [SerializeField]
    private TextMeshProUGUI m_countLabel;

    public void Build()
    {
        m_selectable.interactable = Count > 0;
        m_lampImage.color = Color;
        m_countLabel.text = $"x{Count}";
    }
}
