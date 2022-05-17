using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class ServerIndicator : MonoBehaviour
{
    [SerializeField] private Image image;
    public bool serverIsActive;

    private void Awake()
    {
        image.color = Color.red;

        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.ASCII.GetBytes(SceneManager.GetActiveScene().name);

        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent += OnTransportEvent;
    }

    private void OnTransportEvent(NetworkEvent type, ulong id, ArraySegment<byte> payload, float time)
    {
        // Communication between client and server. Server is online.
        image.color = Color.green;
        serverIsActive = true;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent -= OnTransportEvent;
    }
}