using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuipOrbitalLaser : HCQuip
{
    const string LASER_PREFAB = "HCOrbitalLaser";

    protected override void Activate()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            SpawnLaser();
        }
    }

    void SpawnLaser()
    {
        NetworkBehaviour laser = PrefabController.Instance.NetworkSpawnUtility(LASER_PREFAB, transform.position);
        laser.GetComponent<HCOrbitalLaser>().Init(_quip);
        _readyToDespawn = true;
    }
}
