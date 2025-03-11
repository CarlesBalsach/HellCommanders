using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Services.Core;

public class UISinglePlayerLoad : MonoBehaviour
{

    [SerializeField] TMP_Text ConnectionStatus;
    [SerializeField] Button MainMenuButton;

    void Awake()
    {
        MainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainScene);
        });

        MainMenuButton.gameObject.SetActive(false);
    }

    void Start()
    {
        HCNetworkManager.Instance.OnConnectionStatusChanged += HC_OnConnectionStatusChanged;

        ConnectionStatus.text = "Initializing...";

        InitializeAndConnect();
    }

    async void InitializeAndConnect()
    {
        await UnityServices.InitializeAsync();
        HCNetworkManager.Instance.StartHost();
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnConnectionStatusChanged -= HC_OnConnectionStatusChanged;
    }

    void HC_OnConnectionStatusChanged(HCNetworkManager.ConnectionStatus status)
    {
        ConnectionStatus.text = status.ToString();

        if(status == HCNetworkManager.ConnectionStatus.CONNECTION_FAILED)
        {
            MainMenuButton.gameObject.SetActive(true);
        }

        if(status == HCNetworkManager.ConnectionStatus.HOST_CONNECTED)
        {
            Loader.LoadNetwork(Loader.Scene.CharSelectScene);
        }
    }
}
