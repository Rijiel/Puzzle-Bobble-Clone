using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public enum Scene
    {
        MainMenuScene,
        GameScene
    }

    public static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        SceneLoader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void QuitGame()
    {
        Application.Quit();
        Debug.LogError("Application Quit");
    }
}
