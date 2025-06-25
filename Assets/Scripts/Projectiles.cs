// Projectile.cs
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviourPun
{
    public float speed = 30f;
    public float gravity = 9.81f;
    public float maxDistance = 20f;

    private Rigidbody rb;
    private Vector3 spawnPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        spawnPos = transform.position;
    }
    // ��Ʈ��ũ Instantiate ���� ȣ��
    public void Initialize(Collider ownerCollider)
    {
        // 0.2�� ���� �÷��̾� �ݶ��̴� ����
        Physics.IgnoreCollision(GetComponent<Collider>(), ownerCollider, true);
        Invoke(nameof(ReenableCollision), 0.2f);
    }

    void ReenableCollision()
    {
        // �߻� �Ŀ� �ٽ� �浹 ����
        Physics.IgnoreCollision(GetComponent<Collider>(),
            GetComponent<Collider>(), false);
    }
    void Start()
    {
        rb.velocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        if ((transform.position - spawnPos).magnitude > maxDistance)
            PhotonNetwork.Destroy(gameObject);
    }

    void OnCollisionEnter(Collision _)
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
