using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorSelection : MonoBehaviour
{
    public Color32 Color;

    public int Count;

    public Action<bool, Color32> OnValueChangedCallback;

    private Toggle m_toggle;

    [SerializeField]
    private Image m_lampImage;

    [SerializeField]
    private TextMeshProUGUI m_countLabel;

    void Awake()
    {
        m_toggle = GetComponent<Toggle>();
    }

    public void Build(ToggleGroup group)
    {
        m_toggle.group = group;
        m_toggle.interactable = Count > 0;
        m_lampImage.color = Color;
        m_countLabel.text = $"x{Count}";
    }

    public void OnValueChanged(bool state)
    {
        OnValueChangedCallback?.Invoke(state, Color);
    }
}
