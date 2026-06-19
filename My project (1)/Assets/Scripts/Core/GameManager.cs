using UnityEngine;

namespace CozyStroll.Core
{
    /// <summary>
    /// Lightweight bootstrap / global access point.
    /// Keeps things simple for a solo cozy project — expand only when needed.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Quality of life")]
        [Tooltip("Target frame rate. 60 is plenty for a cozy game and saves the GPU.")]
        public int targetFrameRate = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            // Esc frees the cursor so you can stop play comfortably in the editor.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
