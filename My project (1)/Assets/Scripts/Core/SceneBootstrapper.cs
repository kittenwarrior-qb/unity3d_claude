using UnityEngine;
using UnityEngine.UI;
using CozyStroll.Audio;
using CozyStroll.CameraRig;
using CozyStroll.Environment;
using CozyStroll.Interaction;
using CozyStroll.NPC;
using CozyStroll.Player;
using CozyStroll.UI;

namespace CozyStroll.Core
{
    /// <summary>
    /// One-stop builder for the whole "Cozy Stroll" vertical slice. Press Play and
    /// a complete playable town assembles itself: ground, pastel houses, trees, a
    /// lake, the player + follow camera, warm day/night lighting, ambient music,
    /// footsteps, the cozy HUD (clock + minimap + hints) and little interactables.
    ///
    /// It auto-runs on play via <see cref="AutoBoot"/>, so no manual scene wiring
    /// is needed. You can also drop this component on an empty GameObject to tweak
    /// the inspector values.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("World")]
        public int seed = 1234;
        public int npcCount = 6;
        public int flowerCount = 18;
        public int catCount = 4;
        public float startHour = 17f;

        private static bool _built;
        private bool _owner;

        /// <summary>Auto-spawns the bootstrapper after the scene loads.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (FindObjectOfType<SceneBootstrapper>() != null) return;
            var go = new GameObject("[CozyStroll]");
            go.AddComponent<SceneBootstrapper>();
        }

        private void Awake()
        {
            if (_built) { Destroy(gameObject); return; }
            _built = true;
            _owner = true;
            DontDestroyOnLoad(gameObject);
            BuildEverything();
        }

        private void OnDestroy()
        {
            // Let the next play session rebuild (statics can survive disabled domain reload).
            if (_owner) _built = false;
        }

        private void BuildEverything()
        {
            ConfigureEnvironmentLook();

            // --- Core systems ---
            var manager = gameObject.AddComponent<GameManager>();
            var clock = gameObject.AddComponent<GameClock>();
            clock.startHour = startHour;
            clock.minutesPerSecond = 1f;

            // --- Town ---
            var townGen = gameObject.AddComponent<TownGenerator>();
            townGen.seed = seed;
            Transform town = townGen.Build();

            // --- Lighting (sun + cozy day/night) ---
            Light sun = BuildSun();
            var dayNight = sun.gameObject.AddComponent<DayNightLight>();
            dayNight.clock = clock;
            dayNight.sun = sun;

            // --- Player ---
            var player = BuildPlayer();

            // --- Main follow camera ---
            var cam = BuildMainCamera(player.transform);

            // --- Minimap camera + render texture ---
            RenderTexture minimapRT = BuildMinimapCamera(player.transform);

            // --- HUD ---
            HudBuilder.Build(clock, player.transform, minimapRT, out Text interactPrompt);
            player.GetComponent<PlayerInteractor>().promptLabel = interactPrompt;

            // --- Ambient music ---
            var music = new GameObject("AmbientMusic", typeof(AudioSource), typeof(AmbientMusic));
            music.transform.SetParent(transform);

            // --- Life: interactables + NPCs ---
            ScatterFlowers(town, townGen);
            ScatterCats(town, townGen);
            MakeBenchesSittable(townGen);
            SpawnVillagers(town);

            Debug.Log("[CozyStroll] Town built — go for a stroll! WASD move, Shift run, Space jump, E interact.");
        }

        // ---------------------------------------------------------------
        private void ConfigureEnvironmentLook()
        {
            // Bright pastel sky + soft warm fog for cozy depth.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = PaletteLibrary.Hex("#DFF5EC");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = PaletteLibrary.Hex("#DDF2FF");
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 90f;
        }

        private Light BuildSun()
        {
            // Reuse an existing directional light if the scene has one.
            foreach (var l in FindObjectsOfType<Light>())
                if (l.type == LightType.Directional) return ConfigureSun(l);

            var go = new GameObject("Sun");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            return ConfigureSun(light);
        }

        private Light ConfigureSun(Light light)
        {
            light.color = PaletteLibrary.Hex("#FFF3DD");
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, 160f, 0f);
            return light;
        }

        private PlayerController BuildPlayer()
        {
            var go = new GameObject("Player");
            go.transform.position = new Vector3(0f, 1.2f, -8f);
            go.tag = "Player";

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Cute capsule body + a forward "nose" so facing is readable.
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(PaletteLibrary.Player);

            var nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "Nose";
            Destroy(nose.GetComponent<Collider>());
            nose.transform.SetParent(go.transform, false);
            nose.transform.localPosition = new Vector3(0f, 1.05f, 0.38f);
            nose.transform.localScale = Vector3.one * 0.22f;
            nose.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(PaletteLibrary.PlayerTrim);

            var controller = go.AddComponent<PlayerController>();

            // Footsteps with synthesized clips.
            var audioSrc = go.AddComponent<AudioSource>();
            audioSrc.spatialBlend = 0f;
            var foot = go.AddComponent<FootstepAudio>();
            foot.player = controller;
            foot.footstepClips = ProceduralAudio.BuildFootsteps();

            go.AddComponent<PlayerInteractor>();

            return controller;
        }

        private ThirdPersonCamera BuildMainCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = PaletteLibrary.Hex("#CDEFFF");
            cam.fieldOfView = 60f;
            cam.farClipPlane = 200f;

            var tpc = cam.GetComponent<ThirdPersonCamera>();
            if (tpc == null) tpc = cam.gameObject.AddComponent<ThirdPersonCamera>();
            tpc.target = target;
            return tpc;
        }

        private RenderTexture BuildMinimapCamera(Transform target)
        {
            var rt = new RenderTexture(256, 256, 16) { name = "MinimapRT" };
            rt.Create();

            var go = new GameObject("MinimapCamera");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 22f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = PaletteLibrary.Hex("#BFE3B0");
            cam.cullingMask = ~0;
            cam.targetTexture = rt;
            cam.depth = -10;

            var mini = go.AddComponent<Minimap>();
            mini.target = target;

            return rt;
        }

        // ---------------- life / interactables ----------------
        private void ScatterFlowers(Transform town, TownGenerator gen)
        {
            var rng = new System.Random(seed + 7);
            var colors = new[] { "#FFD3E0", "#FFF3C4", "#E7D6FB", "#CDE7FF", "#FFB7C5" };

            for (int i = 0; i < flowerCount; i++)
            {
                Vector3 p = RandomFlat(rng, gen.groundSize * 0.42f);
                var flower = new GameObject("Flower");
                flower.transform.SetParent(town);
                flower.transform.position = p;

                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "Stem";
                Destroy(stem.GetComponent<Collider>());
                stem.transform.SetParent(flower.transform, false);
                stem.transform.localPosition = new Vector3(0, 0.2f, 0);
                stem.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                stem.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(PaletteLibrary.Leaf);

                var bloom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bloom.name = "Bloom";
                Destroy(bloom.GetComponent<Collider>());
                bloom.transform.SetParent(flower.transform, false);
                bloom.transform.localPosition = new Vector3(0, 0.42f, 0);
                bloom.transform.localScale = Vector3.one * 0.24f;
                bloom.GetComponent<Renderer>().sharedMaterial =
                    PaletteLibrary.Get(PaletteLibrary.Hex(colors[rng.Next(colors.Length)]));

                var trigger = flower.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.5f;

                var it = flower.AddComponent<Interactable>();
                it.promptVerb = "nhặt hoa";
                it.repeatable = false;
                it.bounceOnUse = false;
                var go = flower; // capture
                it.onInteract.AddListener(() => go.SetActive(false));
            }
        }

        private void ScatterCats(Transform town, TownGenerator gen)
        {
            var rng = new System.Random(seed + 13);
            for (int i = 0; i < catCount; i++)
            {
                Vector3 p = RandomFlat(rng, gen.groundSize * 0.38f);
                var cat = new GameObject("Cat");
                cat.transform.SetParent(town);
                cat.transform.position = p;
                cat.transform.rotation = Quaternion.Euler(0, rng.Next(360), 0);

                Color fur = PaletteLibrary.Hex(rng.Next(2) == 0 ? "#D9A066" : "#9B9B9B");

                var bodyGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bodyGo.name = "Body";
                bodyGo.transform.SetParent(cat.transform, false);
                bodyGo.transform.localPosition = new Vector3(0, 0.22f, 0);
                bodyGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
                bodyGo.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
                bodyGo.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(fur);
                Destroy(bodyGo.GetComponent<Collider>());

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(cat.transform, false);
                head.transform.localPosition = new Vector3(0, 0.32f, 0.28f);
                head.transform.localScale = Vector3.one * 0.26f;
                head.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(fur);
                Destroy(head.GetComponent<Collider>());

                var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tail.name = "Tail";
                tail.transform.SetParent(cat.transform, false);
                tail.transform.localPosition = new Vector3(0, 0.34f, -0.28f);
                tail.transform.localRotation = Quaternion.Euler(50, 0, 0);
                tail.transform.localScale = new Vector3(0.05f, 0.18f, 0.05f);
                tail.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(fur);
                Destroy(tail.GetComponent<Collider>());

                var trigger = cat.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.6f;

                var it = cat.AddComponent<Interactable>();
                it.promptVerb = "vỗ mèo";
                it.repeatable = true;
                it.bounceOnUse = true;
            }
        }

        private void MakeBenchesSittable(TownGenerator gen)
        {
            foreach (var bench in gen.Benches)
            {
                var it = bench.gameObject.AddComponent<Interactable>();
                it.promptVerb = "ngồi nghỉ";
                it.repeatable = true;
                it.bounceOnUse = false;
                it.radius = 2.4f;
            }
        }

        private void SpawnVillagers(Transform town)
        {
            var rng = new System.Random(seed + 21);
            var skins = new[] { "#FFB7C5", "#A8E6CF", "#CDE7FF", "#FFE2B8", "#E7D6FB" };

            for (int i = 0; i < npcCount; i++)
            {
                Vector3 p = RandomFlat(rng, 24f);
                var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                npc.name = "Villager";
                npc.transform.SetParent(town);
                npc.transform.position = new Vector3(p.x, 1f, p.z);
                npc.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
                npc.GetComponent<Renderer>().sharedMaterial =
                    PaletteLibrary.Get(PaletteLibrary.Hex(skins[rng.Next(skins.Length)]));

                var w = npc.AddComponent<Wanderer>();
                w.wanderRadius = 18f;
            }
        }

        private Vector3 RandomFlat(System.Random rng, float maxR)
        {
            float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = Mathf.Sqrt((float)rng.NextDouble()) * maxR;
            return new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
        }
    }
}
