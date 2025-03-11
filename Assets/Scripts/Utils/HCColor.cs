using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HCColor
{
    public static Color ColorBlack = new Color(2f / 255f, 2f / 255f, 4f / 255f);
    public static Color DarkGreen = new Color(32f / 255f, 72f / 255f, 41f / 255f);
    public static Color GreenGreen = new Color(34f / 255f, 180f / 255f, 85f / 255f);
    public static Color LighterGreen = new Color(128f / 255f, 180f / 255f, 135f / 255f);
    public static Color LightGreen = new Color(146f / 255f, 229f / 255f, 161f / 255f);

    public static Color AllyColor = new Color(0f / 255f, 208f / 255f, 255f / 255f);
    public static Color EnemyColor = new Color(255f / 255f, 102f / 255f, 0f / 255f);

    public static Color EnemyBlood = new Color(59f / 255f, 92f / 255f, 10f / 255f);

    public static Color Fire = new Color(255f / 255f, 206f / 255f, 0f / 255f);

    public static Color GetPlayerColor(int index)
    {
        switch(index) {
            case 0:
                return Color.green;
            case 1:
                return Color.red;
            case 2:
                return new Color(0f, 1f, 1f);
            case 3:
                return new Color(1f, 0f, 1f);
        }
        return Color.black;
    }

    public static Color GetPlayerColorDark(int index)
    {
        Color c = GetPlayerColor(index);
        Color.RGBToHSV(c, out float H, out float S, out float V);
        return Color.HSVToRGB(H, S, V * 0.5f);
    }

    public static Color GetSparkColor()
    {
        return Color.HSVToRGB(Random.Range(0, 55) / 360f, 1f, 1f);
    }
}
