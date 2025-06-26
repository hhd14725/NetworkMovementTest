using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Master Client에서만 Game 씬 로드 직후에 지정된 영역에 겹치지 않도록 장애물(부유물)을 랜덤 스폰합니다.
/// </summary>
public class ObstacleSpawner : MonoBehaviourPunCallbacks
{
    [Header("Obstacle Settings")]
    [Tooltip("Resources 폴더 내 프리팹 이름 (예: MovingStructure.prefab)")]
    public string obstaclePrefabName = "MovingStructure";
    [Tooltip("생성할 장애물 수")] public int obstacleCount = 10;
    [Tooltip("서로 겹치지 않도록 할 최소 거리")] public float minDistance = 2f;

    [Header("Spawn Area Bounds")]
    public Vector3 areaMin = new Vector3(-10, -5, -10);
    public Vector3 areaMax = new Vector3(10, 5, 10);

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Awake()
    {
        // Game 씬 로드 직후에 스폰을 트리거
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Game 씬에 진입하고 마스터 클라이언트일 때만 스폰
        if (scene.name == "Game" && PhotonNetwork.IsMasterClient)
        {
            SpawnObstacles();
        }
    }

    private void SpawnObstacles()
    {
        int spawned = 0;
        int attempts = 0;
        while (spawned < obstacleCount && attempts < obstacleCount * 10)
        {
            attempts++;
            Vector3 candidate = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                Random.Range(areaMin.z, areaMax.z)
            );

            bool valid = true;
            foreach (var pos in spawnedPositions)
            {
                if (Vector3.Distance(pos, candidate) < minDistance)
                {
                    valid = false;
                    break;
                }
            }
            if (!valid) continue;

            // 룸 단위로 생성 (모든 클라이언트에 동일 ID로 스폰)
            PhotonNetwork.InstantiateRoomObject(
                obstaclePrefabName,
                candidate,
                Quaternion.identity,
                0
            );

            spawnedPositions.Add(candidate);
            spawned++;
        }
    }
}