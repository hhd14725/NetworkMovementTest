using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    // Title UI
    private TMP_InputField titleNicknameInput;

    // Lobby UI
    private TMP_InputField lobbyRoomNameInput;
    private Button lobbyCreateBtn;
    private Button lobbyRefreshBtn;
    private Button lobbyBackBtn;
    private Transform lobbyContent;
    private GameObject lobbyButtonPrefab;

    // Game setup
    private List<Transform> spawnPoints;
    private GameObject playerPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Title": break;
            case "Lobby": PhotonNetwork.JoinLobby(); break;
            case "Game": if (PhotonNetwork.InRoom) SpawnPlayer(); break;
        }
    }

    // Registration methods
    public void RegisterTitleUI(TMP_InputField nicknameInput, Button connectBtn)
    {
        titleNicknameInput = nicknameInput;
        connectBtn.onClick.AddListener(OnTitleConnect);
        // 이전 닉네임 로드
        if (PlayerPrefs.HasKey("NickName"))
            titleNicknameInput.text = PlayerPrefs.GetString("NickName");
    }

    public void RegisterLobbyUI(
        TMP_InputField roomInput,
        Button createBtn,
        Button refreshBtn,
        Button backBtn,
        Transform content,
        GameObject buttonPrefab)
    {
        lobbyRoomNameInput = roomInput;
        lobbyCreateBtn = createBtn;
        lobbyRefreshBtn = refreshBtn;
        lobbyBackBtn = backBtn;
        lobbyContent = content;
        lobbyButtonPrefab = buttonPrefab;

        lobbyCreateBtn.onClick.AddListener(OnCreateRoom);
        lobbyRefreshBtn.onClick.AddListener(() => PhotonNetwork.JoinLobby());
        lobbyBackBtn.onClick.AddListener(OnBackToTitle);
    }

    public void RegisterGameSetup(List<Transform> spawns, GameObject prefab)
    {
        spawnPoints = spawns;
        playerPrefab = prefab;
    }

    // Title callback
    void OnTitleConnect()
    {
        string nick = string.IsNullOrEmpty(titleNicknameInput.text)
            ? "Player" + Random.Range(1000, 9999)
            : titleNicknameInput.text;
        PhotonNetwork.NickName = nick;
        PlayerPrefs.SetString("NickName", nick);
        PlayerPrefs.Save();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }

    // Lobby callbacks
    void OnCreateRoom()
    {
        string rm = string.IsNullOrEmpty(lobbyRoomNameInput.text)
            ? "Room" + Random.Range(1000, 9999)
            : lobbyRoomNameInput.text;
        PhotonNetwork.CreateRoom(rm, new RoomOptions { MaxPlayers = 6 });
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        if (lobbyContent == null || lobbyButtonPrefab == null) return;
        foreach (Transform c in lobbyContent) Destroy(c.gameObject);
        foreach (var info in rooms)
        {
            var btn = Instantiate(lobbyButtonPrefab, lobbyContent);
            btn.GetComponentInChildren<Text>().text =
                $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
            btn.GetComponent<Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(info.Name));
        }
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Game");
    }

    void OnBackToTitle()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Title");
    }

    // Game callbacks
    void SpawnPlayer()
    {
        if (spawnPoints == null || spawnPoints.Count == 0 || playerPrefab == null) return;
        // ActorNumber 기반으로 인덱스 계산 (겹치지 않음)
        int actorIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        int idx = actorIndex % spawnPoints.Count;
        Transform sp = spawnPoints[idx];
        PhotonNetwork.Instantiate(
            playerPrefab.name,
            sp.position,
            sp.rotation);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}