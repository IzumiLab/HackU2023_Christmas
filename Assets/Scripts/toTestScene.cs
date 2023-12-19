using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToTestScene : MonoBehaviour
{
    public void ButtonClick_toTestScene()
    {
        SceneManager.LoadScene("TestScene");
    }
}
