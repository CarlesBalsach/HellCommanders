using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIStarshipPlayer : MonoBehaviour
{
    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text PlayerName;
    [SerializeField] TMP_Text Resources;

    void Start()
    {
        CharacterData character = GameData.Instance.GetCharacterData(HCNetworkManager.Instance.GetPlayerData().CharacterId);
        CharacterImage.sprite = PrefabController.Instance.GetCharacterSprite(character);

        PlayerName.text = HCNetworkManager.Instance.GetPlayerData().Name.ToString();

        UpdateResources();

        HCPlayer.LocalInstance.Materials.OnListChanged += OnPlayerMaterialsChanged;
        HCPlayer.LocalInstance.Credits.OnValueChanged += OnPlayerCreditsChanged;
    }

    void OnDestroy()
    {
        HCPlayer.LocalInstance.Materials.OnListChanged -= OnPlayerMaterialsChanged;
        HCPlayer.LocalInstance.Credits.OnValueChanged -= OnPlayerCreditsChanged;
    }

    private void OnPlayerMaterialsChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateResources();
    }
    
    private void OnPlayerCreditsChanged(int previousValue, int newValue)
    {
        UpdateResources();
    }

    void UpdateResources()
    {
        string resources = $"{HCEmoji.CREDITS}{HCPlayer.LocalInstance.Credits.Value}  ";
        for(int i = 0; i < HCPlayer.LocalInstance.Materials.Count; i++)
        {
            int count = HCPlayer.LocalInstance.Materials[i];
            if(count > 0)
            {
                MaterialData material = GameData.Instance.GetMaterial(i);
                resources += $"{HCEmoji.Get(material.Image)}{count} ";
            }
        }

        Resources.text = resources.Trim();
    }
}
