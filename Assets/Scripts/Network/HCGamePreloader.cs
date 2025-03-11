using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class HCGamePreloader : NetworkBehaviour
{
    [SerializeField] HCPlayer HCPlayerPrefab;
    [SerializeField] HCBase HCBasePrefab;

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            HCNetworkManager.Instance.OnPlayerDataChanged += HC_OnPlayerDataChanged;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SM_OnLoadEventCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            HCNetworkManager.Instance.OnPlayerDataChanged -= HC_OnPlayerDataChanged;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SM_OnLoadEventCompleted;
        }
    }

    private void SM_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        HCBase hcBase = Instantiate(HCBasePrefab);
        hcBase.GetComponent<NetworkObject>().Spawn();

        foreach(PlayerDataNetwork playerData in HCNetworkManager.Instance.GetPlayersData())
        {
            HCPlayer hcPlayer = Instantiate(HCPlayerPrefab);
            hcPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerData.ClientId);
            hcPlayer.InitializeMaterialList();
        }
    }

    private void HC_OnPlayerDataChanged()
    {
        if(HCNetworkManager.Instance.AllPlayersReady())
        {
            HCNetworkManager.Instance.ResetReadyStatusAll();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }
}
