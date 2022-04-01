using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1001)]
public class DVRNetworkManager : NetworkBehaviour
{
    [SerializeField] List<Transform> slots;
    List<Transform> slotsAllocated = new List<Transform>();

    [SerializeField] List<GameObject> playerPrefabs;
    public Vector3 playerAvatarPositionOffset;
    public List<GameObject> bots;

    private Dictionary<ulong, NetworkObject> connectedPlayers = new Dictionary<ulong, NetworkObject>();

    private Dictionary<ulong, int> clientIdToSlot = new Dictionary<ulong, int>();
    private ClientRemoteController clientRemoteController;

    private void ApprovalCheck(byte[] arg1, ulong arg2, NetworkManager.ConnectionApprovedDelegate callback)
    {
        string scene = System.Text.Encoding.ASCII.GetString(arg1);
        bool approved = scene != null && scene == SceneManager.GetActiveScene().name;
        string approvedStr = approved ? "Approved" : "Rejected";
        Debug.Log($"{approvedStr} client connection {arg2}");
        callback(false, null, approved, null, null);
    }

    public void Awake()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.ASCII.GetBytes(SceneManager.GetActiveScene().name);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        bots = new List<GameObject>();
        foreach (Transform slot in slots)
        {
            bots.Add(slot.GetComponentInChildren<DidimoComponents>(true).gameObject);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
        {
            print($"Client connected with ID : {clientId}");
            SpawnIntoSlotServerRpc(NetworkManager.Singleton.LocalClientId);
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += clientId =>
        {
            print($"Client disconnected with ID : {clientId}");
            if (NetworkManager.Singleton.IsServer)
            {
                DespawnClient(clientId);
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        };
    }

    private void OnValidate()
    {
    }

    public Transform AllocateSlot()
    {
        var slot = slots.Except(slotsAllocated).FirstOrDefault();
        if (slot != null)
        {
            slotsAllocated.Add(slot);
        }

        return slot;
    }

    public void DeallocateSlot(Transform slot)
    {
        slotsAllocated.Remove(slot);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            DespawnClientServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnIntoSlotServerRpc(ulong clientId)
    {
        Debug.Log($"Spawning client with ID: {clientId}");
        Transform slot = AllocateSlot();
        if (slot != null)
        {
            int clientSlot = slots.IndexOf(slot);
            clientIdToSlot[clientId] = clientSlot;

            GameObject go = playerPrefabs[clientSlot];
            go = Instantiate(go, slot.position + playerAvatarPositionOffset, slot.rotation);
            NetworkObject networkObject = go.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId);

            if (!networkObject.IsOwner)
            {
                Camera[] cameras = networkObject.GetComponentsInChildren<Camera>();
                foreach (Camera vrCam in cameras)
                {
                    vrCam.gameObject.SetActive(false);
                }
            }

            go.GetComponentInChildren<ClientRemoteController>().slotID.Value = clientSlot;
            connectedPlayers.Add(clientId, networkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnClientServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            DespawnClient(clientId);
        }
    }

    private void DespawnClient(ulong clientId)
    {
        Debug.Log($"Despawning client with ID: {clientId}");

        if (connectedPlayers.ContainsKey(clientId))
        {
            connectedPlayers[clientId].Despawn();
            connectedPlayers.Remove(clientId);

            int clientSlot = clientIdToSlot[clientId];
            DeallocateSlot(slots[clientSlot]);
            clientIdToSlot.Remove(clientId);
        }
    }
}