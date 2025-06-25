// SpawnPointsHandler.cs
using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHandler : MonoBehaviour
{
    public List<Transform> GetSpawnPoints()
    {
        var list = new List<Transform>();
        foreach (Transform child in transform) list.Add(child);
        return list;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (Transform child in transform)
        {
            Gizmos.DrawSphere(child.position, 0.5f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(child.position + Vector3.up * 0.5f, child.name);
#endif
        }
    }
}
