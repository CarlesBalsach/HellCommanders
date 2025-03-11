using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterButton : MonoBehaviour
{

    UICharSelect _uiCharSelect;
    CharacterData _data;
    public Image CharacterImage;

    
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            _uiCharSelect.CharacterSelected(_data);
        });
    }

    void Start()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged += PlayerDataChanged;
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged -= PlayerDataChanged;
    }
    
    public void Init(UICharSelect uiCharSelect, CharacterData data)
    {
        _uiCharSelect = uiCharSelect;
        _data = data;
        CharacterImage.sprite = PrefabController.Instance.GetCharacterSprite(data);
    }

    void PlayerDataChanged()
    {
        // Ensure that if we are leaving the Lobby when there is just 1 person this is not triggering an error
        if(HCNetworkManager.Instance.PlayersConnected() > 0)
        {
            GetComponent<Button>().interactable = !HCNetworkManager.Instance.GetPlayerData().Ready;
        }
    }
}
