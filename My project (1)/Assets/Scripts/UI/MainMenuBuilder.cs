using System;
using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.UI
{
    /// <summary>
    /// Builds the cozy main menu entirely in code: a soft dim over the live scene,
    /// a big "Cozy Stroll" title, Bắt đầu / Cài đặt / Thoát buttons, and a small
    /// settings panel (music volume + time speed). Style per UI_STYLE.md.
    /// </summary>
    public static class MainMenuBuilder
    {
        private static readonly Color Mint     = Hex("#A8E6CF");
        private static readonly Color MintDeep  = Hex("#7FD8B6");
        private static readonly Color Pink       = Hex("#FFD3E0");
        private static readonly Color TextWarm   = Hex("#4A5759");
        private static readonly Color Cream       = Hex("#FBFCF8");

        public static MainMenuController Build(Action onStartGame, AudioSource music, GameClock clock)
        {
            var canvasGo = new GameObject("MainMenu_Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // above HUD
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            var controller = canvasGo.AddComponent<MainMenuController>();
            controller.onStartGame = onStartGame;
            controller.music = music;
            controller.clock = clock;

            // Soft dim so the live town reads as a calm background.
            var dim = NewImage("Dim", canvas.transform, null, new Color(0.98f, 0.99f, 0.97f, 0.35f));
            Stretch(dim.rectTransform);

            BuildMainPanel(canvas.transform, controller);
            BuildSettingsPanel(canvas.transform, controller, music, clock);

            return controller;
        }

        // ---- Main panel ----
        private static void BuildMainPanel(Transform parent, MainMenuController controller)
        {
            var panel = new GameObject("Menu_Root", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var prt = panel.GetComponent<RectTransform>();
            Stretch(prt);
            controller.menuRoot = panel;

            var title = NewText("Title", panel.transform, "Cozy Stroll", 110, MintDeep);
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            var trt = title.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.72f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(1200f, 180f);
            AddTextShadow(title.gameObject);

            var subtitle = NewText("Subtitle", panel.transform, "đi dạo thong thả ~", 34, TextWarm);
            subtitle.alignment = TextAnchor.MiddleCenter;
            var srt = subtitle.rectTransform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.62f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(800f, 60f);

            float y = 0.46f;
            MakeButton(panel.transform, "Bắt đầu", Mint, new Vector2(0.5f, y), controller.StartGame);
            MakeButton(panel.transform, "Cài đặt", Mint, new Vector2(0.5f, y - 0.13f), controller.OpenSettings);
            MakeButton(panel.transform, "Thoát", Pink, new Vector2(0.5f, y - 0.26f), controller.Quit);
        }

        // ---- Settings panel ----
        private static void BuildSettingsPanel(Transform parent, MainMenuController controller,
                                               AudioSource music, GameClock clock)
        {
            var panelImg = NewImage("Settings_Root", parent, RoundedSprite.RoundedRect(), Cream);
            var rt = panelImg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(640f, 460f);
            controller.settingsRoot = panelImg.gameObject;

            var header = NewText("Header", panelImg.transform, "Cài đặt", 56, MintDeep);
            header.alignment = TextAnchor.MiddleCenter;
            header.fontStyle = FontStyle.Bold;
            var hrt = header.rectTransform;
            hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.86f);
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.sizeDelta = new Vector2(400f, 80f);

            MakeSlider(panelImg.transform, "Âm lượng nhạc", new Vector2(0.5f, 0.64f),
                0f, 0.5f, music != null ? music.volume : 0.18f, controller.SetMusicVolume);

            MakeSlider(panelImg.transform, "Tốc độ thời gian", new Vector2(0.5f, 0.42f),
                0.25f, 10f, clock != null ? clock.minutesPerSecond : 1f, controller.SetTimeSpeed);

            MakeButton(panelImg.transform, "Quay lại", Mint, new Vector2(0.5f, 0.14f), controller.CloseSettings,
                width: 280f, height: 64f, fontSize: 30);

            controller.settingsRoot.SetActive(false);
        }

        // ---- Button factory ----
        private static void MakeButton(Transform parent, string text, Color color, Vector2 anchor,
                                       UnityEngine.Events.UnityAction onClick,
                                       float width = 380f, float height = 78f, int fontSize = 38)
        {
            var img = NewImage("Btn_" + text, parent, RoundedSprite.RoundedRect(), color);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            AddSoftShadow(img.gameObject);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.06f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            img.gameObject.AddComponent<UiButtonBounce>();

            var label = NewText("Label", img.transform, text, fontSize, TextWarm);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            Stretch(label.rectTransform);
        }

        // ---- Slider factory ----
        private static void MakeSlider(Transform parent, string caption, Vector2 anchor,
                                       float min, float max, float value, UnityEngine.Events.UnityAction<float> onChange)
        {
            var label = NewText("Cap_" + caption, parent, caption, 26, TextWarm);
            label.alignment = TextAnchor.LowerLeft;
            var lrt = label.rectTransform;
            lrt.anchorMin = lrt.anchorMax = anchor;
            lrt.pivot = new Vector2(0.5f, 0f);
            lrt.anchoredPosition = new Vector2(0f, 12f);
            lrt.sizeDelta = new Vector2(460f, 40f);

            var sliderGo = new GameObject("Slider_" + caption, typeof(RectTransform));
            sliderGo.transform.SetParent(parent, false);
            var srt = sliderGo.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = anchor;
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0f, 0f);
            srt.sizeDelta = new Vector2(460f, 30f);
            var slider = sliderGo.AddComponent<Slider>();

            var bg = NewImage("Background", sliderGo.transform, RoundedSprite.RoundedRect(24, 12), Hex("#DDEFE8"));
            Stretch(bg.rectTransform);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            Stretch(fart);
            var fill = NewImage("Fill", fillArea.transform, RoundedSprite.RoundedRect(24, 12), MintDeep);
            Stretch(fill.rectTransform);

            var handle = NewImage("Handle", sliderGo.transform, RoundedSprite.Circle(), Cream);
            handle.rectTransform.sizeDelta = new Vector2(34f, 34f);
            AddSoftShadow(handle.gameObject);

            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(onChange);
        }

        // ---------- helpers ----------
        private static void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null) return;
            var go = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
            img.color = color;
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
            sh.effectDistance = new Vector2(0f, -5f);
        }

        private static void AddTextShadow(GameObject go)
        {
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(1f, 1f, 1f, 0.6f);
            sh.effectDistance = new Vector2(0f, -3f);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Font BuiltinFont()
            => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        private static Color Hex(string hex)
            => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
