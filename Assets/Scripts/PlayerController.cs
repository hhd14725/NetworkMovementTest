using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour //, IPunObservable
{
    [Header("PlayerMovement Settings")]
    [SerializeField]
    private float yawTorque = 100f;
    [SerializeField]
    private float pitchTorque = 200f;
    [SerializeField]
    private float rollTorque = 500f;
    [SerializeField]
    private float thrust = 100f;
    [SerializeField]
    private float upThrust = 50f;
    [SerializeField]
    private float strafeThrust = 50f;
    [SerializeField]
    [Range(0.001f, 0.999f)]
    private float thrustGlideReduction = 0.5f;
    [SerializeField]
    [Range(0.001f, 0.999f)]
    private float upDownGlideReduction = 0.111f;
    [SerializeField]
    [Range(0.001f, 0.999f)]
    private float leftRightGlideReduction = 0.111f;
    private float glide, verticalGlide, horizontalGlide = 0f;
    private float currentThrust;
    private float angularDamping = 0.05f;

    private Rigidbody rb;

    private float thrust1D, strafe1D, upDown1D, roll1D;
    private Vector2 pitchYaw;

    //[Header("Thrust Settings")]
    //public float forwardThrust = 12f, strafeThrust = 9f, verticalThrust = 9f, maxSpeed = 15f;
    //[Header("Rotation Settings")]
    //public float yawSpeed = 120f, pitchSpeed = 120f, rollSpeed = 50f;
    //[Header("Damping")]
    //[Range(0f, 1f)] public float linearDamping = 0.05f, angularDamping = 0.05f;
    [Header("Shooting")]
    public string projectilePrefabName = "Projectile";
    public Transform muzzleTransform;
    public float fireCooldown = 0.2f;
    private float lastFireTime;
    //[Header("References")]
    //public Transform cameraPivot;
    //public Animator animator;

    //[Header("Friction Settings")]
    //[Tooltip("입력이 없을 때 적용할 drag 값. (높을수록 빨리 멈춤)")]
    //public float coastingDrag = 0.01f;


    //// smoothing
    //private Vector3 networkPos;
    //private Quaternion networkRot;
    //private Vector3 networkVel;
    //private Vector3 networkAngVel;
    //private float lastPacketTime;

    //// input
    //private PlayerInput pi;
    //private Vector2 moveInput, lookInput;
    //private bool thrustUp, thrustDown, rollLeft, rollRight;

    //private Rigidbody rb;

    //private float pitchAngle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        //rb.drag = 0f;
        //rb.angularDrag = 0.05f;
        //rb.interpolation = RigidbodyInterpolation.Extrapolate;

        //pi = GetComponent<PlayerInput>();
        //pi.actions.Disable();

        //networkPos = transform.position;
        //networkRot = transform.rotation;
    }

    void Start()
    {
     
            //pi.actions.Enable();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        
        //else
        //{
        //    pi.actions.Disable();
        //}
    }

    void OnEnable()
    {
        //if (!photonView.IsMine) return;
        //var a = pi.actions;
        //a["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //a["Move"].canceled += ctx => moveInput = Vector2.zero;
        //a["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        //a["Look"].canceled += ctx => lookInput = Vector2.zero;
        //a["ThrustUp"].performed += ctx => thrustUp = true;
        //a["ThrustUp"].canceled += ctx => thrustUp = false;
        //a["ThrustDown"].performed += ctx => thrustDown = true;
        //a["ThrustDown"].canceled += ctx => thrustDown = false;
        //a["RollLeft"].performed += ctx => rollLeft = true;
        //a["RollLeft"].canceled += ctx => rollLeft = false;
        //a["RollRight"].performed += ctx => rollRight = true;
        //a["RollRight"].canceled += ctx => rollRight = false;
        //a["Fire"].performed += ctx => TryFire();
    }

    void OnDisable()
    {
        //if (!photonView.IsMine) return;
        //var a = pi.actions;
        //a["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        //a["Move"].canceled -= ctx => moveInput = Vector2.zero;
        //a["Look"].performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        //a["Look"].canceled -= ctx => lookInput = Vector2.zero;
        //a["ThrustUp"].performed -= ctx => thrustUp = true;
        //a["ThrustUp"].canceled -= ctx => thrustUp = false;
        //a["ThrustDown"].performed -= ctx => thrustDown = true;
        //a["ThrustDown"].canceled -= ctx => thrustDown = false;
        //a["RollLeft"].performed -= ctx => rollLeft = true;
        //a["RollLeft"].canceled -= ctx => rollLeft = false;
        //a["RollRight"].performed -= ctx => rollRight = true;
        //a["RollRight"].canceled -= ctx => rollRight = false;
        //a["Fire"].performed -= ctx => TryFire();
    }


  

    void FixedUpdate()
    {
  
        
            HandleMovement();
        HandleInPuteRotate();

            //// 이동력
            //Vector3 thrust = transform.forward * moveInput.y * forwardThrust
            //               + transform.right * moveInput.x * strafeThrust
            //               + transform.up * ((thrustUp ? 1f : 0f) + (thrustDown ? -1f : 0f)) * verticalThrust;
            //rb.AddForce(thrust, ForceMode.Acceleration);

        ////감속로직
        ////bool isThrusting = moveInput.sqrMagnitude > 0f || thrustUp || thrustDown;
        ////rb.drag = isThrusting ? 0f : coastingDrag;

        //// 회전 토크: Yaw, Roll 만
        //float yaw = lookInput.x * yawSpeed * Time.fixedDeltaTime;
        //float delta = -lookInput.y * pitchSpeed * Time.fixedDeltaTime;
        //float roll = (rollRight ? 1f : 0f) * rollSpeed * Time.fixedDeltaTime
        //           - (rollLeft ? 1f : 0f) * rollSpeed * Time.fixedDeltaTime;
        //rb.AddRelativeTorque(new Vector3(delta, yaw, roll), ForceMode.Acceleration);

        //// 속도 제한
        //if (rb.velocity.magnitude > maxSpeed)
        //    rb.velocity = rb.velocity.normalized * maxSpeed;


        //// 감쇠
        ////rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, linearDamping);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, angularDamping);

            //총구 오리엔테이션 보정(muzzleTransform이 캐릭터 로컬 Z축과 일치하도록 확인)
            if (muzzleTransform != null)
                muzzleTransform.rotation = transform.rotation;

            // 애니메이터
            //if (animator)
            //{
            //    animator.SetFloat("Speed", rb.velocity.magnitude);
            //    animator.SetFloat("Vertical", Vector3.Dot(transform.up, rb.velocity));
            //}
      
            //// 원격 플레이어 보간·예측
            //float lag = (Time.time - lastPacketTime);
            //Vector3 predictedPos = networkPos + networkVel * lag;
            //rb.position = Vector3.Lerp(rb.position, predictedPos, 0.1f);
            //rb.rotation = Quaternion.Slerp(rb.rotation, networkRot, 0.1f);
        
    }

    //private void LateUpdate()
    //{

    //        HandleInPuteRotate();
     
    //}
    void HandleInPuteRotate()
    {
        //Roll
        rb.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.fixedDeltaTime);
        //Pitch
        rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.fixedDeltaTime);
        //Yaw
        rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.fixedDeltaTime);
    }
    void HandleMovement()
    {
        //Thrust
        if(thrust1D >0.1f || thrust1D < -0.1f)
        {
            currentThrust = thrust;
            rb.AddRelativeForce(Vector3.forward * thrust1D * currentThrust * Time.fixedDeltaTime);
            glide = thrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.forward * glide * Time.fixedDeltaTime);
            glide *= thrustGlideReduction;
        }

        //Up/Down
        if(upDown1D > 0.1f || upDown1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.up * upDown1D * upThrust * Time.fixedDeltaTime);
            verticalGlide = upDown1D * upThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide *= upDownGlideReduction;
        }

        //STRAFING
        if (strafe1D > 0.1f || strafe1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.right * strafe1D * strafeThrust * Time.fixedDeltaTime);
            horizontalGlide = strafe1D * strafeThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightGlideReduction;
        }
    }

    #region Input Methods
    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }
    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }
    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
    }
    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        pitchYaw = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("OnFire! phase=" + context.phase);
        if (context.performed)
        {
            TryFire();
        }
    }
    #endregion






    private void TryFire()
    {
        if (Time.time - lastFireTime < fireCooldown || muzzleTransform == null) return;
        lastFireTime = Time.time;
        // 총알 초기 속도 세팅
        var proj = PhotonNetwork.Instantiate(projectilePrefabName,
            muzzleTransform.position,
            muzzleTransform.rotation);
        var projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            var myCol = GetComponent<Collider>();
            if (myCol != null)
                projectile.Initialize(myCol);
        }

    }

    // 네트워크 동기화
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        // 로컬 플레이어 상태 전송
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //        stream.SendNext(rb.velocity);
    //        stream.SendNext(rb.angularVelocity);
    //    }
    //    else
    //    {
    //        // 원격 수신
    //        networkPos = (Vector3)stream.ReceiveNext();
    //        networkRot = (Quaternion)stream.ReceiveNext();
    //        networkVel = (Vector3)stream.ReceiveNext();
    //        networkAngVel = (Vector3)stream.ReceiveNext();
    //        lastPacketTime = Time.time;
    //    }
    //}
}
