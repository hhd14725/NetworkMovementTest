using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHandler : MonoBehaviour
{
    // 자식 Transform들을 리스트로 반환
    public List<Transform> GetSpawnPoints()
    {
        var list = new List<Transform>();
        foreach (Transform child in transform)
        {
            list.Add(child);
        }
        return list;
    }

    // 씬 뷰에서 스폰 포인트를 시각화하기 위한 Gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        // 자식 오브젝트마다 구체를 그리고 이름을 표시
        foreach (Transform child in transform)
        {
            Gizmos.DrawSphere(child.position, 0.5f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                child.position + Vector3.up * 0.5f,
                child.name
            );
#endif
        }
    }
}