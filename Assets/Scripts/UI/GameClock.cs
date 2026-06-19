using System;
using UnityEngine;

namespace CozyStroll.UI
{
    /// <summary>
    /// Tracks in-game time of day and advances it over real time.
    /// Other systems (clock HUD, day/night light) read TimeOfDay01 or CurrentTimeString.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        [Header("Start")]
        [Range(0f, 24f)] public float startHour = 17f; // cozy golden hour by default

        [Header("Speed")]
        [Tooltip("How many in-game minutes pass per real second.")]
        public float minutesPerSecond = 1f;

        /// <summary>Normalized time of day, 0 = 00:00, 1 = 24:00.</summary>
        public float TimeOfDay01 { get; private set; }

        /// <summary>Formatted "HH:MM" (24h).</summary>
        public string CurrentTimeString { get; private set; } = "00:00";

        /// <summary>Fires when the displayed minute changes.</summary>
        public event Action<string> OnMinuteChanged;

        private double _totalMinutes;
        private int _lastShownMinute = -1;

        private void Awake()
        {
            _totalMinutes = startHour * 60.0;
            UpdateDerived();
        }

        private void Update()
        {
            _totalMinutes += minutesPerSecond * Time.deltaTime;
            _totalMinutes %= 24.0 * 60.0;
            UpdateDerived();
        }

        private void UpdateDerived()
        {
            TimeOfDay01 = (float)(_totalMinutes / (24.0 * 60.0));

            int totalMin = (int)_totalMinutes;
            int hh = (totalMin / 60) % 24;
            int mm = totalMin % 60;

            if (mm != _lastShownMinute)
            {
                _lastShownMinute = mm;
                CurrentTimeString = $"{hh:00}:{mm:00}";
                OnMinuteChanged?.Invoke(CurrentTimeString);
            }
        }
    }
}
