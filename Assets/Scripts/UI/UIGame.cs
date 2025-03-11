using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGame : MonoBehaviour
{

    [SerializeField] Transform Player;
    [SerializeField] Transform Base;
    [SerializeField] Transform Info;
    [SerializeField] Transform Allies;
    [SerializeField] Transform MissionReport;

    void Start()
    {
        HCGameManager.Instance.State.OnValueChanged += OnGameStateChanged;
        HidePanels();
        MissionReport.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        HCGameManager.Instance.State.OnValueChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(HCGameManager.GameState previousValue, HCGameManager.GameState newValue)
    {
        if(newValue == HCGameManager.GameState.GAME_ACTIVE)
        {
            ShowPanels();
        }
        else
        {
            HidePanels();
        }
    }

    void ShowPanels()
    {
        SetUIElementsAlpha(Player, 1f);
        SetUIElementsAlpha(Base, 1f);
        SetUIElementsAlpha(Info, 1f);
        SetUIElementsAlpha(Allies, 1f);
    }

    void HidePanels()
    {
        SetUIElementsAlpha(Player, 0f);
        SetUIElementsAlpha(Base, 0f);
        SetUIElementsAlpha(Info, 0f);
        SetUIElementsAlpha(Allies, 0f);
    }

    void SetUIElementsAlpha(Transform parent, float alpha)
    {
        List<Image> images = HCUtils.FindComponentsRecursively<Image>(parent);
        List<TMP_Text> texts = HCUtils.FindComponentsRecursively<TMP_Text>(parent);

        foreach(Image image in images)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        foreach(TMP_Text text in texts)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
