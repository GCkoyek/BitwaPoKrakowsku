using UnityEngine;
using Unity.Netcode;

public class NetcodeDebug : MonoBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[NetcodeDebug] NetworkManager.Singleton == null (brak w scenie)");
            return;
        }

        Debug.Log($"[NetcodeDebug] Netcode start. IsServer={NetworkManager.Singleton.IsServer}, IsClient={NetworkManager.Singleton.IsClient}");

        NetworkManager.Singleton.OnClientConnectedCallback += id =>
        {
            Debug.Log($"[NetcodeDebug] Client connected: {id} (IsServer={NetworkManager.Singleton.IsServer})");
        };

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("[NetcodeDebug] Server started!");
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += id =>
        {
            Debug.Log($"[NetcodeDebug] Client disconnected: {id}");
        };
    }
}
