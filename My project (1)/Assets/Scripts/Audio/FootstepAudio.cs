using UnityEngine;
using CozyStroll.Player;

namespace CozyStroll.Audio
{
    /// <summary>
    /// Plays footstep sounds while the player moves on the ground.
    /// Step interval scales with speed (faster when running).
    /// Needs an AudioSource and the PlayerController on the same hierarchy.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class FootstepAudio : MonoBehaviour
    {
        public PlayerController player;
        public AudioClip[] footstepClips;

        [Tooltip("Seconds between steps at walk speed.")]
        public float walkStepInterval = 0.5f;
        [Tooltip("Seconds between steps at run speed.")]
        public float runStepInterval = 0.3f;
        [Tooltip("Minimum planar speed before footsteps play.")]
        public float moveThreshold = 0.3f;

        [Range(0f, 1f)] public float volume = 0.6f;
        public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

        private AudioSource _source;
        private float _stepTimer;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            if (player == null) player = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {
            if (player == null || footstepClips == null || footstepClips.Length == 0) return;

            bool moving = player.IsGrounded && player.CurrentPlanarSpeed > moveThreshold;
            if (!moving)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep();
                // Faster movement -> shorter interval.
                float t = Mathf.InverseLerp(player.walkSpeed, player.runSpeed, player.CurrentPlanarSpeed);
                _stepTimer = Mathf.Lerp(walkStepInterval, runStepInterval, t);
            }
        }

        private void PlayStep()
        {
            var clip = footstepClips[Random.Range(0, footstepClips.Length)];
            _source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            _source.PlayOneShot(clip, volume);
        }
    }
}
