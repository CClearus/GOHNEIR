using UnityEngine;
using UnityEngine.UI;

public class SanityEffects : MonoBehaviour
{
    [Header("References")]
    public SanitySystem sanity;
    public Image vignetteImage;
    public Image fearOverlay;

    [Header("Vignette")]
    [Range(0f, 1f)]
    public float maxVignetteAlpha = 0.75f;

    [Header("Fear Overlay")]
    [Range(0f, 1f)]
    public float maxOverlayAlpha = 0.35f;

    [Header("Breathing")]
    public float calmBreathingBPM = 12f;

    [Header("Random Panic BPM")]
    public float minPanicBPM = 75f;
    public float maxPanicBPM = 85f;

    private float currentPanicBPM;
    private float nextBPMChange;

    void Start()
    {
        currentPanicBPM = Random.Range(minPanicBPM, maxPanicBPM);
        nextBPMChange = Time.time + Random.Range(2f, 5f);
    }

    void Update()
    {
        if (sanity == null)
            return;

        // Change panic BPM every 2-5 seconds
        if (Time.time >= nextBPMChange)
        {
            currentPanicBPM = Random.Range(minPanicBPM, maxPanicBPM);
            nextBPMChange = Time.time + Random.Range(2f, 5f);
        }

        UpdateVignette();
        UpdateFearOverlay();
    }

    void UpdateVignette()
    {
        if (vignetteImage == null)
            return;

        float current = sanity.currentSanity;
        float baseAlpha;

        // 200 -> 100
        if (current >= 100f)
        {
            float t = Mathf.InverseLerp(200f, 100f, current);
            baseAlpha = Mathf.Lerp(0f, 0.12f, t);
        }
        // 100 -> 20
        else if (current >= 20f)
        {
            float t = Mathf.InverseLerp(100f, 20f, current);
            baseAlpha = Mathf.Lerp(0.12f, maxVignetteAlpha, t);
        }
        // 20 -> 0
        else
        {
            baseAlpha = maxVignetteAlpha;
        }

        // Fear level (0 = calm, 1 = insane)
        float fear = 1f - (current / sanity.maxSanity);

        // Breathing speed
        float bpm = Mathf.Lerp(calmBreathingBPM, currentPanicBPM, fear);
        float frequency = bpm / 60f;

        // Breathing wave
        float breath = (Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) + 1f) * 0.5f;

        // Pulse strength
        float pulseStrength = Mathf.Lerp(0.01f, 0.08f, fear);

        float alpha = baseAlpha + breath * pulseStrength;

        Color c = vignetteImage.color;
        c.a = Mathf.Clamp01(alpha);
        vignetteImage.color = c;
    }

    void UpdateFearOverlay()
    {
        if (fearOverlay == null)
            return;

        Color c = fearOverlay.color;

        if (sanity.currentSanity <= 20f)
        {
            float fear = 1f - (sanity.currentSanity / 20f);

            float frequency = currentPanicBPM / 60f;

            float breath = (Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) + 1f) * 0.5f;

            float minAlpha = Mathf.Lerp(0f, 0.10f, fear);
            float maxAlpha = Mathf.Lerp(0.10f, maxOverlayAlpha, fear);

            c.a = Mathf.Lerp(minAlpha, maxAlpha, breath);
        }
        else
        {
            c.a = 0f;
        }

        fearOverlay.color = c;
    }
}