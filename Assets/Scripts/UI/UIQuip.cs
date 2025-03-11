using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class UIQuip : MonoBehaviour, IPointerClickHandler
{
    public event Action<QuipData> OnQuipClicked;

    [SerializeField] Image QuipImage;
    [SerializeField] TMP_Text QuipName;
    [SerializeField] TMP_Text QuipSequence;
    [SerializeField] TMP_Text Cooldown;

    QuipData _quip;

    public void Init(QuipData quip)
    {
        _quip = quip;
        QuipImage.sprite = PrefabController.Instance.GetQuipSprite(quip);
        QuipName.text = quip.Name;
        QuipSequence.text = GetQuipSequence();
        
        if(Cooldown != null)
        {
            Cooldown.text = string.Empty;
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

    public void UpdateSequence(List<Arrow> sequence)
    {
        UpdateSequenceArrows(sequence);
        if(_quip.SequenceWithinCommand(sequence))
        {
            SetAlpha(1f);
        }
        else
        {
            SetAlpha(0.4f);
        }
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

    void UpdateSequenceArrows(List<Arrow> sequence)
    {
        string seqText = string.Empty;
        int i = 0;
        if(_quip.SequenceWithinCommand(sequence))
        {
            while(i < sequence.Count)
            {
                seqText += HCEmoji.GetArrowFull(sequence[i]);
                i++;
            }
        }
        while(i < _quip.Command.Count)
        {
            seqText += HCEmoji.GetArrowEmpty(_quip.Command[i]);
            i++;
        }

        QuipSequence.text = seqText;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnQuipClicked?.Invoke(_quip);
    }

    public int GetId()
    {
        if(_quip != null)
        {
            return _quip.Id;
        }
        return -1;
    }

    public void UpdateCooldown(float cooldown)
    {
        if(cooldown <= 0)
        {
            SetAlpha(1f);
            Cooldown.text = string.Empty;
        }
        else
        {
            SetAlpha(0.3f);
            Color c = Cooldown.color;
            c.a = 1f;
            Cooldown.color = c;
            Cooldown.text = $"{((int)cooldown) + 1}s";
        }
    }
}
