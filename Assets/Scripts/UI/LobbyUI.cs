using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public TMP_InputField roomNameInput;
    public Button createButton;
    public Button refreshButton;
    public Button backButton;
    public Transform roomListContent;
    public GameObject roomButtonPrefab;

    void Awake()
    {
        NetworkManager.Instance.RegisterLobbyUI(
            roomNameInput,
            createButton,
            refreshButton,
            backButton,
            roomListContent,
            roomButtonPrefab);
    }
}