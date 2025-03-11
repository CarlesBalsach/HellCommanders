using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuipOrbitalNapalm : HCQuip
{
    protected override void Activate()
    {
        DropNapalm();
    }

    void DropNapalm()
    {
        Vector3 bombSpawn = new Vector3(transform.position.x, transform.position.y + HCMap.SIZE, 0f);
        bombSpawn.x += Random.Range(-HCMap.SIZE, HCMap.SIZE);
        BombDrop bomb = PrefabController.Instance.SpawnBombDrop(bombSpawn);
        bomb.transform.localScale = bomb.transform.localScale * _quip.Damage / 20f;

        bomb.Init(bombSpawn, transform.position, _quip.MainRadius / 2f, Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        bomb.OnBombDropped += OnNapalmDropped;
    }

    void OnNapalmDropped(Vector3 position)
    {
        ExplosionAoE explosion = PrefabController.Instance.SpawnExplosionAoE(position);
        explosion.Init(Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        CallLine.enabled = false;

        if(NetworkManager.Singleton.IsServer)
        {
            DealDamage(position);
            SpawnFire(position);
            _readyToDespawn = true;
        }
    }

    void DealDamage(Vector3 position) // Server Only
    {
        List<HCUnit> units = new List<HCUnit>(HCMap.Instance.UnitsSpawned);
        foreach(HCUnit unit in units)
        {
            if(!unit.IsDead())
            {
                float distance = (unit.transform.position.ToV2() - position.ToV2()).magnitude;
                if(distance <= _quip.MainRadius)
                {
                    unit.IsHit(_quip.Damage);
                }
                else if(distance <= _quip.SecondaryRadius)
                {
                    unit.IsHit(_quip.Damage / 2);
                }
            }
        }
    }

    void SpawnFire(Vector3 position) // Server Only
    {
        HCMap.Instance.SpawnFire(position, _quip.MainRadius);
    }
}
