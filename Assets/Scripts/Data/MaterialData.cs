using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialData
{
    public int Id {get; private set;}
    public string Name {get; private set;}
    public int Price {get; private set;}
    public string Image {get; private set;}
    public string Description {get; private set;}

    public MaterialData(int id, string name, int price, string image, string description)
    {
        Id = id;
        Name = name;
        Price = price;
        Image = image;
        Description = description;
    }
}
