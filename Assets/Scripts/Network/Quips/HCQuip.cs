using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HCQuip : NetworkBehaviour
{
    [SerializeField] protected LineRenderer CallLine;
    
    protected QuipData _quip = null;

    protected bool _readyToDespawn = false;

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(_quip == null)
            {
                Debug.LogError("Spawning Quip In Server with NULL Quip");
            }

            int playerIndex = HCNetworkManager.Instance.GetPlayerIndexFromClientId(OwnerClientId);
            Debug.Log($"SERVER: Spawning Quip {_quip.Name} with owner {OwnerClientId} and Player Index {playerIndex}");

            SpawnQuipWithDataClientRpc(_quip.Id);
        }   
    }

    protected virtual void Update()
    {
        if(_readyToDespawn && NetworkManager.Singleton.IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public void Init(QuipData quip)
    {
        _quip = quip;
    }

    [ClientRpc]
    void SpawnQuipWithDataClientRpc(int quipId)
    {
        _quip = GameData.Instance.GetQuipData(quipId);
        int playerIndex = HCNetworkManager.Instance.GetPlayerIndexFromClientId(OwnerClientId);
        Debug.Log($"CLIENT: Spawning Quip {_quip.Name} with owner {OwnerClientId} and Player Index {playerIndex}");

        ShowQuipCallPosition();
        StartCoroutine(WaitForActivation());
    }

    protected virtual void ShowQuipCallPosition()
    {
        Color c = HCColor.GetPlayerColor(HCNetworkManager.Instance.GetPlayerIndexFromClientId(OwnerClientId));
        CallLine.startColor = c;
        c.a = 0f;
        CallLine.endColor = c;
    }

    protected virtual void Activate()
    {
        return;
    }

    IEnumerator WaitForActivation()
    {
        yield return new WaitForSeconds(_quip.ActivationTime);
        Activate();
    }
}
