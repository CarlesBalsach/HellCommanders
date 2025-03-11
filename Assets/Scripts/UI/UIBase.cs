using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;

public class UIBase : MonoBehaviour
{
    [SerializeField] TMP_Text HPText;
    [SerializeField] UIBar HPBar;
    [SerializeField] TMP_Text TimeText;
    [SerializeField] TMP_Text ResourcesText;

    void Start()
    {
        HCBase.Instance.HP.OnValueChanged += HCBase_OnHPChanged;
        HCBase.Instance.MaterialsLooted.OnListChanged += HCBase_OnMaterialsChanged;
        HCGameManager.Instance.StageTime.OnValueChanged += HCGame_OnStageTimeChanged;

        HPText.text = $"HP: {HCBase.Instance.HP.Value}/{HCBase.Instance.MaxHP.Value}";
        TimeText.text = "00:00";
        ResourcesText.text = string.Empty;
    }

    private void OnDestroy()
    {
        HCBase.Instance.HP.OnValueChanged -= HCBase_OnHPChanged;
        HCGameManager.Instance.StageTime.OnValueChanged -= HCGame_OnStageTimeChanged;
        HCBase.Instance.MaterialsLooted.OnListChanged -= HCBase_OnMaterialsChanged;
    }

    private void HCBase_OnHPChanged(int previousValue, int newValue)
    {
        HPText.text = $"HP: {newValue}/{HCBase.Instance.MaxHP.Value}";
        HPBar.UpdateBar((float)newValue/HCBase.Instance.MaxHP.Value);
    }

    private void HCGame_OnStageTimeChanged(float previousValue, float newValue)
    {
        int min = (int)(newValue / 60);
        int sec = (int)(newValue) % 60;
        TimeText.text = $"{min:00}:{sec:00}";
    }

    private void HCBase_OnMaterialsChanged(NetworkListEvent<int> changeEvent)
    {
        string materialsText = "";
        for (int i = 0; i < HCBase.Instance.MaterialsLooted.Count; i++)
        {
            int count = HCBase.Instance.MaterialsLooted[i];
            if(count > 0)
            {
                MaterialData material = GameData.Instance.Materials[i];
                materialsText += $"{HCEmoji.Get(material.Image)}x{count} ";
            }
        }

        ResourcesText.text = materialsText.Trim();
    }
}
