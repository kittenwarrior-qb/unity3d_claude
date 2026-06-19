using UnityEngine;
using CozyStroll.Audio;

namespace CozyStroll.UI
{
    /// <summary>
    /// Tiny shared 2D audio source for cute UI clicks. Lazily created, persists
    /// across scene loads. Uses a synthesized "pop" so no asset file is needed.
    /// </summary>
    public static class UISfx
    {
        private static AudioSource _source;
        private static AudioClip _pop;

        public static void PlayPop()
        {
            EnsureSource();
            if (_pop == null) _pop = ProceduralAudio.BuildPop();
            _source.PlayOneShot(_pop, 0.5f);
        }

        private static void EnsureSource()
        {
            if (_source != null) return;
            var go = new GameObject("UISfx");
            Object.DontDestroyOnLoad(go);
            _source = go.AddComponent<AudioSource>();
            _source.spatialBlend = 0f;
            _source.playOnAwake = false;
        }
    }
}
