using UnityEngine;

namespace CozyStroll.Player
{
    /// <summary>
    /// Third-person character controller for a cozy stroll game.
    /// Move relative to camera, run with Shift, jump with Space.
    /// Attach to a GameObject that has a CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Walk speed in units/second.")]
        public float walkSpeed = 2.5f;
        [Tooltip("Run speed when holding Shift.")]
        public float runSpeed = 5f;
        [Tooltip("How fast the character turns to face the move direction.")]
        public float turnSmoothTime = 0.1f;

        [Header("Jump & Gravity")]
        public float jumpHeight = 1.2f;
        public float gravity = -20f;

        [Header("References")]
        [Tooltip("The camera used to make movement relative. Defaults to Camera.main.")]
        public Transform cameraTransform;

        // Exposed so other systems (footsteps, animation) can read state.
        public bool IsGrounded { get; private set; }
        public float CurrentPlanarSpeed { get; private set; }

        private CharacterController _controller;
        private float _verticalVelocity;
        private float _turnSmoothVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            IsGrounded = _controller.isGrounded;
            if (IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // keep grounded firmly

            // --- Read input (legacy Input Manager) ---
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0f, v).normalized;

            Vector3 move = Vector3.zero;
            if (inputDir.sqrMagnitude > 0.001f)
            {
                // Angle of input relative to camera yaw.
                float camYaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + camYaw;

                float smoothAngle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                move = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }

            bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float speed = running ? runSpeed : walkSpeed;
            Vector3 horizontalVelocity = move * speed;
            CurrentPlanarSpeed = horizontalVelocity.magnitude;

            // --- Jump ---
            if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = horizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
