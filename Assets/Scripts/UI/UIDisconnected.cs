using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIDisconnected : MonoBehaviour
{
    [SerializeField] TMP_Text Reason;
    [SerializeField] Button MenuButton;

    void Awake()
    {
        MenuButton.onClick.AddListener(() =>{
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainScene);
        });
    }

    void Start()
    {
        HCNetworkManager.Instance.OnConnectionStatusChanged += HC_OnConnectionStatusChanged;
        Hide();
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnConnectionStatusChanged -= HC_OnConnectionStatusChanged;
    }

    void HC_OnConnectionStatusChanged(HCNetworkManager.ConnectionStatus status)
    {
        if(status == HCNetworkManager.ConnectionStatus.CONNECTION_FAILED)
        {
            Show();
            if(NetworkManager.Singleton.DisconnectReason != string.Empty)
            {
                Reason.text = $"Reason: {NetworkManager.Singleton.DisconnectReason}";
            }
            else
            {
                Reason.text = string.Empty;
            }
        }
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
