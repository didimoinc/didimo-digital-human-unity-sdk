using System.Collections;
using Didimo.Core.Inspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// If not in editor (built application), will start server automatically on server builds, or client on non-server builds
[RequireComponent(typeof(NetworkManager))]
public class DidimoNetworkBootstrapManager : MonoBehaviour
{
    [Tooltip("Should we automatically start as a client if not in editor and not in batch mode?")]
    public bool autoStartAsClientIfNotEditorAndNotBatch = true;

    [Tooltip("Should we automatically start as a server if in batch mode?")]
    public bool autoStartAsServerIfBatchMode = true;

    IEnumerator StartClientTimer()
    {
        while (true)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StartClient();
            }

            yield return new WaitForSeconds(3f);
        }
    }

    private void Start()
    {
        if (Application.isBatchMode && autoStartAsServerIfBatchMode)
        {
            NetworkManager.Singleton.StartServer();
        }
        else if (autoStartAsClientIfNotEditorAndNotBatch)
        {
            NetworkManager.Singleton.StartClient();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Destroy(gameObject);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /* private void OnGUI()
     {
         var networkManager = NetworkManager.Singleton;
         if (!networkManager.IsClient && !networkManager.IsServer)
         {
             GUILayout.BeginArea(new Rect(10, 10, 300, 300));

             if (GUILayout.Button("Host"))
             {
                 networkManager.StartHost();
             }

             if (GUILayout.Button("Client"))
             {
                 networkManager.StartClient();
             }

             if (GUILayout.Button("Server"))
             {
                 networkManager.StartServer();
             }

             GUILayout.EndArea();
         }
     } */

    [Button]
    public void DisconnectAClient()
    {
        NetworkManager.Singleton.Shutdown();
    }
}