using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviourPun, IPunObservable
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
    private float linearDamping = 0.05f;

    [SerializeField]
    [Range(0f, 1f)]
    private float bounceFactor = 0.1f;

    private Rigidbody rb;

    private float thrust1D, strafe1D, upDown1D, roll1D;
    private Vector2 pitchYaw;

    [Header("Shooting")]
    public string projectilePrefabName = "Projectile";
    public Transform muzzleTransform;
    public float fireCooldown = 0.2f;
    private float lastFireTime = 0;

    private PlayerInput pi;
    // smoothing
    private Vector3 networkPos;
    private Quaternion networkRot;
    private Vector3 networkVel;
    private Vector3 networkAngVel;
    private float lastPacketTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;
        //rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        pi = GetComponent<PlayerInput>();
        if (photonView.IsMine)
        {
            // 로컬 플레이어: 입력 활성화
            pi.enabled = true;
        }
        else
        {
            // 원격 플레이어: 입력 비활성화
            pi.enabled = false;
        }

        networkPos = transform.position;
        networkRot = transform.rotation;
    }

    void Start()
    {
     if(photonView.IsMine)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

    }



  

    void FixedUpdate()
    {
  
        if(photonView.IsMine)
        {
            HandleMovement();
            HandleInPuteRotate();

            //// 감쇠
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, linearDamping);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, angularDamping);

            //총구 오리엔테이션 보정(muzzleTransform이 캐릭터 로컬 Z축과 일치하도록 확인)
            if (muzzleTransform != null)
                muzzleTransform.rotation = transform.rotation;
        }
        else
        {
            float lag = Time.time - lastPacketTime;
            Vector3 predictedPos = networkPos + networkVel * lag;
            transform.position = Vector3.Lerp(transform.position, predictedPos, 0.1f);
            transform.rotation = Quaternion.Slerp(transform.rotation, networkRot, 0.1f);

        }

    }

    //void LateUpdate()
    //{
    //    if (photonView.IsMine) return;

    //    // 원격 보간: transform만 갱신
    //    float lag = Time.time - lastPacketTime;
    //    Vector3 predictedPos = networkPos + networkVel * lag;
    //    transform.position = Vector3.Lerp(transform.position, predictedPos, 0.1f);
    //    transform.rotation = Quaternion.Slerp(transform.rotation, networkRot, 0.1f);
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
        // Thrust
        if (Mathf.Abs(thrust1D) > 0.1f)
        {
            // 부호를 보존한 후진/전진 thrust를 glide에 저장
            glide = thrust1D * thrust;
            rb.AddRelativeForce(Vector3.forward * glide * Time.fixedDeltaTime);
        }
        else
        {
            // glide 부호 그대로 후진 또는 전진 미끄러짐
            rb.AddRelativeForce(Vector3.forward * glide * Time.fixedDeltaTime);
            glide *= thrustGlideReduction;
        }

        // Up/Down, Strafe도 동일하게 수정
        if (Mathf.Abs(upDown1D) > 0.1f)
        {
            verticalGlide = upDown1D * upThrust;
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
        }
        else
        {
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide *= upDownGlideReduction;
        }

        if (Mathf.Abs(strafe1D) > 0.1f)
        {
            horizontalGlide = strafe1D * strafeThrust;
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
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
        if (!photonView.IsMine) return;
        if (context.performed)
        {
            TryFire();
        }
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        // 벽만 반응하도록 Tag 검사
        if (!collision.gameObject.CompareTag("Wall")) return;

        // 첫 접촉의 법선 벡터
        Vector3 normal = collision.contacts[0].normal;

        // 현재 속도
        Vector3 v = rb.velocity;

        // 법선 방향 성분
        Vector3 vNormal = Vector3.Project(v, normal);
        // 접선(벡터의 나머지) 성분
        Vector3 vTangent = v - vNormal;

        // 법선 성분은 반사(bounceFactor), 접선 성분은 그대로
        Vector3 newVelocity = vTangent - vNormal * bounceFactor;
        rb.velocity = newVelocity;
    }




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




    //네트워크 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어 상태 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            // 원격 수신
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
            networkVel = (Vector3)stream.ReceiveNext();
            networkAngVel = (Vector3)stream.ReceiveNext();
            lastPacketTime = Time.time;
        }
    }
}
