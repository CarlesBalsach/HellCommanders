using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuipOrbitalBomb : HCQuip
{
    protected override void Activate()
    {
        DropBomb();
    }

    void DropBomb()
    {
        Vector3 bombSpawn = new Vector3(transform.position.x, transform.position.y + HCMap.SIZE, 0f);
        bombSpawn.x += Random.Range(-HCMap.SIZE, HCMap.SIZE);
        BombDrop bomb = PrefabController.Instance.SpawnBombDrop(bombSpawn);
        bomb.transform.localScale = bomb.transform.localScale * _quip.Damage / 20f;

        bomb.Init(bombSpawn, transform.position, _quip.MainRadius / 2f, Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        bomb.OnBombDropped += OnBombDropped;
    }

    void OnBombDropped(Vector3 position)
    {
        ExplosionAoE explosion = PrefabController.Instance.SpawnExplosionAoE(position);
        explosion.Init(Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        
        // It might have been already destroyed from the server
        if(CallLine != null)
        {
            CallLine.enabled = false;
        }

        if(NetworkManager.Singleton.IsServer)
        {
            _readyToDespawn = true;
            DealDamage(position);
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
}
