using UnityEngine;

namespace CozyStroll.Audio
{
    /// <summary>
    /// Synthesizes simple placeholder audio at runtime so the game has sound
    /// with zero imported asset files. Replace with real Kenney/Freesound clips later.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        /// <summary>A few short, soft "step" thuds with slight variation.</summary>
        public static AudioClip[] BuildFootsteps(int count = 4)
        {
            var clips = new AudioClip[count];
            for (int i = 0; i < count; i++)
                clips[i] = BuildFootstep(120f + i * 18f, 0.10f + i * 0.01f, i);
            return clips;
        }

        private static AudioClip BuildFootstep(float baseFreq, float dur, int variant)
        {
            int n = Mathf.CeilToInt(SampleRate * dur);
            var data = new float[n];
            var rng = new System.Random(1000 + variant);

            for (int s = 0; s < n; s++)
            {
                float t = s / (float)SampleRate;
                // Fast exponential decay -> a soft thud.
                float env = Mathf.Exp(-t * 38f);
                // Low body tone + a touch of filtered noise for the "scuff".
                float body = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.35f;
                data[s] = (body * 0.7f + noise) * env * 0.6f;
            }

            var clip = AudioClip.Create($"step_{variant}", n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// A gentle, slowly-evolving lo-fi pad that loops seamlessly.
        /// Built from a soft major chord + slow tremolo.
        /// </summary>
        public static AudioClip BuildAmbientPad(float seconds = 8f)
        {
            int n = Mathf.CeilToInt(SampleRate * seconds);
            var data = new float[n];

            // A warm, open chord (A major-ish): A2, E3, A3, C#4.
            float[] freqs = { 110.00f, 164.81f, 220.00f, 277.18f };
            float[] gains = { 0.45f, 0.30f, 0.25f, 0.18f };

            float loopFreq = 1f / seconds; // ensures phase returns to start for seamless loop

            for (int s = 0; s < n; s++)
            {
                float t = s / (float)SampleRate;
                float val = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    // Snap each partial to an integer number of cycles over the loop
                    // so the waveform is continuous at the loop boundary.
                    float cycles = Mathf.Round(freqs[k] * seconds);
                    float f = cycles / seconds;
                    val += Mathf.Sin(2f * Mathf.PI * f * t) * gains[k];
                }
                // Slow tremolo / breathing (also loop-aligned).
                float trem = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * loopFreq * t);
                data[s] = val * 0.18f * trem;
            }

            var clip = AudioClip.Create("ambient_pad", n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
