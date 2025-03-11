using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData
{
    public int Id;
    public string PrefabName;
    public int Power;
    public int MinTime;
    public float Radius;
    
    public EnemyData(int id, string prefab_name, int power, int min_time, float radius)
    {
        Id = id;
        PrefabName = prefab_name;
        Power = power;
        MinTime = min_time;
        Radius = radius;
    }
}
