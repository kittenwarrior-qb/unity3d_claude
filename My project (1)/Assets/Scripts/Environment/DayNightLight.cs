using UnityEngine;
using CozyStroll.UI;

namespace CozyStroll.Environment
{
    /// <summary>
    /// Gently drives the sun's angle and colour from the <see cref="GameClock"/>.
    /// Stays cozy: warm at golden hour, soft and dim (never pitch black) at night.
    /// </summary>
    public class DayNightLight : MonoBehaviour
    {
        public GameClock clock;
        public Light sun;

        [Header("Cozy tints")]
        public Gradient sunColorOverDay;
        public Gradient ambientOverDay;
        public AnimationCurve intensityOverDay;

        private void Awake()
        {
            if (sun == null) sun = GetComponent<Light>();
            if (clock == null) clock = FindObjectOfType<GameClock>();
            BuildDefaultsIfEmpty();
        }

        private void Update()
        {
            if (clock == null || sun == null) return;

            float t = clock.TimeOfDay01; // 0..1

            // Sun arc: rotate around X so it rises in the east-ish and sets.
            // t=0.25 (06:00) horizon rise, t=0.5 (12:00) overhead, t=0.75 (18:00) set.
            float sunAngle = (t * 360f) - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            sun.color = sunColorOverDay.Evaluate(t);
            sun.intensity = Mathf.Max(0.15f, intensityOverDay.Evaluate(t));

            RenderSettings.ambientLight = ambientOverDay.Evaluate(t);
        }

        private void BuildDefaultsIfEmpty()
        {
            if (sunColorOverDay == null || sunColorOverDay.colorKeys.Length == 0)
            {
                sunColorOverDay = Grad(
                    (0.00f, "#3A4A6B"), // deep night blue
                    (0.22f, "#FFB07A"), // dawn warm
                    (0.30f, "#FFE2B8"),
                    (0.50f, "#FFF6E6"), // midday soft white
                    (0.70f, "#FFE2B8"),
                    (0.78f, "#FF9E6B"), // golden sunset
                    (0.88f, "#6A6B9B"), // dusk
                    (1.00f, "#3A4A6B"));
            }
            if (ambientOverDay == null || ambientOverDay.colorKeys.Length == 0)
            {
                ambientOverDay = Grad(
                    (0.00f, "#2A3550"),
                    (0.25f, "#C8D8E0"),
                    (0.50f, "#DFF5EC"), // mint daylight ambient
                    (0.75f, "#E8D0C0"),
                    (1.00f, "#2A3550"));
            }
            if (intensityOverDay == null || intensityOverDay.keys.Length == 0)
            {
                intensityOverDay = new AnimationCurve(
                    new Keyframe(0.00f, 0.15f),
                    new Keyframe(0.25f, 0.8f),
                    new Keyframe(0.50f, 1.15f),
                    new Keyframe(0.75f, 0.9f),
                    new Keyframe(0.85f, 0.5f),
                    new Keyframe(1.00f, 0.15f));
            }
        }

        private static Gradient Grad(params (float t, string hex)[] stops)
        {
            var g = new Gradient();
            var keys = new GradientColorKey[stops.Length];
            for (int i = 0; i < stops.Length; i++)
            {
                ColorUtility.TryParseHtmlString(stops[i].hex, out var c);
                keys[i] = new GradientColorKey(c, stops[i].t);
            }
            g.SetKeys(keys, new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            });
            return g;
        }
    }
}
