using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuipOrbitalBarrage : HCQuip
{
    const float DELAY_BETWEEN_ROUNDS = 1f;
    int _rounds = 0;

    protected override void Activate()
    {
        _rounds = _quip.Rounds;
        for(int i = 0; i < _quip.Rounds; i++)
        {
            StartCoroutine(DropBombWithDelay(i * DELAY_BETWEEN_ROUNDS));
        }
    }

    IEnumerator DropBombWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 bombSpawn = new Vector3(transform.position.x, transform.position.y + HCMap.SIZE, 0f);
        bombSpawn.x += Random.Range(-HCMap.SIZE, HCMap.SIZE);
        BombDrop bomb = PrefabController.Instance.SpawnBombDrop(bombSpawn);
        bomb.transform.localScale = bomb.transform.localScale * _quip.Damage / 20f;

        float angle = Random.Range(0f, 360f);
        Vector2 dir = Vector2.up * (_quip.AreaOfEffect - Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        Vector3 dropPos = HCUtils.RotateVector2(dir, angle);

        bomb.Init(bombSpawn, transform.position + dropPos, _quip.MainRadius / 2f, Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        bomb.OnBombDropped += OnBombDropped;
    }

    void OnBombDropped(Vector3 position)
    {
        ExplosionAoE explosion = PrefabController.Instance.SpawnExplosionAoE(position);
        explosion.Init(Mathf.Max(_quip.MainRadius, _quip.SecondaryRadius));
        CallLine.enabled = false;

        if(NetworkManager.Singleton.IsServer)
        {
            DealDamage(position);
            _rounds--;
            if(_rounds <= 0)
            {
                _readyToDespawn = true;
            }
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
