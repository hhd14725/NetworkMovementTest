using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    private const byte EVT_SPAWN_PLAYER = 1;

    [Tooltip("Resources 폴더의 플레이어 프리팹 이름 (Player.prefab)")]
    public string playerPrefabName = "Player";
    [Tooltip("씬에 배치된 SpawnPointsHandler 컴포넌트")]
    public SpawnPointsHandler spawnHandler;

    private List<Transform> spawnPoints;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && PhotonNetwork.InRoom)
        {
            if (spawnHandler == null)
                spawnHandler = FindObjectOfType<SpawnPointsHandler>();
            if (spawnHandler == null)
                Debug.LogError("SpawnPointsHandler 없음");
            else
                spawnPoints = spawnHandler.GetSpawnPoints();
            Debug.Log($"[GameManager] InRoom: {PhotonNetwork.InRoom}, spawnPoints: {spawnPoints?.Count}");
            SpawnLocalPlayer();
        }
    }

    void SpawnLocalPlayer()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("스폰 포인트 없음");
            return;
        }
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        int idx = (actorNum - 1) % spawnPoints.Count;
        Vector3 pos = spawnPoints[idx].position;
        Quaternion rot = spawnPoints[idx].rotation;

        GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        Debug.Log($"SpawnLocalPlayer for actor {actorNum}");

      
        //var cam = Camera.main;
        //if (cam != null)
        //{
        //    var tpc = cam.GetComponent<ThirdPersonCamera>();
        //    if (tpc != null)
        //    {
        //        tpc.target = localPlayer.transform;
        //        Debug.Log($"Camera target set to {localPlayer.name}");
        //    }
        //    else
        //        Debug.LogError("ThirdPersonCamera 컴포넌트 없음");
        //}
        //else
        //    Debug.LogError("Camera.main 없음");

    }

  

    public override void OnLeftRoom()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.LoadScene("Lobby");
    }
}