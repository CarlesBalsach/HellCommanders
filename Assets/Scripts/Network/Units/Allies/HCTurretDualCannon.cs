using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCTurretDualCannon : HCUnit
{
    const string BulletPrefab = "HCBullet";
    [SerializeField] Transform SpawnLeft;
    [SerializeField] Transform SpawnRight;

    NetworkVariable<float> _orientation = new NetworkVariable<float>(0f);

    protected override void Update()
    {
        base.Update();

        if(!_dead)
        {
            RotateTowardsTarget();
        }
    }

    protected override void OrientateTowardsTarget()
    {
        if(TargetInSight())
        {
            Vector2 direction = _target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            _orientation.Value = angle;
        }
    }

    protected override void MoveTowardsTarget()
    {
        // Static Object, Does not move
    }

    protected override void AttackTarget()
    {
        OrientateTowardsTarget();
        RotateTowardsTarget();

        Vector2 dir = _target.transform.position - transform.position;
        SpawnBullet(SpawnLeft.position, dir);
        SpawnBullet(SpawnRight.position, dir);
        SpawnBulletsParticlesClientRpc(dir);
        _roundsLeft.Value = _roundsLeft.Value - 1;
    }

    protected override void DeadAnimation()
    {
        base.DeadAnimation();
        HCVisualEffects.BuildingExplosionEffect(transform.position, Stats.Radius, 50, 10);
    }

    void RotateTowardsTarget()
    {
        UnitSprite.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, _orientation.Value));
    }

    void SpawnBullet(Vector3 position, Vector2 dir)
    {
        dir.Normalize();

        NetworkBehaviour projectile = PrefabController.Instance.NetworkSpawnProjectile(BulletPrefab, position);
        HCBullet bullet = projectile.GetComponent<HCBullet>();
        bullet.Init(dir, Stats.Damage, transform);
    }

    [ClientRpc]
    void SpawnBulletsParticlesClientRpc(Vector2 dir)
    {
        List<Vector3> positions = new List<Vector3>()
        {
            SpawnLeft.position,
            SpawnRight.position
        };

        foreach(Vector2 pos in positions)
        {
            dir = dir.normalized * 0.5f;

            SpriteRenderer sparkL = PrefabController.Instance.SpawnSpark(pos, 0.25f);
            sparkL.color = Color.gray;
            Vector2 dirL = HCUtils.RotateVector2(dir, -45);
            sparkL.transform.DOMove(pos + dirL, 1f).SetEase(Ease.OutQuad);
            sparkL.transform.DOScale(0f, 1f).OnComplete(()=>Destroy(sparkL.gameObject));

            SpriteRenderer sparkR = PrefabController.Instance.SpawnSpark(pos, 0.25f);
            sparkR.color = Color.gray;
            Vector2 dirR = HCUtils.RotateVector2(dir, 45);
            sparkR.transform.DOMove(pos + dirR, 1f).SetEase(Ease.OutQuad);
            sparkR.transform.DOScale(0f, 1f).OnComplete(()=>Destroy(sparkL.gameObject));
        }
    }
}
