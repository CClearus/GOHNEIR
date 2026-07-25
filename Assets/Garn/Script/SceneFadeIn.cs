using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public Image blackScreen;
    public float fadeTime = 2f;

    IEnumerator Start()
    {
        // Start fully black
        Color c = blackScreen.color;
        c.a = 1f;
        blackScreen.color = c;

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            c.a = Mathf.Lerp(1f, 0f, timer / fadeTime);
            blackScreen.color = c;

            yield return null;
        }

        c.a = 0f;
        blackScreen.color = c;
    }
}