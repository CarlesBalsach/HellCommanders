using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HCPlayer : NetworkBehaviour
{
    public const int MAX_QUIPS = 12;

    public static HCPlayer LocalInstance;
    public NetworkVariable<int> Character = new NetworkVariable<int>(-1);
    public NetworkVariable<int> Credits = new NetworkVariable<int>(0);
    public NetworkList<int> Quips;
    public NetworkList<int> Materials;

    private void Awake()
    {
        Quips = new NetworkList<int>();
        Materials = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            HCGameManager.Instance.AddPlayer(GetComponent<NetworkObject>().OwnerClientId, this);
            PlayerDataNetwork playerData = HCNetworkManager.Instance.GetPlayerDataFromClientId(GetComponent<NetworkObject>().OwnerClientId);
            CharacterData character = GameData.Instance.GetCharacterData(playerData.CharacterId);
            Init(character);
        }
        
        if(IsOwner)
        {
            LocalInstance = this;
            HCNetworkManager.Instance.ChangePlayerReadyStatus(true);
        }
    }

    void Init(CharacterData character)
    {
        Character.Value = character.Id;
        foreach(QuipData quip in character.Quips)
        {
            Quips.Add(quip.Id);
        }
    }

    public void InitializeMaterialList()
    {
        foreach(MaterialData mat in GameData.Instance.Materials)
        {
            Materials.Add(0);
        }
    }

    public bool CanPayMaterialPrice(Dictionary<MaterialData, int> materialPrice)
    {
        foreach(var kvp in materialPrice)
        {
            if(Materials[kvp.Key.Id] < kvp.Value)
            {
                return false;
            }
        }
        return true;
    }

    [ServerRpc]
    public void LaunchQuipServerRpc(int quipId, Vector3 position, ServerRpcParams rpcParams = default)
    {
        if(Quips.Contains(quipId) || quipId == 0)
        {
            QuipData quip = GameData.Instance.GetQuipData(quipId);
            if(quip.Size > 0 && position.magnitude < 1 + quip.Size / 2f + 0.2f)
            {
                position = position.normalized + position.normalized * (quip.Size / 2f + 0.2f);
            }

            PrefabController.Instance.NetworkSpawnQuip(quip, transform, position, rpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc]
    public void BuyMaterialServerRpc(int materialId)
    {
        if(materialId < Materials.Count && Credits.Value >= GameData.Instance.GetMaterial(materialId).Price)
        {
            Materials[materialId] = Materials[materialId] + 1;
            Credits.Value = Credits.Value - GameData.Instance.GetMaterial(materialId).Price;
        }
    }

    [ServerRpc]
    public void SellMaterialServerRpc(int materialId)
    {
        if(materialId < Materials.Count && Materials[materialId] > 0)
        {
            Materials[materialId] = Materials[materialId] - 1;
            Credits.Value = Credits.Value + GameData.Instance.GetMaterial(materialId).Price / 2;
        }
    }

    [ServerRpc]
    public void InstallQuipServerRpc(int quipId)
    {
        Dictionary<MaterialData, int> materialPrice = GameData.Instance.GetQuipData(quipId).Price;
        if(Quips.Count < MAX_QUIPS && CanPayMaterialPrice(materialPrice))
        {
            foreach(var kvp in materialPrice)
            {
                Materials[kvp.Key.Id] = Materials[kvp.Key.Id] - kvp.Value;
            }
            Quips.Add(quipId);
        }
    }

    [ServerRpc]
    public void DismantleQuipServerRpc(int quipId)
    {
        Dictionary<MaterialData, int> materialPrice = GameData.Instance.GetQuipData(quipId).Price;
        foreach(var kvp in materialPrice)
        {
            Materials[kvp.Key.Id] = Materials[kvp.Key.Id] + kvp.Value / 2;
        }
        Quips.Remove(quipId);
    }
}
