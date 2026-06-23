using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.UI
{
    /// <summary>
    /// Shows the control hints for a few seconds, then gently fades them out.
    /// Cozy & non-intrusive (UI_STYLE.md): hints appear, then get out of the way.
    /// </summary>
    public class ControlHints : MonoBehaviour
    {
        public CanvasGroup group;
        public float holdSeconds = 5f;
        public float fadeSeconds = 1.5f;

        private float _timer;

        private void Awake()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (group == null) return;
            _timer += Time.deltaTime;

            if (_timer < holdSeconds)
            {
                group.alpha = 1f;
            }
            else if (_timer < holdSeconds + fadeSeconds)
            {
                group.alpha = 1f - (_timer - holdSeconds) / fadeSeconds;
            }
            else
            {
                group.alpha = 0f;
                enabled = false; // done; stop updating
            }
        }
    }
}
