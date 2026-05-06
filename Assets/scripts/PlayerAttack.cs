using Unity.Netcode;
using UnityEngine;

public class PlayerAttackAdvanced : NetworkBehaviour
{
    public float HitRange = 3f;
    public LayerMask whatCanBeHitted;
    public PlayerEquipmentAdvanced equipment;
    public Camera playerCamera;

    void Update()
    {
        if (!IsOwner) return;

        // Atak
        if (Input.GetMouseButtonDown(0))
        {
            equipment?.SwingLocal(); // lokalny efekt
            TryHitServerRpc(playerCamera.transform.position, playerCamera.transform.forward);
            NotifySwingServerRpc(); // synchronizuj ruch
        }

        // Schowaj
        if (Input.GetKeyDown(KeyCode.H))
        {
            equipment?.HideWeaponLocal();
            NotifyHideServerRpc();
        }

        // Wyci¹gnij
        if (Input.GetKeyDown(KeyCode.J))
        {
            equipment?.ShowWeaponLocal();
            NotifyShowServerRpc();
        }
    }

    [ServerRpc]
    void TryHitServerRpc(Vector3 origin, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, HitRange, whatCanBeHitted))
        {
            Debug.Log($"[Server] Gracz {OwnerClientId} trafi³: {hit.collider.name}");
        }
    }

    [ServerRpc]
    void NotifySwingServerRpc(ServerRpcParams rpcParams = default)
    {
        PlaySwingClientRpc();
    }

    [ServerRpc]
    void NotifyHideServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayHideClientRpc();
    }

    [ServerRpc]
    void NotifyShowServerRpc(ServerRpcParams rpcParams = default)
    {
        PlayShowClientRpc();
    }

    [ClientRpc]
    void PlaySwingClientRpc()
    {
        if (IsOwner) return;
        equipment?.SwingLocal(); // ka¿dy gracz odtwarza ruch broni
    }

    [ClientRpc]
    void PlayHideClientRpc()
    {
        if (IsOwner) return;
        equipment?.HideWeaponLocal();
    }

    [ClientRpc]
    void PlayShowClientRpc()
    {
        if (IsOwner) return;
        equipment?.ShowWeaponLocal();
    }
}
