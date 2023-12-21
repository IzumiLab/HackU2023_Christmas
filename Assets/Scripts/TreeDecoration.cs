using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TreeDecoration : MonoBehaviour
{
    [SerializeField]
    private string m_name;

    [SerializeField]
    private Sprite m_largeImage;

    [SerializeField]
    private Sprite m_smallImage;

    [SerializeField]
    private int m_cost;

    [SerializeField]
    private UnityEvent<Color> m_setColorCallback;

    public string TypeName { get => m_name; }

    public Sprite TypeLargeImage { get => m_largeImage; }

    public Sprite TypeSmallImage { get => m_smallImage; }

    public int TypeCost { get => m_cost; }

    public TreeDecorationData Data;

    public void CallSetColorCallback(Color color)
    {
        m_setColorCallback?.Invoke(color);
    }
}
