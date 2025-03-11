using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using System.Text;
using System;

public class HCUtils
{
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static List<int> ConvertStringToIntList(string str)
    {
        // Step 1: Remove the brackets
        str = str.Trim(new char[] { '[', ']' });

        // Step 2 and 3: Split the string and convert each part to an integer
        List<int> numbers = str.Split(',')
                               .Select(s => int.Parse(s.Trim()))
                               .ToList();

        return numbers;
    }

    public static Vector2 RotateVector2(Vector2 v, float degrees)
    {
        // Convert degrees to radians
        float radians = degrees * Mathf.Deg2Rad;

        // Rotation matrix components
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        // Apply rotation matrix
        float newX = v.x * cos + v.y * sin;
        float newY = -v.x * sin + v.y * cos;

        return new Vector2(newX, newY);
    }

    public static List<T> FindComponentsRecursively<T>(Transform parent) where T : Component
    {
        List<T> results = new List<T>();

        if(parent == null)
        {
            return results;
        }

        T component = parent.GetComponent<T>();
        if (component != null)
        {
            results.Add(component);
        }

        foreach (Transform child in parent)
        {
            results.AddRange(FindComponentsRecursively<T>(child));
        }

        return results;
    }

    public static void DestroyAllChildren(Transform parent)
    {
        for(int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static Color GetColorFromHSVRange(int HA, int HB)
    {
        float H = UnityEngine.Random.Range(HA, HB + 1) / 360f;
        return Color.HSVToRGB(H, 1f, 1f);
    }

    public static Vector3 GetPositionInArea(Vector3 position, float radius)
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        Vector2 distance = Vector2.up * UnityEngine.Random.Range(0f, radius);
        return position + RotateVector2(distance, angle).ToV3();
    }

    public static int Random1m1()
    {
        return UnityEngine.Random.Range(0,2) * 2 - 1;
    }

    public static bool InDistance2D(Vector3 posA, Vector3 posB, float distance)
    {
        float sqrMagnitude = (posA.ToV2() - posB.ToV2()).sqrMagnitude;
        return sqrMagnitude <= distance * distance;
    }

    public static string TruncateBytes(string input, int bytes)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Convert the input string to UTF-8 bytes
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(input);

        // If the byte length is within the limit, return the original string
        if (utf8Bytes.Length <= bytes)
        {
            return input;
        }

        // Truncate the byte array to the maximum length
        byte[] truncatedBytes = new byte[bytes];
        Array.Copy(utf8Bytes, truncatedBytes, bytes);

        // Convert the truncated byte array back to a string
        string truncatedString = Encoding.UTF8.GetString(truncatedBytes);

        // Ensure the truncated string does not end in the middle of a multi-byte character
        while (Encoding.UTF8.GetByteCount(truncatedString) > bytes)
        {
            truncatedString = truncatedString.Substring(0, truncatedString.Length - 1);
        }

        return truncatedString;
    }
}

public static class VectorExtensions
{
    public static Vector2 ToV2(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 ToV3(this Vector2 v)
    {
        return new Vector3(v.x, v.y, 0f);
    }
}

public static class ListExtensions
{
    public static FixedList32Bytes<bool> ToFixedList32Bytes(this List<bool> list)
    {
        FixedList32Bytes<bool> fixedList = new FixedList32Bytes<bool>();
        foreach (bool item in list)
        {
            if (fixedList.Length >= 32)
                break; // FixedList32Bytes can only hold up to 32 items
            
            fixedList.Add(item);
        }
        return fixedList;
    }

    public static List<bool> ToList(this FixedList32Bytes<bool> fixedList)
    {
        List<bool> list = new List<bool>();
        for (int i = 0; i < fixedList.Length; i++)
        {
            list.Add(fixedList[i]);
        }
        return list;
    }
}