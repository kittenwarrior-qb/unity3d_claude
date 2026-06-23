using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.CameraRig
{
    /// <summary>
    /// Cozy photo mode: press P to detach a free-fly camera, hide the HUD and
    /// show cinematic letterbox bars. Fly with WASD + QE, look with the mouse,
    /// Shift to move faster. Press Enter to save a screenshot (no UI in the shot).
    /// </summary>
    public class PhotoMode : MonoBehaviour
    {
        public KeyCode toggleKey = KeyCode.P;
        public KeyCode captureKey = KeyCode.Return;
        public float moveSpeed = 8f;
        public float fastMultiplier = 3f;
        public float lookSpeed = 2.5f;

        private Behaviour _tpc;          // ThirdPersonCamera to suspend
        private Behaviour _playerControl; // PlayerController to suspend
        private GameObject _hud;
        private Camera _cam;

        private bool _active;
        private float _yaw, _pitch;
        private CanvasGroup _overlay;
        private Text _hint;

        public void Setup(Camera cam, Behaviour thirdPersonCamera, Behaviour playerControl, GameObject hud)
        {
            _cam = cam;
            _tpc = thirdPersonCamera;
            _playerControl = playerControl;
            _hud = hud;
            BuildOverlay();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                Toggle();

            if (!_active) return;

            FreeLook();
            if (Input.GetKeyDown(captureKey))
                StartCoroutine(Capture());
        }

        private void Toggle()
        {
            _active = !_active;

            if (_tpc != null) _tpc.enabled = !_active;
            if (_playerControl != null) _playerControl.enabled = !_active;
            if (_hud != null) _hud.SetActive(!_active);
            if (_overlay != null) _overlay.gameObject.SetActive(_active);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_active)
            {
                var e = transform.eulerAngles;
                _yaw = e.y;
                _pitch = e.x;
            }
        }

        private void FreeLook()
        {
            _yaw += Input.GetAxis("Mouse X") * lookSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
            Vector3 dir =
                transform.forward * Input.GetAxis("Vertical") +
                transform.right * Input.GetAxis("Horizontal");
            if (Input.GetKey(KeyCode.E)) dir += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) dir += Vector3.down;

            transform.position += dir * speed * Time.unscaledDeltaTime;
        }

        private IEnumerator Capture()
        {
            if (_overlay != null) _overlay.alpha = 0f; // keep bars/hint out of the photo
            yield return new WaitForEndOfFrame();

            string file = $"CozyStroll_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(Application.persistentDataPath, file);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[CozyStroll] 📸 Saved photo: {path}");

            yield return new WaitForEndOfFrame();
            if (_overlay != null)
            {
                _overlay.alpha = 1f;
                if (_hint != null) StartCoroutine(FlashSaved());
            }
        }

        private IEnumerator FlashSaved()
        {
            string original = _hint.text;
            _hint.text = "📸 Đã lưu ảnh!";
            yield return new WaitForSecondsRealtime(1.2f);
            _hint.text = original;
        }

        // ---- Cinematic overlay (letterbox + hint) ----
        private void BuildOverlay()
        {
            var canvasGo = new GameObject("PhotoMode_Canvas",
                typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _overlay = canvasGo.AddComponent<CanvasGroup>();
            _overlay.interactable = false;
            _overlay.blocksRaycasts = false;

            Bar(canvasGo.transform, true);   // top
            Bar(canvasGo.transform, false);  // bottom

            _hint = NewText(canvasGo.transform,
                "Photo Mode  ·  WASD/QE bay  ·  Chuột nhìn  ·  Shift nhanh  ·  Enter chụp  ·  P thoát");
            var rt = _hint.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 36f);
            rt.sizeDelta = new Vector2(1400f, 50f);

            canvasGo.SetActive(false);
        }

        private void Bar(Transform parent, bool top)
        {
            var go = new GameObject(top ? "Bar_Top" : "Bar_Bottom", typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.07f, 0.85f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, top ? 1f : 0f);
            rt.anchorMax = new Vector2(1f, top ? 1f : 0f);
            rt.pivot = new Vector2(0.5f, top ? 1f : 0f);
            rt.sizeDelta = new Vector2(0f, 90f);
            rt.anchoredPosition = Vector2.zero;
        }

        private Text NewText(Transform parent, string text)
        {
            var go = new GameObject("Hint", typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                  ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 26;
            t.color = new Color(0.98f, 0.99f, 0.97f, 0.9f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
