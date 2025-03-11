using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public int Id;
    public string Name;
    public string Description;
    public List<QuipData> Quips = new List<QuipData>();

    public CharacterData(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public void AddQuip(QuipData quip)
    {
        Quips.Add(quip);
    }
}
