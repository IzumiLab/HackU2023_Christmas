using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeEditorController : MonoBehaviour
{
    [SerializeField]
    private DecoratedTree m_tree;

    [SerializeField]
    private Button m_finishButton;

    void Update()
    {
        m_tree.LoadData(SaveManager.Instance.SaveData.MyTree);
    }

    public void OnCancelButtonClicked()
    {
        SceneManager.LoadScene("Tree");
    }

    public void OnFinishButtonClicked()
    {
        SaveManager.Instance.SaveData.MyTree = m_tree.Data;
        SaveManager.Instance.ForceSave();

        SceneManager.LoadScene("Tree");
    }
}
