using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.UI
{
    /// <summary>
    /// Builds the cozy HUD entirely in code (no prefabs/art required):
    ///  • mint rounded clock panel (top-left)
    ///  • circular minimap (top-right)
    ///  • fading control hints (bottom-centre)
    ///  • a centre interaction prompt label
    /// Styling follows UI_STYLE.md (mint & warm pastel, big rounded corners).
    /// </summary>
    public static class HudBuilder
    {
        // Palette (UI_STYLE.md)
        private static readonly Color Mint     = Hex("#A8E6CF");
        private static readonly Color MintDeep  = Hex("#7FD8B6");
        private static readonly Color TextWarm  = Hex("#4A5759");
        private static readonly Color Cream      = Hex("#FBFCF8");

        public static GameObject Build(GameClock clock, Transform player, RenderTexture minimapRT,
                                       out Text interactPrompt)
        {
            var canvasGo = new GameObject("HUD_Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            BuildClock(canvas.transform, clock);
            if (minimapRT != null) BuildMinimap(canvas.transform, minimapRT);
            BuildHints(canvas.transform);
            interactPrompt = BuildInteractPrompt(canvas.transform);

            return canvasGo;
        }

        // ---- Clock panel (top-left) ----
        private static void BuildClock(Transform parent, GameClock clock)
        {
            var panel = NewImage("Clock_Panel", parent, RoundedSprite.RoundedRect(), Mint);
            var rt = panel.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(28f, -28f);
            rt.sizeDelta = new Vector2(190f, 78f);
            AddSoftShadow(panel.gameObject);

            var label = NewText("Clock_Label", panel.transform, "17:00", 40, TextWarm);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            Stretch(label.rectTransform);

            var binder = panel.gameObject.AddComponent<SimpleClockLabel>();
            binder.clock = clock;
            binder.label = label;
        }

        // ---- Minimap (top-right) ----
        private static void BuildMinimap(Transform parent, RenderTexture rt)
        {
            // Mint ring backing.
            var ring = NewImage("Minimap_Ring", parent, RoundedSprite.Circle(), MintDeep);
            var ringRt = ring.rectTransform;
            ringRt.anchorMin = ringRt.anchorMax = new Vector2(1f, 1f);
            ringRt.pivot = new Vector2(1f, 1f);
            ringRt.anchoredPosition = new Vector2(-28f, -28f);
            ringRt.sizeDelta = new Vector2(210f, 210f);
            AddSoftShadow(ring.gameObject);

            // Circular mask holding the render texture.
            var maskGo = new GameObject("Minimap_Mask", typeof(Image), typeof(Mask));
            maskGo.transform.SetParent(ring.transform, false);
            var maskImg = maskGo.GetComponent<Image>();
            maskImg.sprite = RoundedSprite.Circle();
            maskGo.GetComponent<Mask>().showMaskGraphic = false;
            var maskRt = maskImg.rectTransform;
            Stretch(maskRt);
            maskRt.sizeDelta = new Vector2(-12f, -12f); // inset to show the ring

            var rawGo = new GameObject("Minimap_View", typeof(RawImage));
            rawGo.transform.SetParent(maskGo.transform, false);
            var raw = rawGo.GetComponent<RawImage>();
            raw.texture = rt;
            Stretch(raw.rectTransform);

            // Player dot in the middle.
            var dot = NewImage("Player_Dot", maskGo.transform, RoundedSprite.Circle(), Hex("#FF7D9C"));
            dot.rectTransform.sizeDelta = new Vector2(14f, 14f);
            dot.rectTransform.anchoredPosition = Vector2.zero;
        }

        // ---- Control hints (bottom-centre, fades) ----
        private static void BuildHints(Transform parent)
        {
            var go = new GameObject("Control_Hints", typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var group = go.GetComponent<CanvasGroup>();

            var panel = NewImage("Hints_Panel", go.transform, RoundedSprite.RoundedRect(), Cream);
            var c = panel.color; c.a = 0.85f; panel.color = c;
            var rt = panel.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 40f);
            rt.sizeDelta = new Vector2(720f, 64f);

            var label = NewText("Hints_Label", panel.transform,
                "WASD di chuyển · Shift chạy · Space nhảy · E tương tác · P chụp ảnh · V thời tiết",
                23, TextWarm);
            label.alignment = TextAnchor.MiddleCenter;
            Stretch(label.rectTransform);

            var hints = go.AddComponent<ControlHints>();
            hints.group = group;
        }

        // ---- Centre interaction prompt ("Nhấn E để ...") ----
        private static Text BuildInteractPrompt(Transform parent)
        {
            var panel = NewImage("Interact_Prompt", parent, RoundedSprite.RoundedRect(), MintDeep);
            var rt = panel.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -140f);
            rt.sizeDelta = new Vector2(420f, 60f);

            var label = NewText("Interact_Label", panel.transform, "", 26, Cream);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            Stretch(label.rectTransform);

            panel.gameObject.SetActive(false); // shown on demand
            return label;
        }

        // ---------- low-level UI helpers ----------
        private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = Image.Type.Sliced;
            return img;
        }

        private static Text NewText(string name, Transform parent, string text, int size, Color color)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = BuiltinFont();
            t.fontSize = size;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static void AddSoftShadow(GameObject go)
        {
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0.29f, 0.34f, 0.35f, 0.25f);
            sh.effectDistance = new Vector2(0f, -4f);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font BuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Color Hex(string hex)
            => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
