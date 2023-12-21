using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeEditorUI : MonoBehaviour
{
    [SerializeField]
    private Button m_finishButton;

    [SerializeField]
    private VerticalLayoutGroup m_header;

    [SerializeField]
    private VerticalLayoutGroup m_footer;

    [SerializeField]
    private ColorSelection m_colorSelectionPrefab;

    [SerializeField]
    private TreeDecorationSelection m_decorationSelectionPrefab;

    [SerializeField]
    private ToggleGroup m_colorSelectionGroup;

    [SerializeField]
    private ToggleGroup m_decorationSelectionGroup;

    [SerializeField]
    private TreeDecorationDictionary m_decorationDic;

    public Color32 SelectedColor { get; private set; }

    public TreeDecoration SelectedDecoration { get; private set; }

    public UnityEvent<Color32, TreeDecoration> OnSelectionChanged;

    public void BuildAll()
    {
        BuildColorSelection();
        BuildDecorationSelection();
    }

    public void BuildColorSelection()
    {
        foreach (Transform child in m_colorSelectionGroup.transform)
        {
            Destroy(child.gameObject);
        }

        var save = SaveManager.Instance.SaveData;
        foreach (var (color32, count) in save.CollectedLampCount.OrderByDescending(kv => kv.Value))
        {
            var selection = Instantiate(m_colorSelectionPrefab, m_colorSelectionGroup.transform);
            selection.Color = color32;
            selection.Count = count;
            selection.OnValueChangedCallback = OnColorChanged;
            selection.Toggle.isOn = color32.CompareRGB(SelectedColor);
            selection.Build(m_colorSelectionGroup);
        }
    }

    public void BuildDecorationSelection()
    {
        foreach (Transform child in m_decorationSelectionGroup.transform)
        {
            Destroy(child.gameObject);
        }

        var save = SaveManager.Instance.SaveData;
        var balance = save.CollectedLampCount.GetValueOrDefault(SelectedColor, 0);
        foreach (var decoration in m_decorationDic.Data.Values.OrderBy(d => d.TypeCost))
        {
            var selection = Instantiate(m_decorationSelectionPrefab, m_decorationSelectionGroup.transform);
            var canMakePurchase = decoration.TypeCost <= balance;

            selection.Data = decoration;
            selection.CanMakePurchase = canMakePurchase;
            selection.OnValueChangedCallback = OnDecorationChanged;

            if (decoration == SelectedDecoration)
            {
                if (canMakePurchase)
                {
                    selection.Toggle.isOn = true;
                }
                else
                {
                    SelectedDecoration = null;
                    OnSelectionChanged?.Invoke(SelectedColor, SelectedDecoration);
                }
            }

            selection.Build(m_decorationSelectionGroup);
        }
    }

    void OnColorChanged(bool state, Color32 color)
    {
        if (state && !color.CompareRGB(SelectedColor))
        {
            SelectedDecoration = null;
            SelectedColor = color;
            BuildDecorationSelection();
        }

        OnSelectionChanged?.Invoke(SelectedColor, SelectedDecoration);
    }

    void OnDecorationChanged(bool state, TreeDecoration decoration)
    {
        if (state)
        {
            SelectedDecoration = decoration;
        }
        else if (SelectedDecoration == decoration)
        {
            SelectedDecoration = null;
        }

        OnSelectionChanged?.Invoke(SelectedColor, SelectedDecoration);
    }

    public void MarkDirty()
    {
        m_finishButton.interactable = true;
    }

    void Start()
    {
        BuildAll();
    }

    void Update()
    {
        int screenHeight = Screen.height;
        Rect safeArea = Screen.safeArea;

        var headerPadding = m_header.padding;
        var footerPadding = m_footer.padding;

        headerPadding.top = screenHeight - (int)safeArea.yMax;
        footerPadding.bottom = (int)safeArea.yMin;

        m_header.padding = headerPadding;
        m_footer.padding = footerPadding;
    }
}
