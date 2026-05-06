using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerEquipmentAdvanced : NetworkBehaviour
{
    [Header("Bronie (dzieci RightHand)")]
    public WeaponData[] allWeapons;
    public Transform weaponHolder;

    private WeaponData currentWeapon;
    private bool isSwinging = false;
    private bool isHidden = false;

    private void OnEnable()
    {
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"[{OwnerClientId}] Player aktywowany rêcznie z disabled.");
        }

        StartCoroutine(WaitForSpawnThenInit());
    }

    private IEnumerator WaitForSpawnThenInit()
    {
        // Poczekaj, a¿ NetworkObject zostanie zespawnowany (wa¿ne!)
        while (NetworkObject == null || !NetworkObject.IsSpawned)
            yield return null;

        yield return new WaitForSeconds(0.2f);

        if (IsServer)
        {
            Debug.Log($"[{OwnerClientId}] Server: przydzielam broñ po spawnie.");
            StartCoroutine(ServerAssignWeaponDelayed());
        }
        else if (IsClient && IsOwner)
        {
            Debug.Log($"[{OwnerClientId}] Client: proszê serwer o broñ (RequestWeaponServerRpc).");
            RequestWeaponServerRpc();
        }
    }





    // ------------------ SERVER: LOSOWANIE ------------------

    private IEnumerator ServerAssignWeaponDelayed()
    {
        yield return new WaitForSeconds(0.3f);
        if (!IsServer) yield break;

        int randomIndex = Random.Range(0, allWeapons.Length);
        Debug.Log($"[Server] Losujê broñ {randomIndex} ({allWeapons[randomIndex].name}) dla {OwnerClientId}");

        EquipWeapon(randomIndex);
        EquipWeaponClientRpc(randomIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestWeaponServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong senderId = rpcParams.Receive.SenderClientId;
        int randomIndex = Random.Range(0, allWeapons.Length);
        Debug.Log($"[ServerRpc] Klient {senderId} poprosi³ o broñ {randomIndex}.");

        EquipWeapon(randomIndex);
        EquipWeaponClientRpc(randomIndex, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { senderId }
            }
        });
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(int weaponIndex, ClientRpcParams rpcParams = default)
    {
        EquipWeapon(weaponIndex);
    }

    public void EquipWeapon(int weaponIndex)
    {
        if (allWeapons == null || allWeapons.Length == 0)
            return;

        if (weaponIndex < 0 || weaponIndex >= allWeapons.Length)
            return;

        foreach (var weapon in allWeapons)
        {
            if (weapon.weaponObject != null)
                weapon.weaponObject.SetActive(false);
        }

        currentWeapon = allWeapons[weaponIndex];
        if (currentWeapon.weaponObject != null)
        {
            currentWeapon.weaponObject.SetActive(true);
            Debug.Log($"[{OwnerClientId}] Aktywowano broñ: {currentWeapon.name}");
        }
    }

    // ------------------ ANIMACJE ------------------

    public void SwingLocal()
    {
        if (isSwinging || currentWeapon?.weaponObject == null) return;

        StartCoroutine(SwingRoutine());
        SwingServerRpc();
    }

    [ServerRpc]
    private void SwingServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        SwingClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.ConnectedClientsIds
                    .Where(id => id != sender)
                    .ToList()
            }
        });
    }

    [ClientRpc]
    private void SwingClientRpc(ClientRpcParams rpcParams = default)
    {
        if (!IsOwner && currentWeapon != null && !isSwinging)
            StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;

        float swingTime = 1f / currentWeapon.swingSpeed;
        float elapsed = 0f;

        Quaternion startRot = currentWeapon.weaponObject.transform.localRotation;
        Quaternion midRot = startRot * Quaternion.Euler(currentWeapon.swingAngleX, 0, currentWeapon.swingAngleZ);

        while (elapsed < swingTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swingTime);
            currentWeapon.weaponObject.transform.localRotation = Quaternion.Slerp(startRot, midRot, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < swingTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swingTime);
            currentWeapon.weaponObject.transform.localRotation = Quaternion.Slerp(midRot, startRot, t);
            yield return null;
        }

        isSwinging = false;
    }

    // ------------------ CHOWANIE / WYCI¥GANIE ------------------

    public void HideWeaponLocal()
    {
        if (isHidden || currentWeapon?.weaponObject == null) return;
        currentWeapon.weaponObject.SetActive(false);
        isHidden = true;
        HideWeaponServerRpc();
    }

    public void ShowWeaponLocal()
    {
        if (!isHidden || currentWeapon?.weaponObject == null) return;
        currentWeapon.weaponObject.SetActive(true);
        isHidden = false;
        ShowWeaponServerRpc();
    }

    [ServerRpc]
    private void HideWeaponServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        HideWeaponClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.ConnectedClientsIds
                    .Where(id => id != sender)
                    .ToList()
            }
        });
    }

    [ServerRpc]
    private void ShowWeaponServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;
        ShowWeaponClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.ConnectedClientsIds
                    .Where(id => id != sender)
                    .ToList()
            }
        });
    }

    [ClientRpc]
    private void HideWeaponClientRpc(ClientRpcParams rpcParams = default)
    {
        if (currentWeapon?.weaponObject != null)
            currentWeapon.weaponObject.SetActive(false);
        isHidden = true;
    }

    [ClientRpc]
    private void ShowWeaponClientRpc(ClientRpcParams rpcParams = default)
    {
        if (currentWeapon?.weaponObject != null)
            currentWeapon.weaponObject.SetActive(true);
        isHidden = false;
    }
}
