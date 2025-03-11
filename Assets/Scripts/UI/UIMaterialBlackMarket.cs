using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIMaterialBlackMarket : MonoBehaviour
{
    [SerializeField] TMP_Text Name;
    [SerializeField] TMP_Text Sprite;
    [SerializeField] Button BuyButton;
    [SerializeField] Button SellButton;

    MaterialData _material;

    public void Init(MaterialData material)
    {
        _material = material;

        Name.text = material.Name;
        Sprite.text = HCEmoji.Get(material.Image);
        
        BuyButton.GetComponentInChildren<TMP_Text>().text = $"Buy - {material.Price}{HCEmoji.CREDITS}";
        SellButton.GetComponentInChildren<TMP_Text>().text = $"Sell - {material.Price / 2}{HCEmoji.CREDITS}";

        BuyButton.onClick.AddListener(()=>{
            if(_material != null)
            {
                HCPlayer.LocalInstance.BuyMaterialServerRpc(_material.Id);
            }
        });

        SellButton.onClick.AddListener(()=>{
            if(_material != null)
            {
                HCPlayer.LocalInstance.SellMaterialServerRpc(_material.Id);
            }
        });

        UpdateButtonsAvailability();
    }

    void OnEnable()
    {
        HCPlayer.LocalInstance.Credits.OnValueChanged += OnPlayerCreditsChanged;
        HCPlayer.LocalInstance.Materials.OnListChanged += OnPlayerMaterialsChanged;
    }

    void OnDisable()
    {
        HCPlayer.LocalInstance.Credits.OnValueChanged -= OnPlayerCreditsChanged;
        HCPlayer.LocalInstance.Materials.OnListChanged -= OnPlayerMaterialsChanged;
    }

    private void OnPlayerCreditsChanged(int previousValue, int newValue)
    {
        UpdateButtonsAvailability();
    }

    private void OnPlayerMaterialsChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateButtonsAvailability();
    }

    void UpdateButtonsAvailability()
    {
        BuyButton.interactable = HCPlayer.LocalInstance.Credits.Value >= _material.Price;
        SellButton.interactable = HCPlayer.LocalInstance.Materials[_material.Id] > 0;
    }
}
