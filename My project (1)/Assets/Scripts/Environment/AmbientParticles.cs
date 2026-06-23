using UnityEngine;
using CozyStroll.UI;

namespace CozyStroll.Environment
{
    /// <summary>
    /// Builds two gentle particle layers for atmosphere:
    ///  • drifting leaves falling across the whole town (daytime charm)
    ///  • fireflies that softly appear at night (driven by the GameClock).
    /// Materials/textures are generated in code — no asset files required.
    /// </summary>
    public class AmbientParticles : MonoBehaviour
    {
        public CozyStroll.UI.GameClock clock;
        public float areaSize = 60f;

        private ParticleSystem.EmissionModule _fireflyEmission;
        private float _fireflyBaseRate = 26f;

        public void Build(GameClock gameClock, float worldSize)
        {
            clock = gameClock;
            areaSize = worldSize;

            BuildLeaves();
            BuildFireflies();
        }

        private void Update()
        {
            if (clock == null) return;
            float night = NightAmount(clock.TimeOfDay01);
            _fireflyEmission.rateOverTime = _fireflyBaseRate * night;
        }

        // ---- Falling leaves ----
        private void BuildLeaves()
        {
            var go = new GameObject("FX_Leaves");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(0f, 14f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime = 9f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                PaletteLibrary.Hex("#9FD68A"), PaletteLibrary.Hex("#E0A86B"));
            main.gravityModifier = 0.05f;
            main.maxParticles = 250;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

            var emission = ps.emission;
            emission.rateOverTime = 14f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(areaSize, 1f, areaSize);

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.25f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = FadeInOutGradient();

            ApplyMaterial(ps, BlendMode.Alpha);
            ps.Play();
        }

        // ---- Fireflies (night) ----
        private void BuildFireflies()
        {
            var go = new GameObject("FX_Fireflies");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(0f, 1.2f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.startLifetime = 4.5f;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                PaletteLibrary.Hex("#FFF6B0"), PaletteLibrary.Hex("#D8F59A"));
            main.gravityModifier = 0f;
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f; // gated by Update / NightAmount
            _fireflyEmission = emission;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(areaSize * 0.85f, 2.5f, areaSize * 0.85f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.55f;
            noise.frequency = 0.3f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = PulseGradient();

            ApplyMaterial(ps, BlendMode.Additive);
            ps.Play();
        }

        // ---- shared ----
        private enum BlendMode { Alpha, Additive }

        private void ApplyMaterial(ParticleSystem ps, BlendMode blend)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader) { name = "CozyParticle" };
            var tex = RoundedSprite.SoftDotTexture();
            mat.mainTexture = tex;
            if (blend == BlendMode.Additive && mat.HasProperty("_SrcBlend") && mat.HasProperty("_DstBlend"))
            {
                // Push toward additive glow for fireflies.
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            }
            renderer.material = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private static Gradient FadeInOutGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.15f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f),
                });
            return g;
        }

        private static Gradient PulseGradient()
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(0.3f, 0.6f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f),
                });
            return g;
        }

        /// <summary>1 = deep night, 0 = daytime, smooth around dawn/dusk.</summary>
        private static float NightAmount(float t)
        {
            if (t < 0.23f || t > 0.80f) return 1f;
            if (t < 0.30f) return Mathf.InverseLerp(0.30f, 0.23f, t);
            if (t > 0.73f) return Mathf.InverseLerp(0.73f, 0.80f, t);
            return 0f;
        }
    }
}
