using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class TreeDecorationSelection : MonoBehaviour
{
    public bool CanMakePurchase = true;

    public TreeDecoration Data;

    public Action<bool, TreeDecoration> OnValueChangedCallback;

    private Toggle m_toggle;

    [SerializeField]
    private Color m_disabledCostLabelColor;

    [SerializeField]
    private Image m_image;

    [SerializeField]
    private TextMeshProUGUI m_nameLabel;

    [SerializeField]
    private TextMeshProUGUI m_costLabel;

    public Toggle Toggle { get => m_toggle; }

    void Awake()
    {
        m_toggle = GetComponent<Toggle>();
    }

    public void Build(ToggleGroup group)
    {
        m_toggle.group = group;
        m_toggle.interactable = CanMakePurchase;
        m_costLabel.color = CanMakePurchase ? Color.white : m_disabledCostLabelColor;

        if (Data != null)
        {
            m_image.sprite = Data.TypeLargeImage;
            m_nameLabel.text = Data.TypeName;
            m_costLabel.text = Data.TypeCost.ToString();
        }
    }

    public void OnValueChanged(bool state)
    {
        OnValueChangedCallback?.Invoke(state, Data);
    }
}
