using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.UI
{
    /// <summary>
    /// Binds a <see cref="GameClock"/> to a uGUI <see cref="Text"/> label.
    /// Used by the auto-built HUD so the clock renders with Unity's built-in font
    /// (no TextMeshPro essentials import required). For a TMP label, use ClockHUD.
    /// </summary>
    public class SimpleClockLabel : MonoBehaviour
    {
        public GameClock clock;
        public Text label;

        private void OnEnable()
        {
            if (clock == null) clock = FindObjectOfType<GameClock>();
            if (clock != null)
            {
                clock.OnMinuteChanged += SetText;
                SetText(clock.CurrentTimeString);
            }
        }

        private void OnDisable()
        {
            if (clock != null) clock.OnMinuteChanged -= SetText;
        }

        private void SetText(string time)
        {
            if (label != null) label.text = "  " + time;
        }
    }
}
