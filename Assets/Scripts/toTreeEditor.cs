using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToTreeEditor : MonoBehaviour
{
    public void ButtonClick_toTreeEditor()
    {
        SceneManager.LoadScene("TreeEditor");
    }
}
