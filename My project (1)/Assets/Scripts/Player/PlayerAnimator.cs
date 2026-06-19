using UnityEngine;

namespace CozyStroll.Player
{
    /// <summary>
    /// Bridges PlayerController state to an Animator on the child model.
    /// Attach to the Player root; the Animator lives on the Mixamo model child.
    /// Animator parameters expected (names must match what MixamoImporter creates):
    ///   Speed     (Float)   – 0 = idle, 0.4 = walk, 1 = run
    ///   IsGrounded(Bool)    – true when touching ground
    ///   Jump      (Trigger) – fires once on lift-off
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        public PlayerController player;
        public Animator modelAnimator;

        private static readonly int SpeedId     = Animator.StringToHash("Speed");
        private static readonly int GroundedId  = Animator.StringToHash("IsGrounded");
        private static readonly int JumpId      = Animator.StringToHash("Jump");

        private bool _prevGrounded = true;

        private void Awake()
        {
            if (player == null)        player        = GetComponent<PlayerController>();
            if (modelAnimator == null) modelAnimator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (player == null || modelAnimator == null) return;

            // Normalise to 0-1: 0=idle, full speed (run) = 1.
            float maxSpeed = Mathf.Max(player.runSpeed, 0.01f);
            float norm = Mathf.Clamp01(player.CurrentPlanarSpeed / maxSpeed);

            // Dampen the blend so transitions feel smooth.
            modelAnimator.SetFloat(SpeedId, norm, 0.08f, Time.deltaTime);
            modelAnimator.SetBool(GroundedId, player.IsGrounded);

            // Trigger jump exactly once on the frame the player lifts off.
            if (_prevGrounded && !player.IsGrounded)
                modelAnimator.SetTrigger(JumpId);

            _prevGrounded = player.IsGrounded;
        }
    }
}
