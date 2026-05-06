using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("Brak punktów spawnu w SpawnManager!");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject != null)
        {
            playerObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"[SpawnManager] Gracz {clientId} zrespawnowany w {spawnPoint.position}");
        }
        else
        {
            Debug.LogError($"[SpawnManager] Nie znaleziono obiektu gracza dla clientId {clientId}");
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
