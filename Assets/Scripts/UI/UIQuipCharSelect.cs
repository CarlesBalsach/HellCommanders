using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuipCharSelect : MonoBehaviour
{

    [SerializeField] Image QuipImage;
    [SerializeField] TMP_Text QuipName;
    [SerializeField] TMP_Text QuipSequence;

    QuipData _quip;

    public void Init(QuipData quip)
    {
        _quip = quip;
        QuipImage.sprite = PrefabController.Instance.GetQuipSprite(quip);
        QuipName.text = quip.Name;
        QuipSequence.text = GetQuipSequence();
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
