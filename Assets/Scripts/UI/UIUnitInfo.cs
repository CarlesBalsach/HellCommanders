using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitInfo : MonoBehaviour
{
    [SerializeField] Image UnitFrame;
    [SerializeField] Image UnitImage;
    [SerializeField] TMP_Text UnitNameText;
    [SerializeField] TMP_Text UnitStatsText;
    [SerializeField] TMP_Text UnitDescriptionText;

    HCUnit _focusedUnit = null;

    void Start()
    {
        UnitScanner.Instance.OnFocusUnit += OnUnitFocused;
        UnitScanner.Instance.OnDefocusUnit += OnUnitDefocused;
        DisplayViews(false);
    }

    void OnDestroy()
    {
        if(UnitScanner.Instance != null)
        {
            UnitScanner.Instance.OnFocusUnit -= OnUnitFocused;
            UnitScanner.Instance.OnDefocusUnit -= OnUnitDefocused;
        }
    }

    void Update()
    {
        if(_focusedUnit != null)
        {
            UnitStatsText.text = GetUnitStatsText(_focusedUnit);
        }
    }

    private void OnUnitFocused(HCUnit unit, Sprite sprite)
    {
        _focusedUnit = unit;
        UnitImage.sprite = sprite;
        UnitNameText.text = unit.Stats.Name;
        UnitStatsText.text = GetUnitStatsText(unit);
        UnitDescriptionText.text = unit.Stats.Description;
        DisplayViews(true);
    }

    private void OnUnitDefocused()
    {
        _focusedUnit = null;
        DisplayViews(false);
    }

    void DisplayViews(bool state)
    {
        UnitFrame.gameObject.SetActive(state);
        UnitImage.gameObject.SetActive(state);
        UnitNameText.gameObject.SetActive(state);
        UnitStatsText.gameObject.SetActive(state);
        UnitDescriptionText.gameObject.SetActive(state);
    }

    string GetUnitStatsText(HCUnit unit)
    {
        string text = "";
        text += $"{HCEmoji.HEART}{unit.HP.Value}/{unit.Stats.HP}\n";
        text += $"{HCEmoji.SWORD}{unit.Stats.Damage} - {HCEmoji.SHIELD}{unit.Stats.Armor}";
        if(unit.Stats.Rounds > 0)
        {
            text += $" - {HCEmoji.BULLETS}{unit.Stats.Rounds}";
        }
        return text;
    }
}
