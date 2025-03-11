using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuipInfo : MonoBehaviour
{
    [SerializeField] Image QuipImage;
    [SerializeField] TMP_Text QuipName;
    [SerializeField] TMP_Text QuipSequence;
    [SerializeField] TMP_Text QuipDescription;
    [SerializeField] TMP_Text QuipStats;
    [SerializeField] TMP_Text Resources;
    [SerializeField] Button InstallButton;
    [SerializeField] Button DismantleButton;

    QuipData _quip;

    void Awake()
    {
        InstallButton.onClick.AddListener(()=>{
            HCPlayer.LocalInstance.InstallQuipServerRpc(_quip.Id);
        });

        DismantleButton.onClick.AddListener(()=>{
            HCPlayer.LocalInstance.DismantleQuipServerRpc(_quip.Id);
        });
    }

    public void Init(QuipData quip, bool owned)
    {
        _quip = quip;

        QuipImage.sprite = PrefabController.Instance.GetQuipSprite(_quip);
        QuipName.text = _quip.Name;
        QuipSequence.text = GetQuipSequence();
        QuipDescription.text = _quip.Description;
        QuipStats.text = GetQuipStats();
        
        Resources.text = string.Empty;
        foreach(var kvp in _quip.Price)
        {
            int amount = owned ? kvp.Value / 2 : kvp.Value;
            Resources.text += $"{HCEmoji.Get(kvp.Key.Image)}{amount} ";
        }

        Resources.text = Resources.text.Trim();
        InstallButton.gameObject.SetActive(!owned);
        DismantleButton.gameObject.SetActive(owned);

        int ownedQuips = HCPlayer.LocalInstance.Quips.Count;
        InstallButton.gameObject.SetActive(ownedQuips < HCPlayer.MAX_QUIPS);

        InstallButton.interactable = HCPlayer.LocalInstance.CanPayMaterialPrice(_quip.Price);
    }

    string GetQuipStats()
    {
        string stats = string.Empty;
        if(_quip.ActivationTime > 0)
        {
            stats += $"{HCEmoji.ACTIVATION_TIME}{_quip.ActivationTime}s ";
        }
        if(_quip.Cooldown > 0)
        {
            stats += $"{HCEmoji.COOLDOWN}{_quip.Cooldown}s ";
        }
        if(_quip.MainRadius > 0f)
        {
            stats += $"{HCEmoji.RADIUS}{_quip.MainRadius}m ";
        }
        if(_quip.SecondaryRadius > 0f)
        {
            stats += $"{HCEmoji.SECOND_RADIUS}{_quip.SecondaryRadius}m ";
        }
        if(_quip.AreaOfEffect > 0f)
        {
            stats += $"{HCEmoji.AREA}{_quip.AreaOfEffect}m ";
        }
        if(_quip.Size > 0f)
        {
            stats += $"{HCEmoji.AREA}{_quip.Size}m ";
        }
        if(_quip.Damage > 0)
        {
            stats += $"{HCEmoji.SWORD}{_quip.Damage} ";
        }
        if(_quip.Range > 0f)
        {
            stats += $"{HCEmoji.SECOND_RADIUS}{_quip.Range} ";
        }
        if(_quip.Rounds > 0)
        {
            stats += $"{HCEmoji.BULLETS}{_quip.Rounds} ";
        }
        return stats.Trim();
    }

    string GetQuipSequence()
    {
        string text = "";
        foreach(Arrow a in _quip.Command)
        {
            text += HCEmoji.GetArrowEmpty(a);
        }
        return text;
    }
}
