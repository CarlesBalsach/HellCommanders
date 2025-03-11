using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyStatus : MonoBehaviour
{
    [SerializeField] TMP_Text LobbyStatus;
    [SerializeField] TMP_Text ConnectionStatus;
    [SerializeField] TMP_Text FailureReason;
    [SerializeField] Button MainMenuButton;

    void Start()
    {
        MainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainScene);
        });

        ConnectionStatus.text = string.Empty;
        FailureReason.text = string.Empty;

        MainMenuButton.gameObject.SetActive(false);
        HCLobby.Instance.OnLobbyStatusChanged += OnLobbyStatusChanged;
        HCNetworkManager.Instance.OnConnectionStatusChanged += OnConnectionStatusChanged;

        Hide();
    }

    private void OnDestroy()
    {
        HCLobby.Instance.OnLobbyStatusChanged -= OnLobbyStatusChanged;
        HCNetworkManager.Instance.OnConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnLobbyStatusChanged(HCLobby.LobbyStatus status)
    {
        Show();
        LobbyStatus.text = status.ToString();

        if(status == HCLobby.LobbyStatus.FAILED_TO_CREATE || 
            status == HCLobby.LobbyStatus.FAILED_TO_JOIN)
        {
            MainMenuButton.gameObject.SetActive(true);
        }
    }

    void OnConnectionStatusChanged(HCNetworkManager.ConnectionStatus status)
    {
        Show();
        ConnectionStatus.text = $"Connection Status: {status}";

        if(status == HCNetworkManager.ConnectionStatus.CONNECTION_FAILED)
        {
            if(NetworkManager.Singleton.DisconnectReason == string.Empty)
            {
                FailureReason.text = $"Error: {NetworkManager.Singleton.DisconnectReason}";
            }

            MainMenuButton.gameObject.SetActive(true);
        }
    }
}
