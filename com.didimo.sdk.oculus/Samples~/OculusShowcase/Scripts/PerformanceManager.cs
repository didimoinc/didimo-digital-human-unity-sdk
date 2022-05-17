using UnityEngine;
using UnityEngine.SceneManagement;

public class PerformanceManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        // start events
        SceneManager.sceneLoaded += (scene, mode) => SceneLoaded(scene, mode);
        SceneManager.sceneUnloaded += (scene) => SceneUnloaded(scene);

        SceneLoaded(gameObject.scene, LoadSceneMode.Single);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    private void SceneUnloaded(Scene scene)
    {

    }
}