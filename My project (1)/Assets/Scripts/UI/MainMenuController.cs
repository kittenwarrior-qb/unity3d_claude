using System;
using UnityEngine;
using UnityEngine.UI;

namespace CozyStroll.UI
{
    /// <summary>
    /// Drives the cozy main menu: Start hands control to the bootstrapper,
    /// Settings tweaks music volume + time speed, Quit exits.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public GameObject menuRoot;
        public GameObject settingsRoot;

        [Header("Hooks (wired by bootstrapper)")]
        public Action onStartGame;
        public AudioSource music;
        public GameClock clock;

        public void StartGame()
        {
            UISfx.PlayPop();
            gameObject.SetActive(false);
            onStartGame?.Invoke();
        }

        public void OpenSettings()
        {
            UISfx.PlayPop();
            if (menuRoot != null) menuRoot.SetActive(false);
            if (settingsRoot != null) settingsRoot.SetActive(true);
        }

        public void CloseSettings()
        {
            UISfx.PlayPop();
            if (settingsRoot != null) settingsRoot.SetActive(false);
            if (menuRoot != null) menuRoot.SetActive(true);
        }

        public void SetMusicVolume(float v)
        {
            if (music != null) music.volume = v;
        }

        public void SetTimeSpeed(float minutesPerSecond)
        {
            if (clock != null) clock.minutesPerSecond = minutesPerSecond;
        }

        public void Quit()
        {
            UISfx.PlayPop();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
