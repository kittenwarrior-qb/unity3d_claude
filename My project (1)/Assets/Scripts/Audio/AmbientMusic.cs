using UnityEngine;

namespace CozyStroll.Audio
{
    /// <summary>
    /// Loops a gentle ambient pad. If no clip is assigned, one is synthesized
    /// procedurally (see <see cref="ProceduralAudio"/>) so there's always sound.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbientMusic : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.18f;

        private void Awake()
        {
            var src = GetComponent<AudioSource>();
            if (clip == null) clip = ProceduralAudio.BuildAmbientPad();

            src.clip = clip;
            src.loop = true;
            src.volume = volume;
            src.spatialBlend = 0f; // 2D background music
            src.playOnAwake = true;
            src.Play();
        }
    }
}
