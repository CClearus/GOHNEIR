using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Game is quitting...");

        #if UNITY_EDITOR
            // Stops Play Mode inside the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Closes the actual built application (.exe / .app)
            Application.Quit();
        #endif
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
