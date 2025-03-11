using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGamePlayer : MonoBehaviour
{
    [SerializeField] Image CharacterImage;
    [SerializeField] TMP_Text CharacterName;
    [SerializeField] TMP_Text PlayerName;

    [SerializeField] List<UIQuip> Quips;

    [SerializeField] TMP_Text SequenceArrows;

    bool _missPenaltyActive = false;
    List<Arrow> _sequence;

    void Start()
    {
        QuipInputController.Instance.OnSequenceChanged += OnSequenceChanged;
        QuipInputController.Instance.OnMissCounterChanged += OnMissCounterChanged;

        PlayerDataNetwork playerData = HCNetworkManager.Instance.GetPlayerData();
        CharacterData character = GameData.Instance.GetCharacterData(playerData.CharacterId);
        
        CharacterImage.sprite = PrefabController.Instance.GetCharacterSprite(character);
        CharacterName.text = character.Name;
        PlayerName.text = playerData.Name.ToString();

        Quips.ForEach(quip => quip.gameObject.SetActive(false));

        Quips[Quips.Count - 1].gameObject.SetActive(true);
        Quips[Quips.Count - 1].Init(GameData.Instance.GetQuipData(0));

        for (int i = 0; i < HCPlayer.LocalInstance.Quips.Count; i++)
        {
            QuipData quip = GameData.Instance.GetQuipData(HCPlayer.LocalInstance.Quips[i]);
            Quips[i].gameObject.SetActive(true);
            Quips[i].Init(quip);
        }
    
        SequenceArrows.text = string.Empty;
    }

    private void OnDestroy()
    {
        if(QuipInputController.Instance != null)
        {
            QuipInputController.Instance.OnSequenceChanged -= OnSequenceChanged;
            QuipInputController.Instance.OnMissCounterChanged -= OnMissCounterChanged;
        }
    }

    void ChangeSequenceText(List<Arrow> sequence, bool missPenalty)
    {
        string seqText = string.Empty;
        foreach(Arrow arrow in sequence)
        {
            if(!missPenalty)
            {
                seqText += HCEmoji.GetArrowFull(arrow);
            }
            else
            {
                seqText += HCEmoji.GetArrowRed(arrow);
            }
        }
        SequenceArrows.text = seqText;
    }

    void OnSequenceChanged(List<Arrow> sequence)
    {
        _sequence = sequence;
        ChangeSequenceText(sequence, false);

        foreach(UIQuip quip in Quips)
        {
            if(quip.gameObject.activeSelf)
            {
                quip.UpdateSequence(sequence);
            }
        }
    }

    void OnMissCounterChanged(float counter)
    {
        if(counter > 0f)
        {
            float alpha = counter / QuipInputController.MISS_PENALTY;
            SequenceArrows.color = new Color(1f, 1f, 1f, alpha);

            if(!_missPenaltyActive)
            {
                _missPenaltyActive = true;
                ChangeSequenceText(_sequence, true);
            }
        }
        else
        {
            SequenceArrows.color = new Color(1f, 1f, 1f, 1f);
            _missPenaltyActive = false;
        }
    }

    public void UpdateQuipCooldown(int quipId, float cooldown)
    {
        foreach(UIQuip quip in Quips)
        {
            if(quip.GetId() == quipId)
            {
                quip.UpdateCooldown(cooldown);
            }
        }
    }
}
