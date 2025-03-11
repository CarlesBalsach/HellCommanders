using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIGamePreloader : MonoBehaviour
{

    [SerializeField] TMP_Text StatusText;
    [SerializeField] Button MainMenuButton;
    
    void Start()
    {
        StatusText.text = "Loading Game...";

        MainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainScene);
        });

        MainMenuButton.gameObject.SetActive(false);

        HCNetworkManager.Instance.OnConnectionStatusChanged += HC_OnConnectionStatusChanged;
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnConnectionStatusChanged -= HC_OnConnectionStatusChanged;
    }

    void HC_OnConnectionStatusChanged(HCNetworkManager.ConnectionStatus status)
    {
        if(status == HCNetworkManager.ConnectionStatus.CONNECTION_FAILED)
        {
            StatusText.text = "Connection Failed";
            MainMenuButton.gameObject.SetActive(true);
        }
    }
}
