using UnityEngine;

public class FearFOV : MonoBehaviour
{
    public Camera cam;
    public SanitySystem sanity;

    void Update()
    {
        float fear = 1f - sanity.currentSanity / sanity.maxSanity;

        cam.fieldOfView =
            Mathf.Lerp(75f, 82f, fear);
    }
}