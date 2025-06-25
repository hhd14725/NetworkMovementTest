using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Thrust Settings")] public float thrustForce = 10f;
    public float maxSpeed = 15f;
    public float dashImpulse = 20f;

    [Header("Rotation Settings")] public float yawSpeed = 120f;
    public float pitchSpeed = 120f;
    public float minPitch = -60f;
    public float maxPitch = 60f;
    private float yawAngle;
    private float pitchAngle;

    [Header("Damping")] public float linearDrag = 0.1f;

    [Header("Camera Pivot")] public Transform cameraPivot;
    public Vector3 cameraOffset = new Vector3(0, 2, -6);
    public float cameraSmooth = 5f;

    private Rigidbody rb;
    private PlayerInput pi;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); pi = GetComponent<PlayerInput>();
        pi.actions.Disable();
        rb.useGravity = false; rb.drag = linearDrag; rb.constraints = RigidbodyConstraints.FreezeRotation;
        yawAngle = transform.eulerAngles.y; pitchAngle = 0f;
    }

    void Start()
    {
        if (photonView.IsMine)
            pi.actions.Enable();
        else
            pi.actions.Disable();
    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;
        var cam = Camera.main;
        if (cam != null && cameraPivot != null)
        {
            cam.transform.SetParent(cameraPivot);
            cam.transform.localPosition = cameraOffset;
            cam.transform.localRotation = Quaternion.identity;
        }
        var a = pi.actions;
        a["Move"].performed += OnMovePerformed; a["Move"].canceled += OnMoveCanceled;
        a["Look"].performed += OnLookPerformed; a["Look"].canceled += OnLookCanceled;
        a["Boost"].performed += OnBoostPerformed; a["Fire"].performed += OnFirePerformed;
    }

    void OnDisable()
    {
        if (!photonView.IsMine || pi == null) return;
        var a = pi.actions; a["Move"].performed -= OnMovePerformed; a["Move"].canceled -= OnMoveCanceled;
        a["Look"].performed -= OnLookPerformed; a["Look"].canceled -= OnLookCanceled;
        a["Boost"].performed -= OnBoostPerformed; a["Fire"].performed -= OnFirePerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void OnLookPerformed(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    private void OnLookCanceled(InputAction.CallbackContext ctx) => lookInput = Vector2.zero;
    private void OnBoostPerformed(InputAction.CallbackContext ctx) => rb.AddForce(transform.forward * dashImpulse, ForceMode.Impulse);
    private void OnFirePerformed(InputAction.CallbackContext ctx) => PhotonNetwork.Instantiate("Projectile", transform.position + transform.forward * 1.5f, transform.rotation);

    void Update()
    {
        if (!photonView.IsMine) return;
        yawAngle += lookInput.x * yawSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, yawAngle, 0f);
        if (cameraPivot != null)
        {
            pitchAngle = Mathf.Clamp(pitchAngle - lookInput.y * pitchSpeed * Time.deltaTime, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 dir = transform.forward * moveInput.y + transform.right * moveInput.x;
            rb.AddForce(dir.normalized * thrustForce, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position); stream.SendNext(rb.velocity); stream.SendNext(transform.rotation);
        }
        else
        {
            rb.position = (Vector3)stream.ReceiveNext(); rb.velocity = (Vector3)stream.ReceiveNext(); transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
