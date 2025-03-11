using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Mono.CSharp;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class HCBase : NetworkBehaviour
{
    float GATHERING_INTERVAL = 10f;

    public static HCBase Instance = null;

    public NetworkVariable<int> HP = new NetworkVariable<int>(0);
    public NetworkVariable<int> MaxHP = new NetworkVariable<int>(0);
    public NetworkList<int> MaterialsLooted;
    
    int _timesLooted = 0; // increases by 1 every time it gets resources

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MaterialsLooted = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            Init();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadEventCompleted;
            CreateMaterialList();
        }
    }

    void Init()
    {
        // TODO Get Data from Game Data
        HP.Value = 100;
        MaxHP.Value = 100;
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(HCGameManager.Instance != null && HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
            {
                int lootTimes = (int)(HCGameManager.Instance.StageTime.Value / GATHERING_INTERVAL);
                if(_timesLooted < lootTimes)
                {
                    LootResources();
                    _timesLooted++;
                }
            }
        }
    }

    private void OnSceneLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(sceneName == Loader.Scene.GameScene.ToString())
        {
            ClearMaterialsList();
        }
    }

    void CreateMaterialList()
    {
        foreach(var mat in GameData.Instance.Materials)
        {
            MaterialsLooted.Add(0);
        }
    }

    public void ClearMaterialsList()
    {
        _timesLooted = 0;
        for(int i = 0; i < MaterialsLooted.Count; i++)
        {
            MaterialsLooted[i] = 0;
        }
    }

    void LootResources()
    {
        List<MaterialData> materialsAvailable = new List<MaterialData>();
        PlanetDataNetwork planet = HCGameManager.Instance.CurrentPlanet.Value;

        for(int i = 0; i < planet.Materials.Length; i++)
        {
            if(planet.Materials[i])
            {
                materialsAvailable.Add(GameData.Instance.GetMaterial(i));
            }
        }

        MaterialData lootedMaterial = materialsAvailable[UnityEngine.Random.Range(0, materialsAvailable.Count)];
        MaterialsLooted[lootedMaterial.Id] = MaterialsLooted[lootedMaterial.Id] + 1;
    }

    public void SetHP(int hp)
    {
        HP.Value = hp;

        if(hp <= 0 && HCGameManager.Instance != null && HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            HCGameManager.Instance.BaseDestroyed();
        }
    }
}
