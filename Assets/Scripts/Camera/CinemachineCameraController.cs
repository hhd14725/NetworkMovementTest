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
        vCam.Priority = 0;  // 기본 우선순위
        vCam.Follow = null;
        vCam.LookAt = null;
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            vCam.Priority = 100;  // 로컬 플레이어 카메라 우선순위 최상으로
            // (필요하다면 코드에서 Follow/LookAt 재할당)
            Transform pivot = transform.Find("CameraPivot");
            vCam.Follow = pivot;
            vCam.LookAt = pivot;
        }
    }
}