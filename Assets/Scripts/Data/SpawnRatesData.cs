using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRatesData
{
    Dictionary<int, int> SpawnWavePower = new Dictionary<int, int>();
    int _maxWave = -1;

    public SpawnRatesData()
    {

    }

    public void AddWavePower(int wave, int power)
    {
        SpawnWavePower[wave] = power;
        _maxWave = Mathf.Max(_maxWave, wave);
    }

    public int GetWavePower(int wave)
    {
        wave = Mathf.Min(wave, _maxWave);
        return SpawnWavePower[wave];
    }
}
