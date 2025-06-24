using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Move Settings")]
    public float thrustForce = 10f;
    public float maxSpeed = 5f;
    public float boostMultiplier = 2f;
    public float decelerationForce = 8f;

    [Header("Rotation Settings")]
    [Tooltip("degrees per second")]
    public float rotationSpeed = 90f;

    Rigidbody rb;
    Camera cam;
    PlayerInput pi;

    Vector2 moveInput;
    Vector2 lookInput;
    bool isBoosting;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();
        cam = Camera.main;
        // ���߷� ����
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void OnEnable()
    {
        if (photonView.IsMine)
        {
            // ���� �÷��̾ �Է� �ޱ�
            pi.enabled = true;
            var a = pi.actions;
            a["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            a["Move"].canceled += ctx => moveInput = Vector2.zero;
            a["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            a["Look"].canceled += ctx => lookInput = Vector2.zero;
            a["Boost"].performed += ctx => isBoosting = true;
            a["Boost"].canceled += ctx => isBoosting = false;
            a["Fire"].performed += ctx => Shoot();
        }
        else
        {
            // ���� �÷��̾�� �Է¡�ī�޶� ��Ȱ��
            pi.enabled = false;
            cam.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 1) ���콺 �ĸ� Pitch/Yaw ȸ���� ����
        Vector3 e = transform.localEulerAngles;
        float pitch = -lookInput.y * rotationSpeed * Time.deltaTime;
        float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(e.x + pitch, e.y + yaw, 0f);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        // 2) Thruster: ��/��+��/�� �Է�
        Vector3 dir = (transform.forward * moveInput.y + transform.right * moveInput.x);
        if (dir.sqrMagnitude > 0.01f)
        {
            Vector3 force = dir.normalized * thrustForce * (isBoosting ? boostMultiplier : 1f);
            rb.AddForce(force, ForceMode.Acceleration);
            // �ӵ� ����
            float mv = maxSpeed * (isBoosting ? boostMultiplier : 1f);
            if (rb.velocity.magnitude > mv)
                rb.velocity = rb.velocity.normalized * mv;
        }
        else
        {
            // �Է� ������ ����
            if (rb.velocity.magnitude > 0.1f)
            {
                Vector3 decel = -rb.velocity.normalized * decelerationForce;
                rb.AddForce(decel, ForceMode.Acceleration);
            }
        }
    }

    void Shoot()
    {
        // �߻� ��ġ
        Vector3 spawnPos = transform.position + transform.forward * 1.5f;
        PhotonNetwork.Instantiate("Projectile", spawnPos, transform.rotation);
    }

    // 3) ��Ʈ��ũ ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.velocity);
            stream.SendNext(transform.rotation);
        }
        else
        {
            Vector3 pos = (Vector3)stream.ReceiveNext();
            Vector3 vel = (Vector3)stream.ReceiveNext();
            Quaternion rot = (Quaternion)stream.ReceiveNext();
            // �ε巴�� ����
            rb.position = Vector3.Lerp(rb.position, pos, 0.5f);
            rb.velocity = vel;
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.5f);
        }
    }
}
