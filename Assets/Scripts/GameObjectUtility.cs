using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectUtility
{
    public static GameObject AssureEmptyGameObjectChild(
        this Transform transform,
        string name)
    {
        var existing = transform.Find(name);
        if (existing != null)
        {
            GameObject.DestroyImmediate(existing.gameObject);
            existing = null;
        }

        var empty = new GameObject(name);
        empty.transform.parent = transform;
        return empty;
    }
}
