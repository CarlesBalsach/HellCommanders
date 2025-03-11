using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameAllies : MonoBehaviour
{
    [SerializeField] List<UIGameAlly> Allies;

    void Start()
    {
        HC_OnPlayerDataChanged();

        HCNetworkManager.Instance.OnPlayerDataChanged += HC_OnPlayerDataChanged;
        HCGameManager.Instance.State.OnValueChanged += HC_OnGameStateChanged;
    }

    void OnDestroy()
    {
        HCNetworkManager.Instance.OnPlayerDataChanged -= HC_OnPlayerDataChanged;
        HCGameManager.Instance.State.OnValueChanged -= HC_OnGameStateChanged;
    }

    void HC_OnGameStateChanged(HCGameManager.GameState previousValue, HCGameManager.GameState newValue)
    {
        UpdateAllies();
    }


    void HC_OnPlayerDataChanged()
    {
        UpdateAllies();
    }

    void UpdateAllies()
    {
        if(HCGameManager.Instance.State.Value == HCGameManager.GameState.GAME_ACTIVE)
        {
            Allies.ForEach(ally => ally.gameObject.SetActive(false));

            List<PlayerDataNetwork> players = HCNetworkManager.Instance.GetPlayersData();
            for (int i = 0; i < players.Count; i++)
            {
                Allies[i].gameObject.SetActive(true);
                Allies[i].UpdateData(players[i]);
            }
        }
    }
}
