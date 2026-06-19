using UnityEngine;

namespace CozyStroll.UI
{
    /// <summary>
    /// Drives a top-down orthographic camera that follows the player, rendered into
    /// a RenderTexture and shown as a circular HUD minimap (set up by HudBuilder).
    /// </summary>
    public class Minimap : MonoBehaviour
    {
        public Transform target;
        public float height = 25f;
        [Tooltip("World units half-size the minimap camera sees.")]
        public float viewSize = 22f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null || _cam == null) return;

            Vector3 p = target.position;
            transform.position = new Vector3(p.x, p.y + height, p.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // straight down
            _cam.orthographicSize = viewSize;
        }
    }
}
