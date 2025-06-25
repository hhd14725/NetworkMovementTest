using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    private TMP_InputField titleNicknameInput;
    private Button titleConnectBtn;

    private TMP_InputField lobbyRoomNameInput;
    private Button lobbyCreateBtn;
    private Button lobbyRefreshBtn;
    private Button lobbyBackBtn;
    private Transform lobbyContent;
    private GameObject lobbyButtonPrefab;

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
        // 씬 전환 콜백 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 씬 전환 콜백 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 후 처리
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void RegisterTitleUI(TMP_InputField nicknameInput, Button connectBtn)
    {
        titleNicknameInput = nicknameInput;
        titleConnectBtn = connectBtn;
        titleConnectBtn.onClick.AddListener(OnTitleConnect);
        if (PlayerPrefs.HasKey("NickName"))
            titleNicknameInput.text = PlayerPrefs.GetString("NickName");
    }

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

    void OnCreateRoom()
    {
        string rm = string.IsNullOrEmpty(lobbyRoomNameInput.text)
            ? "Room" + Random.Range(1000, 9999)
            : lobbyRoomNameInput.text;
        var options = new RoomOptions
        {
            MaxPlayers = 6,
            EmptyRoomTtl = 0,
            CleanupCacheOnLeave = true
        };
        PhotonNetwork.CreateRoom(rm, options);
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        if (lobbyContent == null || lobbyButtonPrefab == null) return;
        foreach (Transform child in lobbyContent)
            Destroy(child.gameObject);
        foreach (var info in rooms)
        {
            var btn = Instantiate(lobbyButtonPrefab, lobbyContent);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
            btn.GetComponent<Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(info.Name));
        }
    }

    public override void OnJoinedRoom()
    {
        // 모두 같은 씬 전환
        PhotonNetwork.LoadLevel("Game");
    }

    void OnBackToTitle()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Title");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
