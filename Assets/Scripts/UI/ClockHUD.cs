using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyStroll.UI
{
    /// <summary>
    /// Binds a GameClock to a TextMeshPro label in the HUD.
    /// Style: mint rounded panel, warm gray text (see UI_STYLE.md).
    /// Attach to the clock UI panel; assign the clock + label.
    /// </summary>
    public class ClockHUD : MonoBehaviour
    {
        public GameClock clock;
        public TMP_Text label;

        private void Reset()
        {
            // Try to auto-find when added in editor.
            label = GetComponentInChildren<TMP_Text>();
            if (clock == null) clock = FindObjectOfType<GameClock>();
        }

        private void OnEnable()
        {
            if (clock != null)
            {
                clock.OnMinuteChanged += SetText;
                SetText(clock.CurrentTimeString);
            }
        }

        private void OnDisable()
        {
            if (clock != null)
                clock.OnMinuteChanged -= SetText;
        }

        private void SetText(string time)
        {
            if (label != null)
                label.text = time;
        }
    }
}
