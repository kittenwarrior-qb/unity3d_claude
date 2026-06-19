using System.Collections.Generic;
using UnityEngine;

namespace CozyStroll.Environment
{
    /// <summary>
    /// Procedurally builds a small, tidy cozy town from primitives:
    /// grass ground, a central plaza, cross roads, pastel houses, trees,
    /// a little lake and a few benches. Everything is parented under one root
    /// so the scene stays clean. Deterministic via <see cref="seed"/>.
    /// </summary>
    public class TownGenerator : MonoBehaviour
    {
        [Header("Layout")]
        public float groundSize = 70f;
        public int houseCount = 12;
        public int treeCount = 40;
        public int benchCount = 6;
        public int seed = 1234;

        /// <summary>Points where a player/NPC can comfortably stand & wander.</summary>
        public readonly List<Vector3> WanderPoints = new();
        /// <summary>Bench seats, exposed for interaction setup.</summary>
        public readonly List<Transform> Benches = new();

        private Transform _root;
        private System.Random _rng;

        public Transform Build()
        {
            _rng = new System.Random(seed);
            _root = new GameObject("Town").transform;

            BuildGround();
            BuildRoads();
            BuildPlaza();
            BuildLake(new Vector3(groundSize * 0.28f, 0f, -groundSize * 0.2f), 7f);
            BuildHouses();
            BuildTrees();
            BuildBenches();
            BuildBoundaryHedge();

            return _root;
        }

        // ---- Ground ----
        private void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(_root);
            ground.transform.localScale = Vector3.one * (groundSize / 10f); // Plane is 10 units
            ground.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(PaletteLibrary.Grass);
            ground.isStatic = true;
        }

        // ---- Cross roads ----
        private void BuildRoads()
        {
            float len = groundSize * 0.92f;
            CreateBox("Road_NS", new Vector3(0, 0.02f, 0), new Vector3(4f, 0.04f, len), PaletteLibrary.Path);
            CreateBox("Road_EW", new Vector3(0, 0.02f, 0), new Vector3(len, 0.04f, 4f), PaletteLibrary.Path);
        }

        // ---- Central plaza (ring of pavers + a lamp) ----
        private void BuildPlaza()
        {
            var plaza = CreateCylinder("Plaza", new Vector3(0, 0.03f, 0), 6f, 0.05f, PaletteLibrary.Path);
            plaza.isStatic = true;

            // A central lamp post / landmark.
            CreateBox("Lamp_Post", new Vector3(0, 1.4f, 0), new Vector3(0.18f, 2.8f, 0.18f), PaletteLibrary.TrunkBrown);
            var bulb = CreateSphere("Lamp_Bulb", new Vector3(0, 2.9f, 0), 0.5f, PaletteLibrary.Hex("#FFF3C4"));
            // Make the bulb glow softly.
            var light = bulb.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = PaletteLibrary.Hex("#FFE8B0");
            light.range = 14f;
            light.intensity = 1.2f;
        }

