using UnityEngine;

public static class ProceduralAudio {
    public static AudioClip GenerateTone(float frequency, float duration) {
        int sampleRate = 44100;
        int sampleLength = (int)(sampleRate * duration);
        float[] samples = new float[sampleLength];
        for (int i = 0; i < sampleLength; i++) {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }
        AudioClip clip = AudioClip.Create("Tone", sampleLength, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
