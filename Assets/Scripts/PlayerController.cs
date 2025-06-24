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
        // 무중력 세팅
        rb.useGravity = false;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void OnEnable()
    {
        if (photonView.IsMine)
        {
            // 로컬 플레이어만 입력 받기
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
            // 원격 플레이어는 입력·카메라 비활성
            pi.enabled = false;
            cam.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 1) 마우스 Δ를 Pitch/Yaw 회전에 매핑
        Vector3 e = transform.localEulerAngles;
        float pitch = -lookInput.y * rotationSpeed * Time.deltaTime;
        float yaw = lookInput.x * rotationSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(e.x + pitch, e.y + yaw, 0f);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        // 2) Thruster: 앞/뒤+좌/우 입력
        Vector3 dir = (transform.forward * moveInput.y + transform.right * moveInput.x);
        if (dir.sqrMagnitude > 0.01f)
        {
            Vector3 force = dir.normalized * thrustForce * (isBoosting ? boostMultiplier : 1f);
            rb.AddForce(force, ForceMode.Acceleration);
            // 속도 제한
            float mv = maxSpeed * (isBoosting ? boostMultiplier : 1f);
            if (rb.velocity.magnitude > mv)
                rb.velocity = rb.velocity.normalized * mv;
        }
        else
        {
            // 입력 없으면 감속
            if (rb.velocity.magnitude > 0.1f)
            {
                Vector3 decel = -rb.velocity.normalized * decelerationForce;
                rb.AddForce(decel, ForceMode.Acceleration);
            }
        }
    }

    void Shoot()
    {
        // 발사 위치
        Vector3 spawnPos = transform.position + transform.forward * 1.5f;
        PhotonNetwork.Instantiate("Projectile", spawnPos, transform.rotation);
    }

    // 3) 네트워크 동기화
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
            // 부드럽게 보간
            rb.position = Vector3.Lerp(rb.position, pos, 0.5f);
            rb.velocity = vel;
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.5f);
        }
    }
}
