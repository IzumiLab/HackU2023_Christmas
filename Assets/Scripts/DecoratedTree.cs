using OpenCVForUnity.DnnModule;
using OpenCVForUnity.Features2dModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class DecoratedTree : MonoBehaviour
{
    [SerializeField]
    private bool m_loadMyTree = false;

    [SerializeField]
    private Transform m_treeDecorations;

    [SerializeField]
    private Transform m_topDecoration;

    [SerializeField]
    private MeshCollider m_treeCollider;

    [SerializeField]
    private TreeDecorationDictionary m_decorationDic;

    public Transform TreeDecorationParent { get => m_treeDecorations; }

    public Transform TopDecorationParent { get => m_topDecoration; }

    public DecoratedTreeData Data { get; private set; } = new DecoratedTreeData();

    void Start()
    {
        if (m_loadMyTree)
        {
            LoadData(SaveManager.Instance.SaveData.MyTree);
        }
    }

    public bool TryAddDecoration(TreeDecoration prefab, Color color, Ray ray, out TreeDecoration instance)
    {
        if (m_treeCollider.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 tangent = Vector3.Cross(hit.normal, Vector3.up);
            Vector3 binormal = Vector3.Cross(tangent, hit.normal);

            instance = AddDecoration(prefab, hit.point, Quaternion.FromToRotation(Vector3.up, binormal), color);

            return true;
        }

        instance = null;
        return false;
    }

    public void LoadData(DecoratedTreeData data)
    {
        if (data == null) throw new ArgumentNullException("data");

        Data = data;

        // 削除
        foreach (Transform child in m_treeDecorations.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in m_topDecoration.transform)
        {
            Destroy(child.gameObject);
        }

        // 構築
        foreach (var decoration in data.Decorations)
        {
            CreateDecorationInstance(decoration);
        }
    }

    private TreeDecoration CreateDecorationInstance(TreeDecorationData data)
    {
        // プレハブを検索
        TreeDecoration prefab;
        if (!m_decorationDic.Data.TryGetValue(data.TypeName, out prefab))
        {
            // 見つからなかったらスキップ
            return null;
        }

        // インスタンスを作成
        var instance = Instantiate(prefab, m_treeDecorations.TransformPoint(data.LocalPosition), Quaternion.identity, m_treeDecorations);
        instance.transform.localRotation = data.LocalRotation;
        instance.Data = data;

        // 色を設定
        instance.CallSetColorCallback(data.Color);

        return instance;
    }

    private TreeDecoration AddDecoration(TreeDecoration prefab, Vector3 position, Quaternion rotation, Color color)
    {
        // インスタンスの初期化
        var instance = Instantiate(prefab, position, rotation, m_treeDecorations);
        TreeDecorationData newData = new TreeDecorationData
        {
            TypeName = instance.TypeName,
            Color = color
        };
        instance.transform.GetLocalPositionAndRotation(out newData.LocalPosition, out newData.LocalRotation);
        instance.Data = newData;

        // 色を設定
        instance.CallSetColorCallback(color);

        // Dataに反映
        Data.Decorations.Add(newData);

        return instance;
    }
}
