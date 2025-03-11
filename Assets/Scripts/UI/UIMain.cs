using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{

    [SerializeField] private Button SinglePlayerButton;
    [SerializeField] private Button MultiplayerButton;
    [SerializeField] private Button QuitButton;

    void Start()
    {
        ClearInstances();

        SinglePlayerButton.onClick.AddListener(() => {
            HCNetworkManager.IsSinglePlayer = true;
            Loader.Load(Loader.Scene.SinglePlayerLoadScene);
        });

        MultiplayerButton.onClick.AddListener(() => {
            HCNetworkManager.IsSinglePlayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        
        QuitButton.onClick.AddListener(() => Application.Quit());
    }

    void ClearInstances()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if(HCNetworkManager.Instance != null)
        {
            Destroy(HCNetworkManager.Instance.gameObject);
        }
        if(HCLobby.Instance != null)
        {
            Destroy(HCLobby.Instance.gameObject);
        }
        if(HCGameManager.Instance != null)
        {
            Destroy(HCGameManager.Instance.gameObject);
        }
        if(HCBase.Instance != null)
        {
            Destroy(HCBase.Instance.gameObject);
        }
    }
}
