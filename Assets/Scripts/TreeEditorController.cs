using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeEditorController : MonoBehaviour
{
    [SerializeField]
    private Button m_finishButton;

    public void OnCancelButtonClicked()
    {
        SceneManager.LoadScene("Tree");
    }

    public void OnFinishButtonClicked()
    {
        SceneManager.LoadScene("Tree");
    }
}
