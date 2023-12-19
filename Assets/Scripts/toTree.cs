using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToTree : MonoBehaviour
{
    public void ButtonClick_toTree()
    {
        SceneManager.LoadScene("Tree");
    }
}
