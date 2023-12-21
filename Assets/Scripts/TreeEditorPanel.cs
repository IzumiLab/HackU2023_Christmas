using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TreeEditorPanel : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField]
    private TreeEditorUI m_ui;

    private DecoratedTree m_tree;

    private void Awake()
    {
        m_tree = FindObjectOfType<DecoratedTree>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_ui.SelectedDecoration != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition);
            if (m_tree.TryAddDecoration(m_ui.SelectedDecoration, m_ui.SelectedColor, ray, out var _))
            {
                m_ui.MarkDirty();
            }
        }
    }
}
