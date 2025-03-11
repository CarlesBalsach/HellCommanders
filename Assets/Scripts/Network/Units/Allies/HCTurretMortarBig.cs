using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCTurretMortarBig : HCUnit
{
    [SerializeField] ExplosiveShell ExplosiveShellPrefab;

    protected override void OrientateTowardsTarget()
    {
        // Static Object, Does not move
    }

    protected override void MoveTowardsTarget()
    {
        // Static Object, Does not move
    }

    protected override void DeadAnimation()
    {
        base.DeadAnimation();
        HCVisualEffects.BuildingExplosionEffect(transform.position, Stats.Radius, 50, 10);
    }

    protected override void FindTarget()
    {
        _target = null;
        List<HCUnit> targets = new List<HCUnit>();

        foreach(HCUnit unit in HCMap.Instance.EnemyUnits)
        {
            if(!unit.IsDead())
            {
                float unitDistanceSqr = (unit.transform.position - transform.position).sqrMagnitude;
                if(unitDistanceSqr >= Stats.MinDistance * Stats.MinDistance && unitDistanceSqr <= Stats.AttackDistance * Stats.AttackDistance)
                {
                    targets.Add(unit);
                }
            }
        }

        if(targets.Count > 0)
        {
            _target = targets[Random.Range(0, targets.Count)];
        }
    }

    protected override bool TargetInSight()
    {
        if(_target == null)
        {
            return false;
        }

        float distance = (transform.position - _target.transform.position).magnitude;
        return distance >= Stats.MinDistance && distance <= Stats.AttackDistance;
    }

    protected override void AttackTarget()
    {
        SpawnShellClientRpc(_target.transform.position);
        _roundsLeft.Value = _roundsLeft.Value - 1;
    }

    [ClientRpc]
    void SpawnShellClientRpc(Vector2 position)
    {
        ExplosiveShell shell = Instantiate(ExplosiveShellPrefab);
        shell.transform.position = transform.position;
        shell.Init(position, Stats.Damage);

        for (int i = 0; i < 360; i+=18)
        {
            Vector2 dir = HCUtils.RotateVector2(Vector2.up, i);
            Vector3 pos = transform.position.ToV2() + dir * 0.2f;
            SpriteRenderer spark = PrefabController.Instance.SpawnSpark(pos, 0.25f);
            spark.color = Color.gray;
            spark.transform.DOMove(pos + dir.ToV3(), 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(0f, 1f).OnComplete(()=>Destroy(spark.gameObject)); 
        }
    }
}
