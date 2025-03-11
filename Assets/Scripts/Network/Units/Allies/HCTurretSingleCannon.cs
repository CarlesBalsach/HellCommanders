using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCTurretSingleCannon : HCUnit
{
    const string BulletPrefab = "HCBullet";

    NetworkVariable<float> _orientation = new NetworkVariable<float>(0f);

    protected override void Update()
    {
        base.Update();

        if(!_dead && _ready)
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
        if(_ready)
        {
            OrientateTowardsTarget();
            RotateTowardsTarget();
            SpawnBullet();
            _roundsLeft.Value = _roundsLeft.Value - 1;
        }
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

    void SpawnBullet()
    {
        Vector2 dir = _target.transform.position - transform.position;
        dir.Normalize();

        SpawnBulletParticlesClientRpc(transform.position.ToV2() + dir * Stats.Radius, dir);

        Vector3 position = transform.position + dir.ToV3() * Stats.Radius;
        NetworkBehaviour projectile = PrefabController.Instance.NetworkSpawnProjectile(BulletPrefab, position);
        HCBullet bullet = projectile.GetComponent<HCBullet>();
        bullet.Init(dir, Stats.Damage, transform);
    }

    [ClientRpc]
    void SpawnBulletParticlesClientRpc(Vector2 pos, Vector2 dir)
    {
        dir *= 0.5f;

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
