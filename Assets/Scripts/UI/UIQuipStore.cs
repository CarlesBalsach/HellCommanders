using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class UIQuipStore : MonoBehaviour, IPointerClickHandler
{
    public event Action<QuipData> OnQuipClicked;

    [SerializeField] Image QuipImage;
    [SerializeField] TMP_Text QuipName;
    [SerializeField] TMP_Text QuipSequence;
    [SerializeField] TMP_Text Price;

    QuipData _quip;

    public void Init(QuipData quip, bool available)
    {
        _quip = quip;
        QuipImage.sprite = PrefabController.Instance.GetQuipSprite(quip);
        QuipName.text = quip.Name;
        QuipSequence.text = GetQuipSequence();
        Price.text = GetQuipPrice();
        if(!available)
        {
            SetAlpha(0.3f);
        }
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

    string GetQuipPrice()
    {
        string price = string.Empty;
        foreach(var kvp in _quip.Price)
        {
            price += $"{HCEmoji.Get(kvp.Key.Image)}{kvp.Value} ";
        }
        return price.Trim();
    }

    void SetAlpha(float alpha)
    {
        List<Image> images = HCUtils.FindComponentsRecursively<Image>(transform);
        List<TMP_Text> texts = HCUtils.FindComponentsRecursively<TMP_Text>(transform);

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

    public void OnPointerClick(PointerEventData eventData)
    {
        OnQuipClicked?.Invoke(_quip);
    }
}
