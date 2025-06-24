using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public Button connectButton;

    void Awake()
    {
        // NetworkManager�� UI ���
        NetworkManager.Instance.RegisterTitleUI(nicknameInput, connectButton);
    }
}