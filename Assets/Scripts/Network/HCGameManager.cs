using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;


public class HCGameManager : NetworkBehaviour
{
    const float LANDING_TIME = 5f;

    public enum GameState {
        PRELOADING,
        GAME_LANDING_PLANET,
        GAME_ACTIVE,
        GAME_DEPARTING_PLANET,
        GAME_FAILED,
        STARSHIP,
    }

    public static HCGameManager Instance = null;

    // Only populated in Server
    Dictionary<ulong, HCPlayer> Players = new Dictionary<ulong, HCPlayer>();

    public NetworkVariable<float> StageTime = new NetworkVariable<float>(0f);
    public NetworkVariable<GameState> State = new NetworkVariable<GameState>(GameState.PRELOADING);
    public NetworkVariable<int> Seed = new NetworkVariable<int>(0);
    public NetworkVariable<int> EnemiesKilled = new NetworkVariable<int>(0);
    public NetworkVariable<int> Layer = new NetworkVariable<int>(0);
    public NetworkVariable<PlanetDataNetwork> CurrentPlanet;
    public NetworkList<PlanetDataNetwork> PlanetChoices;
    public NetworkVariable<int> PlanetChoice = new NetworkVariable<int>(-1);

    float _internalChrono = 0f;

    private void Awake()
    {
        PlanetChoices = new NetworkList<PlanetDataNetwork>();
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(State.Value == GameState.GAME_LANDING_PLANET)
            {
                _internalChrono += Time.deltaTime;
                if(_internalChrono >= LANDING_TIME)
                {
                    State.Value = GameState.GAME_ACTIVE;
                    StageTime.Value = StageTime.Value + Time.deltaTime;
                }
            }
            if(State.Value == GameState.GAME_ACTIVE)
            {
                StageTime.Value += Time.deltaTime;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        State.OnValueChanged += OnStateValueChanged;
        Seed.OnValueChanged += OnSeedValueChanged;

        if(NetworkManager.Singleton.IsServer)
        {
            HCNetworkManager.Instance.OnPlayerDataChanged += HC_OnPlayerDataChanged;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SM_OnLoadEventCompleted;
            Seed.Value = UnityEngine.Random.Range(0, 1000000);
            CurrentPlanet.Value = InitialPlanet();
        }
        
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            HCNetworkManager.Instance.OnPlayerDataChanged -= HC_OnPlayerDataChanged;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SM_OnLoadEventCompleted;
        }
    }

