using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Netcode.Samples
{
    [DisallowMultipleComponent]
    public class DidimoClientNetworkTransform : DidimoNetworkTransform
    {
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                m_CachedNetworkManager = NetworkManager;
                TryCommitTransformToServer(transform, NetworkManager.ServerTime.Time);
            }
            base.OnNetworkSpawn();
            CanCommitToTransform = IsOwner;
        }

        protected override void Update()
        {
            CanCommitToTransform = IsOwner;
            base.Update();
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsListening))
            {
                if (CanCommitToTransform)
                {
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }
}