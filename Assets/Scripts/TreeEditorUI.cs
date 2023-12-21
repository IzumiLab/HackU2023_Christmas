using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TreeEditorUI : MonoBehaviour
{
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

    private Color32? m_selectedColor;

    private string m_selectedDecoration = null;

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

        var save = SaveManager.Instance;
        foreach (var (color32, count) in save.SaveData.CollectedLampCount.OrderByDescending(kv => kv.Value))
        {
            var selection = Instantiate(m_colorSelectionPrefab, m_colorSelectionGroup.transform);
            selection.Color = color32;
            selection.Count = count;
            selection.Build(m_colorSelectionGroup);
        }
    }

    public void BuildDecorationSelection()
    {
        foreach (Transform child in m_decorationSelectionGroup.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var decoration in m_decorationDic.Data.Values)
        {
            var selection = Instantiate(m_decorationSelectionPrefab, m_decorationSelectionGroup.transform);
            selection.Data = decoration;
            selection.CanPurchase = true;
            selection.Build(m_decorationSelectionGroup);
        }
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
