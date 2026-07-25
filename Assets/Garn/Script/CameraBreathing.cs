using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    public SanitySystem sanity;

    public float maxBreathAmount = 0.05f;
    public float maxBreathSpeed = 3f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float fear = 1f - (sanity.currentSanity / sanity.maxSanity);

        float amount = Mathf.Lerp(0f, maxBreathAmount, fear);
        float speed = Mathf.Lerp(0f, maxBreathSpeed, fear);

        transform.localPosition =
            startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amount;
    }
}