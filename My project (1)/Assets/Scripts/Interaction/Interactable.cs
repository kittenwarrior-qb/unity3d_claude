using UnityEngine;
using UnityEngine.Events;

namespace CozyStroll.Interaction
{
    /// <summary>
    /// Anything the player can walk up to and press E on: a flower to pick,
    /// a bench to sit on, a cat to pet. Cozy & lightweight — no inventory,
    /// just a friendly response and an optional one-shot or repeatable action.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Verb shown in the prompt, e.g. 'nhặt hoa', 'ngồi xuống', 'vỗ mèo'.")]
        public string promptVerb = "tương tác";

        [Tooltip("How close (metres) the player must be to interact.")]
        public float radius = 2.2f;

        [Tooltip("Can it be used more than once?")]
        public bool repeatable = true;

        [Tooltip("Little hop/feedback when used.")]
        public bool bounceOnUse = true;

        public UnityEvent onInteract;

        private bool _used;
        private Vector3 _baseScale;
        private float _bounceT = -1f;

        private void Awake() => _baseScale = transform.localScale;

        public bool CanInteract => repeatable || !_used;

        public string Prompt => $"Nhấn E để {promptVerb}";

        public void Interact()
        {
            if (!CanInteract) return;
            _used = true;
            if (bounceOnUse) _bounceT = 0f;
            onInteract?.Invoke();
        }

        private void Update()
        {
            if (_bounceT < 0f) return;
            _bounceT += Time.deltaTime * 3.5f;
            if (_bounceT >= 1f)
            {
                _bounceT = -1f;
                transform.localScale = _baseScale;
                return;
            }
            // Ease-out-back-ish pop.
            float s = 1f + Mathf.Sin(_bounceT * Mathf.PI) * 0.18f;
            transform.localScale = _baseScale * s;
        }
    }
}
