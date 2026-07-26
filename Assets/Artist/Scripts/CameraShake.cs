using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Default Shake Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultMagnitude = 0.3f;

    private Vector3 originalPos;

    private void OnEnable()
    {
        originalPos = transform.localPosition;

        TriggerShake();
    }

    public void TriggerShake(float duration = -1f, float magnitude = -1f)
    {
        float dur = duration > 0 ? duration : defaultDuration;
        float mag = magnitude > 0 ? magnitude : defaultMagnitude;

        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(dur, mag));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Pick a random offset within a sphere
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        // Reset camera position back to normal
        transform.localPosition = originalPos;
    }
}