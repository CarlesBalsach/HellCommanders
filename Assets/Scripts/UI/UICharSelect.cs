using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;


public class UICharSelect : MonoBehaviour
{
    [SerializeField] UICharacterButton CharacterSelectionButtonPrefab;
    [SerializeField] List<UIPlayerCharacterSelection> PlayerCharacters;
    [SerializeField] TMP_Text LobbyText;
    [SerializeField] TMP_InputField PlayerNameIF;
    [SerializeField] RectTransform CharactersAvailableRT;
    [SerializeField] UICharacterInfo CharInfo;
    [SerializeField] Button ReadyButton;
    [SerializeField] Button BackButton;
    [SerializeField] UIDisconnected UIDisconnected;

    void Awake()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged += HC_OnPlayerDataChanged;

        ReadyButton.onClick.AddListener(() => {
            HCNetworkManager.Instance.ChangePlayerReadyStatus(true);
            ReadyButton.GetComponentInChildren<TMP_Text>().text = "Ready";
            ReadyButton.interactable = false;

            if(HCNetworkManager.IsSinglePlayer)
            {
                Loader.LoadNetwork(Loader.Scene.GamePreloadingScene);
            }
        });

        BackButton.onClick.AddListener(() => {
            if(HCLobby.Instance != null)
            {
                HCLobby.Instance.LeaveLobby();
            }
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainScene);
        });

        PlayerNameIF.onEndEdit.AddListener((string value) => {
            HCPrefs.SetPlayerName(value);
            HCNetworkManager.Instance.ChangePlayerName(value);
        });
    }

    void Start()
    {
        UIDisconnected.gameObject.SetActive(true);
        if(HCNetworkManager.IsSinglePlayer)
        {
            LobbyText.text = $"Single Player\nMode";
        }
        else
        {
            LobbyText.text = $"Lobby:\n{HCLobby.Instance.GetLobby().LobbyCode}";
        }
        
        PlayerNameIF.text = HCPrefs.GetPlayerName();
        CharInfo.Init(GameData.Instance.GetCharacterData(0));
        ReadyButton.interactable = HCNetworkManager.IsSinglePlayer;

        foreach(CharacterData character in GameData.Instance.Characters)
        {
            UICharacterButton charButton = Instantiate(CharacterSelectionButtonPrefab, CharactersAvailableRT);
            charButton.Init(this, character);
        }
    }

    private void OnDestroy()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged -= HC_OnPlayerDataChanged;
    }

    void HC_OnPlayerDataChanged()
    {
        if(!HCNetworkManager.IsSinglePlayer && HCNetworkManager.Instance.PlayersConnected() > 0)
        {
            int players = HCNetworkManager.Instance.PlayersConnected();
            bool hostReady = HCNetworkManager.Instance.GetPlayerData().Ready;
            ReadyButton.interactable = players >= 2 && !hostReady;

            if(NetworkManager.Singleton.IsHost && HCNetworkManager.Instance.AllPlayersReady())
            {
                HCNetworkManager.Instance.ResetReadyStatusAll();
                Loader.LoadNetwork(Loader.Scene.GamePreloadingScene);
            }
        }
    }

    public void CharacterSelected(CharacterData character)
    {
        HCNetworkManager.Instance.ChangePlayerCharacterId(character.Id);
        CharInfo.Init(character);
    }
}
