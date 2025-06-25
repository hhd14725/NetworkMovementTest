using UnityEngine;
using Cinemachine;
using Photon.Pun;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineCameraController : MonoBehaviourPun
{
    CinemachineVirtualCamera vCam;

    void Awake()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        vCam.Priority = 0;  // �⺻ �켱����
        vCam.Follow = null;
        vCam.LookAt = null;
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            vCam.Priority = 100;  // ���� �÷��̾� ī�޶� �켱���� �ֻ�����
            // (�ʿ��ϴٸ� �ڵ忡�� Follow/LookAt ���Ҵ�)
            Transform pivot = transform.Find("CameraPivot");
            vCam.Follow = pivot;
            vCam.LookAt = pivot;
        }
    }
}