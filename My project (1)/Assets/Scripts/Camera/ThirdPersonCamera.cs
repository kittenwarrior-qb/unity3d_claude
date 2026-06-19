using UnityEngine;

namespace CozyStroll.CameraRig
{
    /// <summary>
    /// Smooth third-person follow camera with mouse orbit.
    /// Put this on the Main Camera and assign 'target' to the player.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        [Tooltip("Offset above the target's pivot to look at (e.g. head height).")]
        public Vector3 lookAtOffset = new Vector3(0f, 1.4f, 0f);

        [Header("Distance & Height")]
        public float distance = 5f;
        public float height = 2f;

        [Header("Orbit (mouse)")]
        public float yawSpeed = 180f;   // deg/sec per mouse unit
        public float pitchSpeed = 120f;
        public float minPitch = -10f;
        public float maxPitch = 60f;

        [Header("Smoothing")]
        public float followSmoothTime = 0.12f;

        private float _yaw;
        private float _pitch = 15f;
        private Vector3 _currentVelocity;

        private void Start()
        {
            if (target != null)
                _yaw = target.eulerAngles.y;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Orbit from mouse.
            _yaw += Input.GetAxis("Mouse X") * yawSpeed * Time.deltaTime;
            _pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 focusPoint = target.position + lookAtOffset;
            Vector3 desiredPos = focusPoint - rotation * Vector3.forward * distance + Vector3.up * (height - lookAtOffset.y);

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _currentVelocity, followSmoothTime);
            transform.LookAt(focusPoint);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
