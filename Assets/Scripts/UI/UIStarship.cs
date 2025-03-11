using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStarship : MonoBehaviour
{
    [SerializeField] Button JumpButton;
    [SerializeField] Button BuyQuipButton;
    [SerializeField] Button UpgradeQuipButton;
    [SerializeField] Button BlackMarketButton;

    [SerializeField] UIStarshipJump JumpWindow;
    [SerializeField] UIStarshipBlackMarket BlackMarketWindow;
    [SerializeField] UIStarshipQuips QuipsWindow;

    public void Awake()
    {
        JumpButton.onClick.AddListener(() => {
            ShowButtons(false);
            JumpWindow.gameObject.SetActive(true);
            JumpWindow.Init();
        });

        BlackMarketButton.onClick.AddListener(() => {
            ShowButtons(false);
            BlackMarketWindow.gameObject.SetActive(true);
        });

        BuyQuipButton.onClick.AddListener(() => {
            ShowButtons(false);
            QuipsWindow.gameObject.SetActive(true);
        });
    }

    void Start()
    {
        JumpWindow.OnClose += CloseAllWindows;
        BlackMarketWindow.OnClose += CloseAllWindows;
        QuipsWindow.OnClose += CloseAllWindows;
        CloseAllWindows();
    }

    void CloseAllWindows()
    {
        JumpWindow.gameObject.SetActive(false);
        BlackMarketWindow.gameObject.SetActive(false);
        QuipsWindow.gameObject.SetActive(false);
        ShowButtons(true);
    }

    void ShowButtons(bool show)
    {
        JumpButton.gameObject.SetActive(show);
        BuyQuipButton.gameObject.SetActive(show);
        UpgradeQuipButton.gameObject.SetActive(show);
        BlackMarketButton.gameObject.SetActive(show);
    }
}
