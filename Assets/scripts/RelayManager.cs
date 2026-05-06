using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    [Header("Lobby References")]
    public GameObject lobbyCanvas;
    public Camera lobbyCamera;
    public RelayUI relayUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (NetworkManager.Singleton != null)
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);

        Debug.Log("[RelayManager] Awake()");
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("[RelayManager] Unity Services zainicjalizowane");

        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
        {
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("[RelayManager] Zalogowano anonimowo.");
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.LogError("[RelayManager] Brak NetworkManager.Singleton!");
        }
    }

    // ---------------------- CREATE RELAY ----------------------
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            Debug.Log($"[RelayManager] Host uruchomiony. Kod: {joinCode}");

            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            HideLobby();

            if (relayUI != null)
                relayUI.SetJoinCode(joinCode);

            // 🔥 Naprawa: czekamy aż NGO w pełni zespawnuje hosta
            StartCoroutine(EnsureHostPlayerActive());

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RelayManager] Błąd CreateRelay(): {e}");
            return null;
        }
    }

    private IEnumerator EnsureHostPlayerActive()
    {
        Debug.Log("[RelayManager] Czekam na PlayerObject hosta...");

        NetworkObject playerObj = null;
        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObj != null)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (playerObj == null)
        {
            Debug.LogWarning("[RelayManager] Host nie ma PlayerObject po timeout!");
            yield break;
        }

        yield return null; // ⏳ Poczekaj jedną klatkę po spawnie

        var playerGO = playerObj.gameObject;

        if (!playerGO.activeSelf)
        {
            playerGO.SetActive(true);
            Debug.Log("[RelayManager] Wymusiłem aktywację playera hosta.");
        }

        foreach (Transform child in playerGO.transform)
        {
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }

        Debug.Log("[RelayManager] Player hosta jest aktywny ✅");
    }

    // ---------------------- JOIN RELAY ----------------------
    public async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
            );

            bool started = NetworkManager.Singleton.StartClient();
            Debug.Log($"[RelayManager] StartClient() => {started}");

            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            HideLobby();

            if (started)
                StartCoroutine(WaitForClientPlayerActive());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RelayManager] JoinRelay error: {e}");
        }
    }

    private IEnumerator WaitForClientPlayerActive()
    {
        Debug.Log("[RelayManager] Czekam na PlayerObject klienta...");

        NetworkObject playerObj = null;
        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObj != null)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (playerObj == null)
        {
            Debug.LogWarning("[RelayManager] Klient nie ma PlayerObject po timeout!");
            yield break;
        }

        yield return null;

        var playerGO = playerObj.gameObject;
        if (!playerGO.activeSelf)
        {
            playerGO.SetActive(true);
            Debug.Log("[RelayManager] Wymusiłem aktywację playera klienta.");
        }

        foreach (Transform child in playerGO.transform)
        {
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }

        Debug.Log("[RelayManager] Player klienta aktywny ✅");
    }

    // ---------------------- CALLBACKS ----------------------
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[RelayManager] ClientConnected: {clientId}");

        if (NetworkManager.Singleton.IsServer)
        {
            var playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
            if (playerPrefab == null)
            {
                Debug.LogError("[RelayManager] Brak PlayerPrefab w NetworkManager!");
                return;
            }

            StartCoroutine(SpawnPlayerForClient(clientId, playerPrefab));
        }
    }




    private IEnumerator SpawnPlayerForClient(ulong clientId, GameObject prefab)
    {
        yield return null; // ⏳ Poczekaj 1 klatkę

        if (!NetworkManager.Singleton.IsServer)
            yield break;

        var newPlayer = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        if (!newPlayer.activeSelf)
            newPlayer.SetActive(true);

        var netObj = newPlayer.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"[RelayManager] Player zespawnowany dla klienta {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[RelayManager] ClientDisconnected: {clientId}");
    }

    private void HideLobby()
    {
        if (lobbyCanvas != null)
            lobbyCanvas.SetActive(false);
        if (lobbyCamera != null)
            lobbyCamera.gameObject.SetActive(false);
    }
}
