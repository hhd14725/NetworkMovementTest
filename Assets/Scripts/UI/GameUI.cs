using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Tooltip("Player Prefab (Resources/Player.prefab)")]
    public GameObject playerPrefab;

    void Awake()
    {
        var handler = FindObjectOfType<SpawnPointsHandler>();
        if (handler != null && playerPrefab != null)
        {
            var spawnList = handler.GetSpawnPoints();
            NetworkManager.Instance.RegisterGameSetup(spawnList, playerPrefab);
        }
    }
}