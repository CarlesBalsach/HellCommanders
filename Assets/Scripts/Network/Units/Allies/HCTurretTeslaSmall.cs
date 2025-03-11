using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCTurretTeslaSmall : HCUnit
{
    const int TARGETS = 3;
    const float JUMP_DISTANCE = 0.5f;

    [SerializeField] Lightning LightningPrefab;

    protected override void OrientateTowardsTarget()
    {
        // Static Object, Does not move
    }

    protected override void MoveTowardsTarget()
    {
        // Static Object, Does not move
    }

    protected override void AttackTarget()
    {
        if(_ready)
        {
            SpawnLightning();
            _roundsLeft.Value = _roundsLeft.Value - 1;
        }
    }

    protected override void DeadAnimation()
    {
        base.DeadAnimation();
        HCVisualEffects.BuildingExplosionEffect(transform.position, Stats.Radius, 50, 10);
    }

    void SpawnLightning()
    {
        Vector2 dir = _target.transform.position - transform.position;
        dir.Normalize();
        List<HCUnit> targets = GetLightningTargets();
        DealDamage(targets);
        SpawnLightningClientRpc(GetTargetNOR(targets));
    }

    List<HCUnit> GetLightningTargets()
    {
        List<HCUnit> targets = new List<HCUnit>(){_target};
        for (int i = 1; i < TARGETS; i++)
        {
            if(targets.Count < i)
            {
                break;
            }

            List<HCUnit> enemies = new List<HCUnit>(HCMap.Instance.EnemyUnits);
            HCUtils.Shuffle(enemies);
            foreach(HCUnit enemy in enemies)
            {
                if(!targets.Contains(enemy))
                {
                    HCUnit lastTarget = targets[targets.Count - 1];
                    if(HCUtils.InDistance2D(lastTarget.transform.position, enemy.transform.position, JUMP_DISTANCE + lastTarget.Stats.Radius + enemy.Stats.Radius))
                    {
                        targets.Add(enemy);
                        break;
                    }
                }
            }
        }

        return targets;
    }

    NetworkObjectReference[] GetTargetNOR(List<HCUnit> targets)
    {
        NetworkObjectReference[] networkObjectReferences = new NetworkObjectReference[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            networkObjectReferences[i] = new NetworkObjectReference(targets[i].GetComponent<NetworkObject>());
        }

        return networkObjectReferences;
    }

    [ClientRpc]
    void SpawnLightningClientRpc(NetworkObjectReference[] targets)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var targetReference in targets)
        {
            if (targetReference.TryGet(out NetworkObject networkObject))
            {
                positions.Add(networkObject.transform.position);
            }
        }
        SpawnLightningLines(positions);
    }

    void SpawnLightningLines(List<Vector3> positions)
    {
        positions.Insert(0, transform.position);
        Lightning lightning = Instantiate(LightningPrefab);
        lightning.Init(positions);
    }

    void DealDamage(List<HCUnit> targets)
    {
        foreach(HCUnit target in targets)
        {
            target.IsHit(Stats.Damage, true);
        }
    }
}
