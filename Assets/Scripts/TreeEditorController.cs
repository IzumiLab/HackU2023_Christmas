using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeEditorController : MonoBehaviour
{
    private DecoratedTree m_tree;

    private void Awake()
    {
        m_tree = FindAnyObjectByType<DecoratedTree>();
    }

    void Update()
    {
        m_tree.LoadData(SaveManager.Instance.SaveData.MyTree);
    }
}
