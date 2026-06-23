using UnityEngine;

namespace CozyStroll.UI
{
    /// <summary>
    /// Generates UI sprites at runtime (no imported art needed):
    /// a soft rounded-rect panel (9-sliced) and a filled circle for the minimap mask.
    /// </summary>
    public static class RoundedSprite
    {
        /// <summary>Rounded rectangle, white so it can be tinted by the Image colour. 9-sliced.</summary>
        public static Sprite RoundedRect(int size = 48, int radius = 16)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "CozyRoundedRect"
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float a = RoundedRectAlpha(x, y, size, size, radius);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            tex.SetPixels32(pixels);
            tex.Apply();

            var border = new Vector4(radius, radius, radius, radius);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, border);
        }

        /// <summary>Solid white circle (for masking the minimap into a disc).</summary>
        public static Sprite Circle(int size = 128)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "CozyCircle"
            };

            float c = (size - 1) * 0.5f;
            float r = size * 0.5f - 1f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = Mathf.Clamp01(r - d); // 1px soft edge
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>A soft radial dot texture for particles (leaves, fireflies).</summary>
        public static Texture2D SoftDotTexture(int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "CozySoftDot"
            };

            float c = (size - 1) * 0.5f;
            float r = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                float a = Mathf.Clamp01(1f - d);
                a = a * a; // softer falloff
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static float RoundedRectAlpha(int x, int y, int w, int h, int radius)
        {
            float dx = Mathf.Min(x, w - 1 - x);
            float dy = Mathf.Min(y, h - 1 - y);
            if (dx >= radius || dy >= radius) return 1f; // straight edges / middle

            float cx = dx < radius ? radius : dx;
            float cy = dy < radius ? radius : dy;
            float dist = Mathf.Sqrt((cx - dx) * (cx - dx) + (cy - dy) * (cy - dy));
            return Mathf.Clamp01(radius - dist); // soft 1px AA edge
        }
    }
}
