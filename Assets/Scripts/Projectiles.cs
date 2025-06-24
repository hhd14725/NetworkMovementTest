using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviourPun
{
    [Header("Projectile Settings")]
    public float speed = 30f;
    public float gravity = 9.81f;
    public float maxDistance = 20f;

    Rigidbody rb;
    Vector3 spawnPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        spawnPos = transform.position;
    }

    void Start()
    {
        // 초기 속도
        rb.velocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        // 수동 중력 적용 (낙차)
        rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;

        // 사거리 초과 시 소멸
        if ((transform.position - spawnPos).magnitude > maxDistance)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision _)
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
