using System.Linq;
using UnityEngine;

public static class TransformExtension
{
    public static void DestroyAllChildren(this Transform t)
    {
        foreach (var c in t.GetAllChildren())
        {
            Object.Destroy(c);
        }
    }

    public static GameObject[] GetAllChildren(this Transform t)
    {
        return Enumerable.Range(0, t.childCount).Select(e => t.GetChild(e).gameObject).ToArray();
    }
}