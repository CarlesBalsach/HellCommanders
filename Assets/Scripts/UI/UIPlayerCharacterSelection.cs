using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerCharacterSelection : MonoBehaviour
{
    
    [SerializeField] int Index;
    [SerializeField] Image CharImage;
    [SerializeField] TMP_Text Name;
    [SerializeField] Button KickButton;
    [SerializeField] TMP_Text ReadyText;

    void Awake()
    {
        KickButton.onClick.AddListener(() => {
            PlayerDataNetwork playerData = HCNetworkManager.Instance.GetPlayerDataFromPlayerIndex(Index);
            HCLobby.Instance.KickPlayer(playerData.Id.ToString());
            HCNetworkManager.Instance.KickPlayer(playerData.ClientId);
        });
    }

    void Start()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged += HC_OnPlayerDataChanged;

        KickButton.gameObject.SetActive(NetworkManager.Singleton.IsHost && Index > 0);

        UpdatePlayer();
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged -= HC_OnPlayerDataChanged;
    }

    void HC_OnPlayerDataChanged()
    {
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        if(HCNetworkManager.Instance.IsPlayerIndexConnected(Index))
        {
            Show();

            PlayerDataNetwork playerData = HCNetworkManager.Instance.GetPlayerDataFromPlayerIndex(Index);
            ReadyText.gameObject.SetActive(playerData.Ready);
            Name.text = playerData.Name.ToString();
            CharacterData character = GameData.Instance.GetCharacterData(playerData.CharacterId);
            CharImage.sprite = PrefabController.Instance.GetCharacterSprite(character);
        }
        else
        {
            Hide();
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
