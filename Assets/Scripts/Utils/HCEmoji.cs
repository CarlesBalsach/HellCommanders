using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HCEmoji
{
    public static string HEART = "<sprite name=\"HEART\">";
    public static string SWORD = "<sprite name=\"SWORD\">";
    public static string SHIELD = "<sprite name=\"SHIELD\">";
    public static string ACTIVATION_TIME = "<sprite name=\"ACTIVATION_TIME\">";
    public static string COOLDOWN = "<sprite name=\"COOLDOWN\">";
    public static string RADIUS = "<sprite name=\"RADIUS\">";
    public static string SECOND_RADIUS = "<sprite name=\"SECOND_RADIUS\">";
    public static string AREA = "<sprite name=\"AREA\">";
    public static string BULLETS = "<sprite name=\"BULLETS\">";
    public static string CREDITS = "<sprite name=\"CREDITS\">";

    public static string Get(string key)
    {
        return $"<sprite name=\"{key}\">";
    }

    public static string GetArrowEmpty(Arrow arrow)
    {
        switch(arrow) {
            case Arrow.UP:
                return Get("ARROW_EMPTY_UP");
            case Arrow.DOWN:
                return Get("ARROW_EMPTY_DOWN");
            case Arrow.LEFT:
                return Get("ARROW_EMPTY_LEFT");
            case Arrow.RIGHT:
                return Get("ARROW_EMPTY_RIGHT");
        }
        return string.Empty;
    }

    public static string GetArrowFull(Arrow arrow)
    {
        switch(arrow) {
            case Arrow.UP:
                return Get("ARROW_FULL_UP");
            case Arrow.DOWN:
                return Get("ARROW_FULL_DOWN");
            case Arrow.LEFT:
                return Get("ARROW_FULL_LEFT");
            case Arrow.RIGHT:
                return Get("ARROW_FULL_RIGHT");
        }
        return string.Empty;
    }

    public static string GetArrowRed(Arrow arrow)
    {
        switch(arrow) {
            case Arrow.UP:
                return Get("ARROW_RED_UP");
            case Arrow.DOWN:
                return Get("ARROW_RED_DOWN");
            case Arrow.LEFT:
                return Get("ARROW_RED_LEFT");
            case Arrow.RIGHT:
                return Get("ARROW_RED_RIGHT");
        }
        return string.Empty;
    }
}