        // ---- Lake ----
        private void BuildLake(Vector3 center, float radius)
        {
            // Slightly sunken water disc + a soft sandy rim.
            CreateCylinder("Lake_Rim", center + Vector3.up * 0.01f, radius + 1.2f, 0.06f, PaletteLibrary.Path);
            var water = CreateCylinder("Lake_Water", center + Vector3.up * 0.02f, radius, 0.04f, PaletteLibrary.Water);
            var m = water.GetComponent<Renderer>().sharedMaterial;
            // Calm reflective-ish look.
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.7f);
            RegisterAvoid(center, radius + 1.5f);
        }

        // ---- Houses ----
        private readonly List<(Vector3 c, float r)> _avoid = new();

        private void BuildHouses()
        {
            int placed = 0, guard = 0;
            while (placed < houseCount && guard++ < houseCount * 30)
            {
                Vector3 p = RandomGroundPoint(8f, groundSize * 0.42f);
                float footprint = 3.5f;
                if (TooClose(p, footprint + 1.5f)) continue;

                BuildHouse(p, footprint);
                RegisterAvoid(p, footprint);
                placed++;
            }
        }

        private void BuildHouse(Vector3 basePos, float footprint)
        {
            var house = new GameObject($"House_{Benches.Count}_{_rng.Next(999)}");
            house.transform.SetParent(_root);
            house.transform.position = basePos;
            house.transform.rotation = Quaternion.Euler(0, _rng.Next(0, 4) * 90f, 0);

            float w = footprint * RandFloat(0.8f, 1.1f);
            float d = footprint * RandFloat(0.8f, 1.1f);
            float h = RandFloat(2.2f, 3.4f);
            Color wall = PaletteLibrary.Walls[_rng.Next(PaletteLibrary.Walls.Length)];
            Color roof = PaletteLibrary.Roofs[_rng.Next(PaletteLibrary.Roofs.Length)];

            var body = CreateBox("Body", new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), wall);
            body.transform.SetParent(house.transform, false);

            // Pyramid-ish roof using a stretched, rotated cube cap.
            var roofGo = CreateBox("Roof", new Vector3(0, h + 0.4f, 0), new Vector3(w * 1.15f, 0.9f, d * 1.15f), roof);
            roofGo.transform.SetParent(house.transform, false);

            // A little door + window for charm.
            var door = CreateBox("Door", new Vector3(0, 0.8f, d * 0.5f + 0.02f), new Vector3(0.8f, 1.6f, 0.1f), PaletteLibrary.Bench);
            door.transform.SetParent(house.transform, false);
            var win = CreateBox("Window", new Vector3(w * 0.28f, h * 0.6f, d * 0.5f + 0.02f), new Vector3(0.7f, 0.7f, 0.1f), PaletteLibrary.Hex("#CDE7FF"));
            win.transform.SetParent(house.transform, false);
        }

        // ---- Trees ----
        private void BuildTrees()
        {
            int placed = 0, guard = 0;
            while (placed < treeCount && guard++ < treeCount * 30)
            {
                Vector3 p = RandomGroundPoint(7f, groundSize * 0.46f);
                if (TooClose(p, 1.8f)) continue;
                BuildTree(p);
                placed++;
            }
        }

        private void BuildTree(Vector3 pos)
        {
            var tree = new GameObject("Tree");
            tree.transform.SetParent(_root);
            tree.transform.position = pos;

            float scale = RandFloat(0.8f, 1.4f);
            var trunk = CreateCylinder("Trunk", new Vector3(0, 0.9f * scale, 0), 0.18f * scale, 0.9f * scale, PaletteLibrary.TrunkBrown);
            trunk.transform.SetParent(tree.transform, false);

            Color leaf = _rng.Next(2) == 0 ? PaletteLibrary.Leaf : PaletteLibrary.LeafSoft;
            var foliage = CreateSphere("Foliage", new Vector3(0, 2.1f * scale, 0), 1.1f * scale, leaf);
            foliage.transform.SetParent(tree.transform, false);
            // A second smaller blob for a fuller, stylised canopy.
            var foliage2 = CreateSphere("Foliage2", new Vector3(0.3f * scale, 2.6f * scale, 0.1f * scale), 0.8f * scale, leaf);
            foliage2.transform.SetParent(tree.transform, false);

            RegisterAvoid(pos, 1.0f * scale);
        }

        // ---- Benches ----
        private void BuildBenches()
        {
            int placed = 0, guard = 0;
            while (placed < benchCount && guard++ < benchCount * 40)
            {
                Vector3 p = RandomGroundPoint(6f, groundSize * 0.4f);
                if (TooClose(p, 2.5f)) continue;

                var bench = new GameObject("Bench");
                bench.transform.SetParent(_root);
                bench.transform.position = p;
                bench.transform.rotation = Quaternion.Euler(0, _rng.Next(0, 360), 0);

                var seat = CreateBox("Seat", new Vector3(0, 0.45f, 0), new Vector3(1.6f, 0.12f, 0.5f), PaletteLibrary.Bench);
                seat.transform.SetParent(bench.transform, false);
                var back = CreateBox("Back", new Vector3(0, 0.8f, -0.2f), new Vector3(1.6f, 0.5f, 0.1f), PaletteLibrary.Bench);
                back.transform.SetParent(bench.transform, false);
                CreateBox("LegL", new Vector3(-0.7f, 0.22f, 0), new Vector3(0.1f, 0.45f, 0.45f), PaletteLibrary.TrunkBrown).transform.SetParent(bench.transform, false);
                CreateBox("LegR", new Vector3(0.7f, 0.22f, 0), new Vector3(0.1f, 0.45f, 0.45f), PaletteLibrary.TrunkBrown).transform.SetParent(bench.transform, false);

                Benches.Add(bench.transform);
                RegisterAvoid(p, 1.5f);
                placed++;
            }
        }

        // ---- Boundary hedge so you can't stroll off the world ----
        private void BuildBoundaryHedge()
        {
            float half = groundSize * 0.5f - 0.5f;
            float t = 1f;     // thickness
            float h = 1.4f;   // height
            Color hedge = PaletteLibrary.GrassDark;
            CreateBox("Hedge_N", new Vector3(0, h * 0.5f, half), new Vector3(groundSize, h, t), hedge);
            CreateBox("Hedge_S", new Vector3(0, h * 0.5f, -half), new Vector3(groundSize, h, t), hedge);
            CreateBox("Hedge_E", new Vector3(half, h * 0.5f, 0), new Vector3(t, h, groundSize), hedge);
            CreateBox("Hedge_W", new Vector3(-half, h * 0.5f, 0), new Vector3(t, h, groundSize), hedge);
        }

        // ---------- helpers ----------
        private GameObject CreateBox(string name, Vector3 worldOrLocalPos, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root);
            go.transform.position = worldOrLocalPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(color);
            go.isStatic = true;
            return go;
        }

        private GameObject CreateCylinder(string name, Vector3 pos, float radius, float halfHeight, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(_root);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(radius * 2f, halfHeight, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(color);
            go.isStatic = true;
            return go;
        }

        private GameObject CreateSphere(string name, Vector3 pos, float radius, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(_root);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = PaletteLibrary.Get(color);
            go.isStatic = true;
            return go;
        }

        private Vector3 RandomGroundPoint(float minR, float maxR)
        {
            float ang = (float)_rng.NextDouble() * Mathf.PI * 2f;
            float r = Mathf.Lerp(minR, maxR, (float)_rng.NextDouble());
            return new Vector3(Mathf.Cos(ang) * r, 0f, Mathf.Sin(ang) * r);
        }

        private void RegisterAvoid(Vector3 c, float r)
        {
            _avoid.Add((c, r));
            // Anything that's a clear standing spot becomes a wander candidate.
            if (r <= 2f) WanderPoints.Add(c);
        }

        private bool TooClose(Vector3 p, float r)
        {
            foreach (var a in _avoid)
                if ((a.c - p).sqrMagnitude < (a.r + r) * (a.r + r)) return true;
            return false;
        }

        private float RandFloat(float a, float b) => Mathf.Lerp(a, b, (float)_rng.NextDouble());
    }
}
