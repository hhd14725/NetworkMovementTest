//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(Camera))]
//public class ThirdPersonCamera : MonoBehaviour
//{
//    public Transform target;
//    public Vector3 offset = new Vector3(0f, 2f, -4f);
//    public float smoothSpeed = 5f;

//    private float yawAngle;
//    private float pitchAngle;

//    void LateUpdate()
//    {
//        if (target == null) return;
//        var pi = target.GetComponent<PlayerInput>();
//        if (pi != null)
//        {
//            Vector2 look = pi.actions["Look"].ReadValue<Vector2>();
//            yawAngle += look.x * Time.deltaTime * 90f;
//            pitchAngle -= look.y * Time.deltaTime * 90f;
//            pitchAngle = Mathf.Clamp(pitchAngle, -30f, 60f);
//        }
//        Quaternion rot = Quaternion.Euler(pitchAngle, yawAngle, 0f);
//        Vector3 desiredPos = target.position + rot * offset;
//        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
//        transform.rotation = Quaternion.Slerp(transform.rotation, rot, smoothSpeed * Time.deltaTime);
//    }
//}