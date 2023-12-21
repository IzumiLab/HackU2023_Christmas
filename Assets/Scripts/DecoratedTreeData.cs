using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TreeDecorationData
{
    public string TypeName;

    public Vector3 LocalPosition;

    public Quaternion LocalRotation;

    public Color Color;
}

[Serializable]
public class DecoratedTreeData
{
    public List<TreeDecorationData> Decorations = new List<TreeDecorationData>();
}
