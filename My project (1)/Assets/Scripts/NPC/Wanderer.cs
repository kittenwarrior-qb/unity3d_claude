using UnityEngine;

namespace CozyStroll.NPC
{
    /// <summary>
    /// A gentle town villager that ambles between random points, pausing now and then.
    /// No pathfinding needed for a cozy open square — just smooth turn + walk + idle.
    /// </summary>
    public class Wanderer : MonoBehaviour
    {
        public float walkSpeed = 1.4f;
        public float turnSpeed = 4f;
        public float wanderRadius = 22f;
        public Vector2 pauseRange = new Vector2(1.5f, 4f);
        public float arriveDistance = 0.6f;

        private Vector3 _home;
        private Vector3 _target;
        private float _pauseTimer;

        private void Start()
        {
            _home = transform.position;
            PickNewTarget();
        }

        private void Update()
        {
            if (_pauseTimer > 0f)
            {
                _pauseTimer -= Time.deltaTime;
                return;
            }

            Vector3 flat = _target - transform.position;
            flat.y = 0f;

            if (flat.magnitude <= arriveDistance)
            {
                _pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
                PickNewTarget();
                return;
            }

            Vector3 dir = flat.normalized;
            // Smooth turn toward heading.
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);

            transform.position += dir * walkSpeed * Time.deltaTime;
        }

        private void PickNewTarget()
        {
            Vector2 c = Random.insideUnitCircle * wanderRadius;
            _target = _home + new Vector3(c.x, 0f, c.y);
        }
    }
}
