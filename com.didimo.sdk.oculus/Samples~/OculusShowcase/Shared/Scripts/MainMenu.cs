using Didimo.Core.Inspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Class to control scene loading in and to the main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Range(0, 5f)] public float transitionTime = 0.8f;
    public string mainMenu = "MainMenu";
    public SceneFade faderPrefab;
    [SerializeField]
    private GameObject serverManager;

    void Awake()
    {
        SceneFade.FadeIn(faderPrefab, transitionTime);
        // Invoke ("_load", 5);
    }

    void _load() => LoadScene("MeetADidimo");

    public virtual void LoadScene(int level)
    {
        if (transitionTime == 0 || !faderPrefab) SceneManager.LoadScene(level);
        else LoadSceneWithTransition(level);
    }

    public void LoadScene(string level)
    {
        if (transitionTime == 0 || !faderPrefab) SceneManager.LoadScene(level);
        else LoadSceneWithTransition(level);
    }


    [Button]
    public void LoadMainMenu()
    {
        LoadScene(mainMenu);
    }

    [Button]
    public void LoadMultiUserScene()
    {
        if (serverManager?.GetComponent<ServerIndicator>()?.serverIsActive ?? false)
        {
            Destroy(NetworkManager.Singleton.gameObject);
            LoadScene("MultiUser");
        }
        else
        {
            LoadScene("MultiUser2");
        }
    }

    private void Update()
    {
        // OVRInput.Update(); // Not needed as OVRManager is in the scene - regardless of what the Oculus website says.
        if (OVRInput.GetDown(OVRInput.Button.Start) && SceneManager.GetActiveScene().name != mainMenu)
        {
            OnExit();
            LoadScene(mainMenu);
        }
    }

    protected virtual void OnExit() { }

    private void LoadSceneWithTransition(string level) =>
        SceneFade.FadeOut(faderPrefab, transitionTime, () => SceneManager.LoadScene(level));

    private void LoadSceneWithTransition(int level) =>
        SceneFade.FadeOut(faderPrefab, transitionTime, () => SceneManager.LoadScene(level));
}
