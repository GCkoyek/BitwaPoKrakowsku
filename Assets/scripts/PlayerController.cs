using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;

    [Header("Optional FP/TP models")]
    public GameObject firstPersonObjects;
    public GameObject thirdPersonModel;

    [Header("Equipment")]
    public PlayerEquipmentAdvanced equipment;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (firstPersonObjects != null) firstPersonObjects.SetActive(false);
            if (thirdPersonModel != null) thirdPersonModel.SetActive(true);
            return;
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
        }

        if (firstPersonObjects != null) firstPersonObjects.SetActive(true);
        if (thirdPersonModel != null) thirdPersonModel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"[PlayerController] Spawned local player: {gameObject.name}");

        // 💥 Losowanie broni
        if (IsServer)
        {
            int randomIndex = UnityEngine.Random.Range(0, equipment.allWeapons.Length);
            SetWeaponClientRpc(randomIndex);
        }
        else
        {
            RequestWeaponServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWeaponServerRpc(ServerRpcParams rpcParams = default)
    {
        int randomIndex = UnityEngine.Random.Range(0, equipment.allWeapons.Length);
        SetWeaponClientRpc(randomIndex, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        });
    }

    [ClientRpc]
    private void SetWeaponClientRpc(int weaponIndex, ClientRpcParams rpcParams = default)
    {
        equipment.EquipWeapon(weaponIndex);
    }
}
