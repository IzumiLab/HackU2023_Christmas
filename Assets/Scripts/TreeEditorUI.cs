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
    private Transform m_colorSelectionArea;

    [SerializeField]
    private Transform m_decorationSelectionArea;

    [SerializeField]
    private TreeDecorationDictionary m_decorationDic;

    public void Build()
    {
        //削除

        foreach (Transform child in m_colorSelectionArea)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in m_decorationSelectionArea)
        {
            Destroy(child.gameObject);
        }

        // ColorSelectionを作成

        var save = SaveManager.Instance;
        foreach (var (color32, count) in save.SaveData.CollectedLampCount.OrderByDescending(kv => kv.Value))
        {
            var selection = Instantiate(m_colorSelectionPrefab, m_colorSelectionArea);
            selection.Color = color32;
            selection.Count = count;
            selection.Build();
        }

        // TreeDecorationSelectionを作成

        foreach (var decoration in m_decorationDic.Data.Values)
        {
            var selection = Instantiate(m_decorationSelectionPrefab, m_decorationSelectionArea);
            selection.Data = decoration;
            selection.CanPurchase = true;
            selection.Build();
        }
    }

    void Start()
    {
        Build();
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
