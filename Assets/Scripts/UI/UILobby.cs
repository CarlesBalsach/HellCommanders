using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
    [SerializeField] Button HostButton;
    [SerializeField] TMP_InputField LobbyCodeIF;
    [SerializeField] Button JoinCodeButton;
    [SerializeField] UILobbyStatus UILobbyStatus;

    bool _signedIn = false;

    void Awake()
    {
        HostButton.onClick.AddListener(() => {
            HCLobby.Instance.CreateLobby();
        });

        JoinCodeButton.onClick.AddListener(() => {
            HCLobby.Instance.JoinWithCode(LobbyCodeIF.text);
        });

        LobbyCodeIF.onValueChanged.AddListener((string text) => {
            JoinCodeButton.interactable = text.Length > 0;    
        });

        LobbyCodeIF.onSubmit.AddListener((string input)=> {
            HCLobby.Instance.JoinWithCode(input);
        });

        JoinCodeButton.interactable = false;
    }

    void Start()
    {
        UILobbyStatus.gameObject.SetActive(true);
    }

    private void Update()
    {
        _signedIn = HCLobby.Instance.IsSignedIn();
        HostButton.interactable = _signedIn;
        LobbyCodeIF.interactable = _signedIn;
        JoinCodeButton.interactable = _signedIn;
    }
}
