using System.Collections.Generic;
using UnityEngine;

namespace CozyStroll.Environment
{
    /// <summary>
    /// Builds and caches the cozy pastel materials used across the town.
    /// Colours follow UI_STYLE.md (mint & warm pastels). Uses the URP/Lit shader
    /// so everything renders correctly under the Universal Render Pipeline.
    /// </summary>
    public static class PaletteLibrary
    {
        // --- Palette (mint & warm pastel) ---
        public static readonly Color Grass     = Hex("#BFE3B0");
        public static readonly Color GrassDark = Hex("#A9D89C");
        public static readonly Color Path      = Hex("#E8D8B0");
        public static readonly Color Water      = Hex("#A9DCEB");
        public static readonly Color TrunkBrown = Hex("#9A6F4E");
        public static readonly Color Leaf       = Hex("#9FD68A");
        public static readonly Color LeafSoft   = Hex("#B7E0A0");
        public static readonly Color Player      = Hex("#FF9E7D");
        public static readonly Color PlayerTrim  = Hex("#4A5759");
        public static readonly Color Bench       = Hex("#C98E6B");

        // Pastel wall + roof options for variety.
        public static readonly Color[] Walls =
        {
            Hex("#FBE7D6"), Hex("#FFD3E0"), Hex("#A8E6CF"),
            Hex("#CDE7FF"), Hex("#FFF3C4"), Hex("#E7D6FB"),
        };
        public static readonly Color[] Roofs =
        {
            Hex("#E07A5F"), Hex("#7FD8B6"), Hex("#E0917A"),
            Hex("#6FB8D8"), Hex("#D88FB0"), Hex("#C98E6B"),
        };

        private static readonly Dictionary<Color, Material> _cache = new();
        private static Shader _litShader;

        private static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    // URP lit, with sensible fallbacks if the project ever changes pipeline.
                    _litShader = Shader.Find("Universal Render Pipeline/Lit")
                              ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                              ?? Shader.Find("Standard");
                }
                return _litShader;
            }
        }

        /// <summary>Get (or create) a flat-lit material for a colour. Cached by colour.</summary>
        public static Material Get(Color color)
        {
            if (_cache.TryGetValue(color, out var existing) && existing != null)
                return existing;

            var mat = new Material(LitShader) { name = "Cozy_" + ColorUtility.ToHtmlStringRGB(color) };
            SetColor(mat, color);
            // Soft, matte cozy look: low smoothness.
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.1f);
            _cache[color] = mat;
            return mat;
        }

        private static void SetColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.color = color;
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        }
    }
}
