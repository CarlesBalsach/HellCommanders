using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HCPrefs
{
    public const string PLAYER_NAME = "PLAYER_NAME";


    public static string GetPlayerName()
    {
        if(PlayerPrefs.HasKey(PLAYER_NAME))
        {
            return PlayerPrefs.GetString(PLAYER_NAME);
        }

        string randName = $"PlayerName{Random.Range(1000, 10000)}";
        PlayerPrefs.SetString(PLAYER_NAME, randName);
        return randName;
    }

    public static void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PLAYER_NAME, name);
    }
}
