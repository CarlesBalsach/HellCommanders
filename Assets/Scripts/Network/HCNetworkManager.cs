using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using System;
using UnityEngine.SceneManagement;
using System.ComponentModel;

public class HCNetworkManager : NetworkBehaviour
{
    const float PING_REQUEST_INTERVAL = 30;

    public enum ConnectionStatus {
        NONE,
        STARTING_HOST,
        HOST_CONNECTED,
        STARTING_CLIENT,
        CLIENT_CONNECTED,
        CONNECTION_FAILED
    }

    public const int MAX_PLAYERS = 4;
    
    public static HCNetworkManager Instance { get; private set; }
    public static bool IsSinglePlayer = true;

    public event Action<ConnectionStatus> OnConnectionStatusChanged;
    public event Action OnPlayerDataChanged;

    NetworkList<PlayerDataNetwork> _playerData;
    ConnectionStatus _status = ConnectionStatus.NONE;

    float _pingRequestTime = 0f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerData = new NetworkList<PlayerDataNetwork>();
        _playerData.OnListChanged += OnPlayerDataListChanged;
    }

    void Update()
    {
        if(_status == ConnectionStatus.HOST_CONNECTED || _status == ConnectionStatus.CLIENT_CONNECTED)
        {
            if(Time.time > _pingRequestTime + PING_REQUEST_INTERVAL)
            {
                _pingRequestTime = Time.time;
                PingRequestServerRpc();
            }
        }
    }

    /**********
    CONNECTION ACTIONS
    ***********/

    public void StartHost()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Host_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Host_OnClientDisconnectCallback;

        if(!IsSinglePlayer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += Host_ConnectionApprovalCallback;
        }

        ChangeConnectionStatus(ConnectionStatus.STARTING_HOST);
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        ChangeConnectionStatus(ConnectionStatus.STARTING_CLIENT);

        NetworkManager.Singleton.OnClientConnectedCallback += Client_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        Host_OnClientDisconnectCallback(clientId);
    }

    /**********
    CALLBACKS HOST
    ***********/

    void OnPlayerDataListChanged(NetworkListEvent<PlayerDataNetwork> changedEvent)
    {
        Debug.Log($"Player Data Changed:\n{changedEvent.PreviousValue}\n{changedEvent.Value}");
        OnPlayerDataChanged?.Invoke();
    }

    void Host_OnClientConnectedCallback(ulong clientId)
    {
        if(clientId == GetLocalPlayedId())
        {
            ChangeConnectionStatus(ConnectionStatus.HOST_CONNECTED);
            Debug.Log($"Connected as: HOST {clientId}");
        }
        else
        {
            Debug.Log($"New Client Connected: {clientId}");
        }

        _playerData.Add(new PlayerDataNetwork {
            ClientId = clientId,
            CharacterId = 0,
        });

        ChangePlayerNameServerRpc(HCPrefs.GetPlayerName());

        if(!IsSinglePlayer)
        {
            SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
        }
    }

    void Host_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < _playerData.Count; i++)
        {
            PlayerDataNetwork playerData = _playerData[i];
            if(playerData.ClientId == clientId)
            {
                _playerData.RemoveAt(i);
                return;
            }
        }
    }

    void Host_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse respone)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharSelectScene.ToString())
        {
            Debug.Log("Client Rejected: Game has already Started");
            respone.Approved = false;
            respone.Reason = "Game has already started";
            return;
        }
        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYERS)
        {
            Debug.Log("Client Rejected: Game is Full");
            respone.Approved = false;
            respone.Reason = "Lobby is full";
            return;
        }

        Debug.Log("Client Accepted");
        respone.Approved = true;
    }

    /**********
    CALLBACKS CLIENT
    ***********/

    void Client_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Connected as: CLIENT {clientId}");
        ChangeConnectionStatus(ConnectionStatus.CLIENT_CONNECTED);
        
        ChangePlayerNameServerRpc(HCPrefs.GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    void Client_OnClientDisconnectCallback(ulong clientId)
    {
        ChangeConnectionStatus(ConnectionStatus.CONNECTION_FAILED);
    }

    /**********
    SERVER RPC
    ***********/

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerDataNetwork playerData = _playerData[playerIndex];
        playerData.Id = playerId;
        _playerData[playerIndex] = playerData;
    }

    [ServerRpc (RequireOwnership = false)]
    void ChangePlayerCharacterServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        
        PlayerDataNetwork playerData = _playerData[playerIndex];
        playerData.CharacterId = characterId;
        _playerData[playerIndex] = playerData;
    }

    [ServerRpc (RequireOwnership = false)]
    void ChangePlayerNameServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerDataNetwork playerData = _playerData[playerIndex];
        playerData.Name = HCUtils.TruncateBytes(name, 64);
        _playerData[playerIndex] = playerData;
    }

    [ServerRpc (RequireOwnership = false)]
    void ChangePlayerReadyStatusServerRpc(bool ready, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerDataNetwork playerData = _playerData[playerIndex];
        playerData.Ready = ready;
        _playerData[playerIndex] = playerData;
    }

    [ServerRpc (RequireOwnership = false)]
    void ChangePlayerPingServerRpc(int ping, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerDataNetwork playerData = _playerData[playerIndex];
        playerData.Ping = ping;
        _playerData[playerIndex] = playerData;
    }

    [ServerRpc (RequireOwnership = false)]
    void PingRequestServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{serverRpcParams.Receive.SenderClientId}
            }
        };

        PingResponseClientRpc(clientRpcParams);
    }

    /**********
    CLIENT RPC
    ***********/

    [ClientRpc]
    void PingResponseClientRpc(ClientRpcParams clientRpcParams = default)
    {
        int ping = (int)((Time.time - _pingRequestTime) * 1000);
        ChangePlayerPingServerRpc(ping);
    }

    /**********
    SERVER ONLY
    ***********/

    public void ResetReadyStatusAll()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            for(int i = 0; i < _playerData.Count; i++)
            {
                PlayerDataNetwork pdata = _playerData[i];
                pdata.Ready = false;
                _playerData[i] = pdata;
            }
        }
        else
        {
            Debug.LogError("Trying to unready all players when not the server");
        }
    }

    /**********
    GAME ACTIONS
    ***********/

    public void ChangePlayerCharacterId(int characterId)
    {
        ChangePlayerCharacterServerRpc(characterId);
    }

    public void ChangePlayerName(string name)
    {
        ChangePlayerNameServerRpc(name);
    }

    public void ChangePlayerReadyStatus(bool ready)
    {
        ChangePlayerReadyStatusServerRpc(ready);
    }

    /**********
       UTILS
    ***********/

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerData.Count;
    }

    public int GetPlayerIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerData.Count; i++)
        {
            if(_playerData[i].ClientId == clientId)
            {
                return i;
            }
        }
        Debug.LogError($"Player Index not found for Client Id: {clientId}");
        return -1;
    }

    public PlayerDataNetwork GetPlayerDataFromClientId(ulong clientId)
    {
        foreach(PlayerDataNetwork playerData in _playerData)
        {
            if(playerData.ClientId == clientId)
            {
                return playerData;
            }
        }
        Debug.LogError($"Player Data not found for Client Id: {clientId}");
        return default;
    }

    public ulong GetLocalPlayedId()
    {
        return NetworkManager.Singleton.LocalClientId;
    }

    public PlayerDataNetwork GetPlayerData()
    {
        return GetPlayerDataFromClientId(GetLocalPlayedId());
    }

    public List<PlayerDataNetwork> GetPlayersData()
    {
        List<PlayerDataNetwork> playersData = new List<PlayerDataNetwork>();
        foreach(PlayerDataNetwork pd in _playerData)
        {
            playersData.Add(pd);
        }
        return playersData;
    }

    public PlayerDataNetwork GetPlayerDataFromPlayerIndex(int index)
    {
        return _playerData[index];
    }

    public int PlayersConnected()
    {
        return _playerData.Count;
    }

    public bool AllPlayersReady()
    {
        foreach(PlayerDataNetwork playerData in _playerData)
        {
            if(playerData.Ready == false)
            {
                return false;
            }
        }
        return true;
    }

    void ChangeConnectionStatus(ConnectionStatus status)
    {
        _status = status;
        OnConnectionStatusChanged?.Invoke(status);
    }
}
