using UnityEngine;
using Cinemachine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineCameraController : MonoBehaviourPun
{
    CinemachineVirtualCamera vCam;
    CinemachineCollider colliderExt;

    void Awake()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        colliderExt = vCam.GetComponent<CinemachineCollider>();

        if (!photonView.IsMine)
        {
            // Remote �÷��̾� ī�޶�� �ƿ� �� �����ϴ�.
            vCam.gameObject.SetActive(false);
            return;
        }

        // Local �÷��̾�� Priority ���� + Follow/LookAt
        Transform pivot = transform.Find("CameraPivot");
        if (pivot == null)
            Debug.LogError("CameraPivot�� ã�� �� �����ϴ�!", this);
        else
        {
            vCam.Follow = pivot;
            vCam.LookAt = pivot;
        }
        vCam.Priority = 100;

        // �ݶ��̴��� ���߿�(�� ������ ��) �ѱ� ���� �ϴ� �� �Ӵϴ�.
        if (colliderExt != null)
            colliderExt.enabled = false;
    }

    // IEnumerator Start�� ���� �ڷ�ƾ ���̵� �� ������ ��⸦ �� �� �ֽ��ϴ�.
    IEnumerator Start()
    {
        // Local �÷��̾�� ����
        if (!photonView.IsMine)
            yield break;

        // �� ������ ��� �� pivot, rigidbody, ��Ʈ��ũ ���� �� ��� �ʱ�ȭ ���� ����
        yield return null;

        if (colliderExt != null)
            colliderExt.enabled = true;
    }
}
