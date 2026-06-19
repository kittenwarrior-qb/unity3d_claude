using UnityEngine;
using CozyStroll.UI;

namespace CozyStroll.Environment
{
    /// <summary>
    /// Gentle, cozy weather: clear / light rain / soft snow. Drifts between states
    /// on a slow timer (mostly clear) and can be cycled manually with V. Particle
    /// systems follow the player so coverage feels global; fog tints to match.
    /// </summary>
    public class WeatherController : MonoBehaviour
    {
        public enum Weather { Clear, Rain, Snow }

        public Transform follow;
        public KeyCode cycleKey = KeyCode.V;
        public Vector2 stateDuration = new Vector2(35f, 90f);

        private ParticleSystem _rain;
        private ParticleSystem _snow;
        private ParticleSystem.EmissionModule _rainEmit;
        private ParticleSystem.EmissionModule _snowEmit;

        private Weather _current = Weather.Clear;
        private float _timer;
        private readonly Color _clearFog = PaletteLibrary.Hex("#DDF2FF");
        private readonly Color _rainFog  = PaletteLibrary.Hex("#C5D0DA");
        private readonly Color _snowFog  = PaletteLibrary.Hex("#E8EEF2");

        public void Build(Transform followTarget, float worldSize)
        {
            follow = followTarget;
            _rain = BuildRain(worldSize);
            _snow = BuildSnow(worldSize);
            _rainEmit = _rain.emission;
            _snowEmit = _snow.emission;
            SetWeather(Weather.Clear);
            _timer = Random.Range(stateDuration.x, stateDuration.y);
        }

        private void Update()
        {
            if (follow != null)
            {
                Vector3 p = follow.position;
                transform.position = new Vector3(p.x, p.y + 16f, p.z);
            }

            if (Input.GetKeyDown(cycleKey))
                SetWeather((Weather)(((int)_current + 1) % 3));

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                SetWeather(PickNext());
                _timer = Random.Range(stateDuration.x, stateDuration.y);
            }
        }

        public void SetWeather(Weather w)
        {
            _current = w;
            _rainEmit.rateOverTime = w == Weather.Rain ? 320f : 0f;
            _snowEmit.rateOverTime = w == Weather.Snow ? 90f : 0f;

            RenderSettings.fogColor = w switch
            {
                Weather.Rain => _rainFog,
                Weather.Snow => _snowFog,
                _ => _clearFog,
            };
            // Rain pulls the view in a touch for a snug, drizzly feel.
            RenderSettings.fogStartDistance = w == Weather.Rain ? 22f : 35f;
            RenderSettings.fogEndDistance = w == Weather.Rain ? 75f : 90f;
        }

        private Weather PickNext()
        {
            float r = Random.value;          // mostly clear, occasional weather
            if (r < 0.60f) return Weather.Clear;
            if (r < 0.82f) return Weather.Rain;
            return Weather.Snow;
        }

        // ---- Rain ----
        private ParticleSystem BuildRain(float worldSize)
        {
            var go = new GameObject("FX_Rain");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime = 1.4f;
            main.startSpeed = 18f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.07f);
            main.startColor = new Color(0.78f, 0.86f, 0.95f, 0.55f);
            main.gravityModifier = 1f;
            main.maxParticles = 1200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 1f, 34f);
            shape.rotation = new Vector3(90f, 0f, 0f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 6f;
            renderer.velocityScale = 0.1f;
            renderer.material = ParticleMat(RoundedSprite.SoftDotTexture());

            ps.Play();
            return ps;
        }

        // ---- Snow ----
        private ParticleSystem BuildSnow(float worldSize)
        {
            var go = new GameObject("FX_Snow");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime = 9f;
            main.startSpeed = 1.2f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = new Color(1f, 1f, 1f, 0.9f);
            main.gravityModifier = 0.06f;
            main.maxParticles = 900;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(38f, 1f, 38f);
            shape.rotation = new Vector3(90f, 0f, 0f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.2f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = ParticleMat(RoundedSprite.SoftDotTexture());

            ps.Play();
            return ps;
        }

        private static Material ParticleMat(Texture2D tex)
        {
            var mat = new Material(Shader.Find("Sprites/Default")) { name = "CozyWeather" };
            mat.mainTexture = tex;
            return mat;
        }
    }
}
