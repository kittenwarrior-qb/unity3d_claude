using UnityEngine;
using UnityEngine.EventSystems;

namespace CozyStroll.UI
{
    /// <summary>
    /// Cozy hover/press feedback for UI buttons: a gentle ease-out-back scale up
    /// on hover and a small dip on press, returning smoothly to rest.
    /// </summary>
    public class UiButtonBounce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                  IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.06f;
        public float pressScale = 0.96f;
        public float speed = 12f;

        private Vector3 _rest;
        private float _target = 1f;

        private void Awake() => _rest = transform.localScale;

        private void Update()
        {
            float s = Mathf.Lerp(transform.localScale.x / Mathf.Max(_rest.x, 1e-4f), _target,
                                 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = _rest * s;
        }

        public void OnPointerEnter(PointerEventData e) => _target = hoverScale;
        public void OnPointerExit(PointerEventData e) => _target = 1f;
        public void OnPointerDown(PointerEventData e) => _target = pressScale;
        public void OnPointerUp(PointerEventData e) => _target = hoverScale;
    }
}
