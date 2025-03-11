using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGameAlly : MonoBehaviour
{
    [SerializeField] Image Frame;
    [SerializeField] Image CharacterFrame;
    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text PlayerName;
    [SerializeField] TMP_Text ReadyText;
    [SerializeField] Image ConnectionStatusImage;

    public void UpdateData(PlayerDataNetwork playerData)
    {
        CharacterData character = GameData.Instance.GetCharacterData(playerData.CharacterId);
        CharacterImage.sprite = PrefabController.Instance.GetCharacterSprite(character);
        PlayerName.text = playerData.Name.ToString();

        ReadyText.text = string.Empty;
        CharacterFrame.color = HCColor.GetPlayerColorDark(HCNetworkManager.Instance.GetPlayerIndexFromClientId(playerData.ClientId));
        Frame.color = HCColor.GetPlayerColorDark(HCNetworkManager.Instance.GetPlayerIndexFromClientId(playerData.ClientId));

        if(playerData.Ready)
        {
            ReadyText.text = "Ready to Leave!";
            CharacterFrame.color = HCColor.GetPlayerColor(HCNetworkManager.Instance.GetPlayerIndexFromClientId(playerData.ClientId));
            Frame.color = HCColor.GetPlayerColor(HCNetworkManager.Instance.GetPlayerIndexFromClientId(playerData.ClientId));
        }
        
        if(playerData.Ping <= 100)
        {
            ConnectionStatusImage.sprite = PrefabController.Instance.GoodConnectionSprite;
        }
        else if(playerData.Ping <= 200)
        {
            ConnectionStatusImage.sprite = PrefabController.Instance.OkConnectionSprite;
        }
        else
        {
            ConnectionStatusImage.sprite = PrefabController.Instance.BadConnectionSprite;
        }
    }
}
