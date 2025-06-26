using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Master Client������ Game �� �ε� ���Ŀ� ������ ������ ��ġ�� �ʵ��� ��ֹ�(������)�� ���� �����մϴ�.
/// </summary>
public class ObstacleSpawner : MonoBehaviourPunCallbacks
{
    [Header("Obstacle Settings")]
    [Tooltip("Resources ���� �� ������ �̸� (��: MovingStructure.prefab)")]
    public string obstaclePrefabName = "MovingStructure";
    [Tooltip("������ ��ֹ� ��")] public int obstacleCount = 10;
    [Tooltip("���� ��ġ�� �ʵ��� �� �ּ� �Ÿ�")] public float minDistance = 2f;

    [Header("Spawn Area Bounds")]
    public Vector3 areaMin = new Vector3(-10, -5, -10);
    public Vector3 areaMax = new Vector3(10, 5, 10);

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Awake()
    {
        // Game �� �ε� ���Ŀ� ������ Ʈ����
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Game ���� �����ϰ� ������ Ŭ���̾�Ʈ�� ���� ����
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

            // �� ������ ���� (��� Ŭ���̾�Ʈ�� ���� ID�� ����)
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