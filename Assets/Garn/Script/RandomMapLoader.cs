using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomMapLoader : MonoBehaviour
{
    [Header("Map Variants")]
    public string[] mapScenes;

    public void PlayGame()
    {
        if (mapScenes == null || mapScenes.Length == 0)
        {
            Debug.LogError("No map scenes have been assigned!");
            return;
        }

        int randomIndex = Random.Range(0, mapScenes.Length);

        SceneManager.LoadScene(mapScenes[randomIndex]);
    }
}