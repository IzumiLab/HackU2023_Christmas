using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeEditorController : MonoBehaviour
{
    private TreeEditorUI m_ui;

    private DecoratedTree m_tree;

    [SerializeField]
    private Transform m_cameraPivot;

    bool m_dragging = false;

    private void Awake()
    {
        m_ui = FindAnyObjectByType<TreeEditorUI>();
        m_tree = FindAnyObjectByType<DecoratedTree>();
    }

    void Start()
    {
        m_tree.LoadData(SaveManager.Instance.SaveData.MyTree);
    }

    public void OnCancelButtonClick()
    {
        SceneManager.LoadScene("Tree");
    }

    public void OnFinishButtonClick()
    {
        SaveManager.Instance.SaveData.MyTree = m_tree.Data;
        SaveManager.Instance.ForceSave();

        SceneManager.LoadScene("Tree");
    }

    public void OnEditorPanelClick(BaseEventData data)
    {
        var pointerEvent = data as PointerEventData;
        if (m_ui.SelectedDecoration != null && !m_dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(pointerEvent.pointerCurrentRaycast.screenPosition);
            if (m_tree.TryAddDecoration(m_ui.SelectedDecoration, m_ui.SelectedColor, ray, out var _))
            {
                m_ui.MarkDirty();
            }
        }
    }

    public void OnEditorPanelBeginDrag(BaseEventData _)
    {
        m_dragging = true;
    }

    public void OnEditorPanelEndDrag(BaseEventData _)
    {
        m_dragging = false;
    }

    public void OnEditorPanelDrag(BaseEventData data)
    {
        var pointerEvent = data as PointerEventData;
        var delta = pointerEvent.delta;

        m_cameraPivot.Rotate(new Vector3(0, delta.x, 0));
    }
}
