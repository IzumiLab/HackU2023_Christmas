using OpenCVForUnity.ObjdetectModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SaveData : ISerializationCallbackReceiver
{
    public Dictionary<Color32, int> CollectedLampCount { get; set; } = new Dictionary<Color32, int>();

    [SerializeField]
    public DecoratedTreeData MyTree = new DecoratedTreeData();

    public bool ARDemoMode = false;

    [SerializeField]
    private List<Color32> CollectedColors;

    [SerializeField]
    private List<int> CollectedColorCounts;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        CollectedLampCount.Clear();
        var e = Enumerable.Range(0, Math.Min(CollectedColors.Count, CollectedColorCounts.Count))
            .Select(i => (CollectedColors[i], CollectedColorCounts[i]));
        foreach ((var key, var value) in e)
        {
            CollectedLampCount.Add(key, value);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        CollectedColors = new ();
        CollectedColorCounts = new ();
        foreach (var kv in CollectedLampCount)
        {
            CollectedColors.Add(kv.Key);
            CollectedColorCounts.Add(kv.Value);
        }
    }
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance = null;

    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                CreateInstance();
            }
            return instance;
        }
    }

    private static void CreateInstance()
    {
        instance = FindObjectOfType<SaveManager>();

        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "SaveManager";
            instance = gameObj.AddComponent<SaveManager>();
            DontDestroyOnLoad(gameObj);
        }
    }

    public static void RequireInstance()
    {
        if (instance == null)
        {
            CreateInstance();
        }
    }

    public SaveData SaveData { get; private set; } = new SaveData();

    public string SavePath { get; private set; }

    public float IOLimitSec { get; set; } = 5.0f;

    /// <summary>
    /// 最後にセーブ/ロードした時間
    /// </summary>
    private float m_lastIOTime = 0;

    /// <summary>
    /// 後でセーブするフラグ
    /// </summary>
    private bool m_querySave = false;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
            return;
        }

        instance = this;
        SavePath = Application.persistentDataPath + "/save.txt";
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void Update()
    {
        if (m_querySave && m_lastIOTime + IOLimitSec <= Time.time)
        {
            ForceSave();
        }
    }

    public void Clear()
    {
        m_querySave = false;
        SaveData = new SaveData();
    }

    public bool Load()
    {
        m_querySave = false;
        m_lastIOTime = Time.time;

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData = JsonUtility.FromJson<SaveData>(json);
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }

        Debug.Log("Loaded!");

        return true;
    }

    public bool Save()
    {
        if (m_querySave || m_lastIOTime + IOLimitSec > Time.time)
        {
            m_querySave = true;
            return true;
        }
        return ForceSave();
    }

    public bool ForceSave()
    {
        m_querySave = false;
        m_lastIOTime = Time.time;

        try
        {
            string json = JsonUtility.ToJson(SaveData);
            File.WriteAllText(SavePath, json);
        }
        catch (IOException)
        {
            return false;
        }

        Debug.Log("Saved!");

        return true;
    }
}
