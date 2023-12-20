using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDecorationDictionary : MonoBehaviour
{
    [SerializeField]
    private List<TreeDecoration> m_prefabs;

    public Dictionary<string, TreeDecoration> Data { get; private set; }

    void Awake()
    {
        Data = new Dictionary<string, TreeDecoration>();
        foreach (var prefab in m_prefabs)
        {
            Data.Add(prefab.Data.TypeName, prefab);
        }
    }
}
