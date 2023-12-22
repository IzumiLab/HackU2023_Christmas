using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [SerializeField]
    private Button ToggleARDemoModeButton;

    public void BuildUI()
    {
        ToggleARDemoModeButton.GetComponentInChildren<TextMeshProUGUI>().text = $"ARデモモード：{(SaveManager.Instance.SaveData.ARDemoMode ? "ON" : "OFF")}";
    }

    public void ToggleARDemoMode()
    {
        SaveManager.Instance.SaveData.ARDemoMode = !SaveManager.Instance.SaveData.ARDemoMode;
        SaveManager.Instance.ForceSave();
        BuildUI();
    }

    public void DeleteSaveData()
    {
        SaveManager.Instance.Clear();
        SaveManager.Instance.ForceSave();
        BuildUI();
    }

    public void CloseMenu()
    {
        SceneManager.UnloadSceneAsync("DebugMenuScene");
    }

    void Start()
    {
        BuildUI();
    }
}
