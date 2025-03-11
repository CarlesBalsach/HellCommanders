using UnityEngine;
using System.Collections.Generic;

public class CSVReaderGPT
{
    public static List<Dictionary<string, object>> Read(string file)
    {
        TextAsset data = Resources.Load<TextAsset>(file);
        if (data == null)
        {
            Debug.LogError("File not found: " + file);
            return new List<Dictionary<string, object>>();
        }

        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        string[] lines = data.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        if (lines.Length == 0) return rows;

        // Adjust headers to handle (str) and store which are strings
        string[] headers = ParseCSVLine(lines[0]);
        bool[] forceString = new bool[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Contains("(str)"))
            {
                forceString[i] = true; // Mark as forced string
                headers[i] = headers[i].Replace("(str)", "").Trim(); // Remove (str) tag from header
            }
            else
            {
                forceString[i] = false;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            if (values.Length == 0 || values[0] == "") continue;

            var row = new Dictionary<string, object>();
            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                object value = forceString[j] ? values[j] : ParseValue(values[j]);
                if(!string.IsNullOrEmpty(value.ToString()))
                {
                    row.Add(headers[j], value);
                }
            }
            rows.Add(row);
        }

        return rows;
    }

    private static string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            result.Add(current.Trim());
        }

        return result.ToArray();
    }

    private static object ParseValue(string value)
    {
        value = value.Trim('"'); // Remove surrounding quotes

        if (int.TryParse(value, out int intValue))
        {
            return intValue;
        }
        else if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
        {
            return floatValue;
        }
        else
        {
            return value;
        }
    }
}
