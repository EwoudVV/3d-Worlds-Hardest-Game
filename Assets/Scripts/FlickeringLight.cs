using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FlickeringLight : MonoBehaviour
{
    public Light flickerLight;
    public AudioSource flickerSound;
    public AudioSource buzzingSound;
    public float flickerClipLength = 0.1f;
    public float minIntensity = 0.05f;
    public float maxIntensity = 0.1f;
    public float minFlickerSpeed = 0.005f;
    public float maxFlickerSpeed = 0.01f;
    public float blinkOffTime = 0.05f;
    public float minBurstTime = 0.2f;
    public float maxBurstTime = 1.0f;
    public float minStableTime = 0.1f;
    public float maxStableTime = 0.3f;
    private List<AudioClip> flickerClips = new List<AudioClip>();

    void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();

        if (buzzingSound != null)
        {
            buzzingSound.loop = true;
            buzzingSound.Play();
        }

        if (flickerSound != null)
            ExtractFlickerClips();

        StartCoroutine(FlickerRoutine());
    }

    void ExtractFlickerClips()
    {
        if (flickerSound.clip == null) return;
        float[] samples = new float[flickerSound.clip.samples];
        flickerSound.clip.GetData(samples, 0);
        int sampleRate = flickerSound.clip.frequency;
        int clipLengthSamples = Mathf.RoundToInt(flickerClipLength * sampleRate);
        List<int> peakIndices = new List<int>();
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) > 0.5f)
                peakIndices.Add(i);
        }
        peakIndices = peakIndices.OrderByDescending(i => Mathf.Abs(samples[i])).Take(20).ToList();
        foreach (int index in peakIndices)
        {
            int startSample = Mathf.Clamp(index - clipLengthSamples / 2, 0, samples.Length - clipLengthSamples);
            float[] newClipSamples = new float[clipLengthSamples];
            System.Array.Copy(samples, startSample, newClipSamples, 0, clipLengthSamples);
            AudioClip newClip = AudioClip.Create("FlickerSegment", clipLengthSamples, flickerSound.clip.channels, sampleRate, false);
            newClip.SetData(newClipSamples, 0);
            flickerClips.Add(newClip);
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            flickerLight.intensity = 0;
            yield return new WaitForSeconds(Random.Range(minStableTime, maxStableTime));
            float burstDuration = Random.Range(minBurstTime, maxBurstTime);
            float timer = 0f;
            while (timer < burstDuration)
            {
                float onTime = Random.Range(minFlickerSpeed, maxFlickerSpeed);
                flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
                if (flickerClips.Count > 0)
                {
                    int randomIndex = Random.Range(0, flickerClips.Count);
                    flickerSound.PlayOneShot(flickerClips[randomIndex]);
                }
                yield return new WaitForSeconds(onTime);
                flickerLight.intensity = 0;
                yield return new WaitForSeconds(blinkOffTime);
                timer += onTime + blinkOffTime;
            }
            flickerLight.intensity = maxIntensity;
        }
    }
}
