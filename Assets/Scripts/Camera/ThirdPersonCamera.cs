using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("플레이어 트랜스폼")]
    public Transform target;
    [Tooltip("카메라 위치 오프셋")]
    public Vector3 offset = new Vector3(0, 2, -4);
    [Tooltip("회전 속도")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        // 목표 위치 계산
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        // 플레이어 바라보기
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}