using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Arrow {
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class QuipData
{
    public int Id;
    public string Name;
    public List<Arrow> Command;
    public int ActivationTime;
    public int Cooldown;
    public float MainRadius;
    public float SecondaryRadius;
    public float AreaOfEffect;
    public int Damage;
    public float Size;
    public float Range;
    public float MinRange;
    public int Rounds;
    public string Image;
    public string Description;
    public string QuipPrefab;
    public string UnitPrefab;
    public Dictionary<MaterialData, int> Price = new Dictionary<MaterialData, int>();

    public QuipData(int id, string name, List<Arrow> command, int activationTime, int cooldown, float mainRadius,
                    float secondaryRadius, float aoe, int damage, float size, float range, float minRange, int rounds,
                    string image, string description, string quipPrefab, string unitPrefab)
    {
        Id = id;
        Name = name;
        Command = command;
        ActivationTime = activationTime;
        Cooldown = cooldown;
        MainRadius = mainRadius;
        SecondaryRadius = secondaryRadius;
        AreaOfEffect = aoe;
        Damage = damage;
        Size = size;
        Range = range;
        MinRange = minRange;
        Rounds = rounds;
        Image = image;
        Description = description;
        QuipPrefab = quipPrefab;
        UnitPrefab = unitPrefab;
    }

    public bool SequenceWithinCommand(List<Arrow> sequence)
    {
        if(sequence.Count > Command.Count)
        {
            return false;
        }

        for(int i = 0; i < sequence.Count; i++)
        {
            if(sequence[i] != Command[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool SequenceActivatedCommand(List<Arrow> sequence)
    {
        if(sequence.Count != Command.Count)
        {
            return false;
        }

        for(int i = 0; i < sequence.Count; i++)
        {
            if(sequence[i] != Command[i])
            {
                return false;
            }
        }

        return true;
    }

    public void AddMaterialPrice(MaterialData material, int amount)
    {
        Price[material] = amount;
    }
}
