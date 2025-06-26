using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class FloatingObstacle : MonoBehaviourPun
{
    [Header("Stabilization Settings")]
    [Tooltip("초기 위치로 되돌아가는 스프링 힘 크기")] public float springStrength = 5f;
    [Tooltip("공중 고정 시 허용 최대 거리")] public float maxSpringDistance = 0.5f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private PhotonRigidbodyView rbView;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        initialPosition = transform.position;

        rbView = GetComponent<PhotonRigidbodyView>();

        if (!photonView.IsMine)
        {
            rb.isKinematic = true;
            if (rbView != null)
                rbView.enabled = false; // 클라이언트는 velocity sync 비활성
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

        Vector3 offset = initialPosition - transform.position;
        float distance = offset.magnitude;
        if (distance > 0.01f)
        {
            float strength = springStrength;
            if (distance > maxSpringDistance)
                strength *= (maxSpringDistance / distance);
            rb.AddForce(offset.normalized * strength, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine)
            return;

        Vector3 normal = collision.contacts[0].normal;
        Vector3 reflect = Vector3.Reflect(rb.velocity, normal);
        rb.velocity = reflect * 0.8f;
    }
}