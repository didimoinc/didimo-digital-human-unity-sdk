using System.Linq;
using System.Net;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace __Didimo.Oculus.Showcase.Scripts.Network
{
    [RequireComponent(typeof(UnityTransport))]
    public class DnsLookupForUNetTransport : MonoBehaviour
    {
        public string address;

        [SerializeField]
        private bool performDNSLookup = true;
        private void Awake()
        {
            if (performDNSLookup && !string.IsNullOrEmpty(address))
            {
                IPAddress ipAddress = Dns.GetHostAddresses(address).First();
                UnityTransport networkTransport = GetComponent<UnityTransport>();

                networkTransport.ConnectionData.Address = ipAddress.MapToIPv4().ToString();
            }
        }

    }
}