    void HC_OnPlayerDataChanged()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(State.Value == GameState.GAME_ACTIVE && HCNetworkManager.Instance.AllPlayersReady())
            {
                HCNetworkManager.Instance.ResetReadyStatusAll();
                State.Value = GameState.GAME_DEPARTING_PLANET;
            }
            if(State.Value == GameState.GAME_DEPARTING_PLANET && HCNetworkManager.Instance.AllPlayersReady())
            {
                HCNetworkManager.Instance.ResetReadyStatusAll();
                Loader.LoadNetwork(Loader.Scene.StarshipScene);
            }
            if(State.Value == GameState.STARSHIP && HCNetworkManager.Instance.AllPlayersReady())
            {
                if(PlanetChoice.Value >= 0 && PlanetChoice.Value < 3)
                {
                    CurrentPlanet.Value = PlanetChoices[PlanetChoice.Value];
                    HCNetworkManager.Instance.ResetReadyStatusAll();
                    Loader.LoadNetwork(Loader.Scene.GameScene);
                }
                else
                {
                    Debug.LogError("Planet Choice is not within Bounds");
                }
            }
        }
    }

    void SM_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(sceneName == Loader.Scene.GamePreloadingScene.ToString())
            {
                State.Value = GameState.PRELOADING;
            }
            else if(sceneName == Loader.Scene.GameScene.ToString())
            {
                State.Value = GameState.GAME_LANDING_PLANET;
                StageTime.Value = 0f;
                _internalChrono = 0f;
                Layer.Value = CurrentPlanet.Value.Layer;
            }
            else if(sceneName == Loader.Scene.StarshipScene.ToString())
            {
                State.Value = GameState.STARSHIP;
                UpdateStarshipValues();
            }    
        }
    }

    private void OnStateValueChanged(GameState previousValue, GameState newValue)
    {
        Debug.Log($"Game State Changed: {State.Value}");
        if(newValue == GameState.GAME_ACTIVE)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if(previousValue == GameState.GAME_ACTIVE && (newValue == GameState.GAME_FAILED || newValue == GameState.GAME_DEPARTING_PLANET))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnSeedValueChanged(int previousValue, int newValue)
    {
        UnityEngine.Random.InitState(newValue);
    }

    public void AddPlayer(ulong playerId, HCPlayer player)
    {
        Players[playerId] = player;
    }

    public void BaseDestroyed()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            if(State.Value == GameState.GAME_ACTIVE)
            {
                State.Value = GameState.GAME_FAILED;
            }
        }
    }

    void UpdateStarshipValues()
    {
        UpdatePlayerResources();
        PlanetChoice.Value = -1;
        UpdatePlanetChoices();
    }

    void UpdatePlayerResources()
    {
        for(int i = 0; i < HCBase.Instance.MaterialsLooted.Count; i++)
        {
            foreach(HCPlayer player in Players.Values)
            {
                if(player != null)
                {
                    player.Materials[i] = player.Materials[i] + HCBase.Instance.MaterialsLooted[i];
                }
            }
        }
        
        HCBase.Instance.ClearMaterialsList();
    }

    PlanetDataNetwork InitialPlanet()
    {
        int layer = 1;
        name = GetRandomPlanetName();
        int colorH = UnityEngine.Random.Range(0, 360);
        List<bool> layout = GetRandomPlanetLayout();
        List<bool> mats = new List<bool> {
            true,
            true,
        };

        return new PlanetDataNetwork {
            Layer = layer,
            Name = HCUtils.TruncateBytes(name, 32),
            ColorH = colorH,
            Materials = mats.ToFixedList32Bytes(),
            Layout = layout.ToFixedList32Bytes(),
        };
    }

    void UpdatePlanetChoices()
    {
        for(int i = 0; i < 3; i++)
        {
            int layer = Layer.Value + 1;
            string name = GetRandomPlanetName();
            int colorH = UnityEngine.Random.Range(0, 360);
            List<bool> layout = GetRandomPlanetLayout();
            List<bool> mats = GetPlanetMaterialList();

            PlanetDataNetwork planet = new PlanetDataNetwork {
                Layer = layer,
                Name = HCUtils.TruncateBytes(name, 32),
                ColorH = colorH,
                Materials = mats.ToFixedList32Bytes(),
                Layout = layout.ToFixedList32Bytes(),
            };

            if(i < PlanetChoices.Count)
            {
                PlanetChoices[i] = planet;
            }
            else
            {
                PlanetChoices.Add(planet);
            }
        }
    }

    string GetRandomPlanetName()
    {
        string name = string.Empty;
        for(int i = 0; i < 3; i++)
        {
            name += (char)UnityEngine.Random.Range('A','Z' + 1);
        }
        name += "-";
        for (int i = 0; i < 4; i++)
        {
            name += UnityEngine.Random.Range(0, 10).ToString();
        }
        return name;
    }

    List<bool> GetRandomPlanetLayout()
    {
        List<bool> layout = new List<bool>();
        for(int i = 0; i < 6; i++)
        {
            int rand = UnityEngine.Random.Range(0,2);
            layout.Add(rand == 0);
        }
        return layout;
    }

    List<bool> GetPlanetMaterialList()
    {
        List<bool> matsAvailable = new List<bool>();
        int layer = Layer.Value;
        int numMaterials = GameData.Instance.Materials.Count;
        for(int i = 0; i < numMaterials; i++)
        {
            if(i <= layer + 1)
            {
                matsAvailable.Add(true);
            }
            else
            {
                matsAvailable.Add(false);
            }
        }

        return matsAvailable;
    }
}
