using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("�÷��̾� Ʈ������")]
    public Transform target;
    [Tooltip("ī�޶� ��ġ ������")]
    public Vector3 offset = new Vector3(0, 2, -4);
    [Tooltip("ȸ�� �ӵ�")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        // ��ǥ ��ġ ���
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        // �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        // �÷��̾� �ٶ󺸱�
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}