using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UICharacterInfo : MonoBehaviour
{
    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text NameText;
    [SerializeField] TMP_Text DescriptionText;
    [SerializeField] List<UIQuipCharSelect> Quips; // Max 3

    public void Init(CharacterData data)
    {
        CharacterImage.sprite = PrefabController.Instance.GetCharacterSprite(data);
        NameText.text = data.Name.Replace(" ", "\n");
        DescriptionText.text = data.Description;
        
        foreach(UIQuipCharSelect quip in Quips)
        {
            quip.gameObject.SetActive(false);
        }

        for(int i = 0; i < data.Quips.Count && i < Quips.Count; i++)
        {
            Quips[i].gameObject.SetActive(true);
            Quips[i].Init(data.Quips[i]);
        }
    }
}
