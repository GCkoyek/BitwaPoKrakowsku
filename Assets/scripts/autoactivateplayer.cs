using Unity.Netcode;
using UnityEngine;

public class PlayerAutoEnable : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[PlayerAutoEnable] Wymusi³em aktywacjê playera po OnNetworkSpawn()");
        }

        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }
    }
}
