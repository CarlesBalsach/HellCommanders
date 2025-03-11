using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuipUnitDeploy : HCQuip
{
    protected override void Activate()
    {
        DropUnit();
    }

    void DropUnit()
    {
        UnitDrop capsule = PrefabController.Instance.SpawnUnitDrop(transform.position);
        capsule.transform.localScale = capsule.transform.localScale * _quip.Damage / 20f;

        capsule.Init(transform.position, _quip.Size);
        capsule.OnUnitDropped += OnUnitDropped;
    }

    void OnUnitDropped()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            SpawnUnit();
            _readyToDespawn = true;
        }
    }

    void SpawnUnit()
    {
        HCMap.Instance.SpawnAlly(_quip.UnitPrefab, transform.position);
    }
}
