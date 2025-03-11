using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HCLobby : MonoBehaviour
{
    public enum LobbyStatus {
        NONE,
        CREATING_LOBBY,
        CREATING_RELAY,
        LOBBY_CREATED,
        FAILED_TO_CREATE,
        JOINING_LOBBY,
        JOINING_RELAY,
        LOBBY_JOINED,
        FAILED_TO_JOIN
    }

    const float HEARTBEAT_TIME = 5f;
    const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public event Action<LobbyStatus> OnLobbyStatusChanged;

    public static HCLobby Instance { get; private set;}

    Lobby _lobby;
    LobbyStatus _status = LobbyStatus.NONE;

    float _chrono = 0f;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthenticator();
    }

    private void Update()
    {
        HandleHeartbeat();
    }

    async void InitializeUnityAuthenticator()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0, 1000000).ToString());

            await UnityServices.InitializeAsync(options);
        }
        if(!AuthenticationService.Instance.IsAuthorized)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
   }

   async Task<Allocation> AllocateRelay()
   {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(HCNetworkManager.MAX_PLAYERS - 1);
            return allocation;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
            return default;
        }
   }

    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            return default;
        }
    }

    async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e);
            return default;
        }
    }

   public async void CreateLobby()
   {
        ChangeStatus(LobbyStatus.CREATING_LOBBY);
        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync(name, HCNetworkManager.MAX_PLAYERS, new CreateLobbyOptions {
                IsPrivate = true,
            });

            ChangeStatus(LobbyStatus.CREATING_RELAY);
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            ChangeStatus(LobbyStatus.LOBBY_CREATED);

            HCNetworkManager.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharSelectScene);

        } catch(LobbyServiceException e) {
            ChangeStatus(LobbyStatus.FAILED_TO_CREATE);
            Debug.LogError(e);
        }
   }

   public async void JoinWithCode(string code)
   {
        ChangeStatus(LobbyStatus.JOINING_LOBBY);
        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

            ChangeStatus(LobbyStatus.JOINING_RELAY);
            string relayJoinCode = _lobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            ChangeStatus(LobbyStatus.LOBBY_JOINED);
            HCNetworkManager.Instance.StartClient();
        } 
        catch(LobbyServiceException e) {
            ChangeStatus(LobbyStatus.FAILED_TO_JOIN);
            Debug.LogError(e);
        }
   }

   public async void DeleteLobby()
   {
        if(_lobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
                _lobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
   }

   public async void LeaveLobby()
   {
        if(_lobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);
                _lobby = null;
            }
            catch(LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
   }

   public async void KickPlayer(string playerId)
   {
        if(IsLobbyHost())
        {
            try {
                await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.LogError(e);
            }
        }
   }

   void HandleHeartbeat()
   {
        if(IsLobbyHost())
        {
            _chrono += Time.deltaTime;
            if(_chrono > HEARTBEAT_TIME)
            {
                _chrono -= HEARTBEAT_TIME;
                LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);
            }
        }
   }

   public bool IsSignedIn()
   {
        return AuthenticationService.Instance.IsAuthorized;
   }

   public Lobby GetLobby()
   {
        return _lobby;
   }

   bool IsLobbyHost()
   {
        return _lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId;
   }

   void ChangeStatus(LobbyStatus status)
   {
        _status = status;

        OnLobbyStatusChanged?.Invoke(status);
   }
}
