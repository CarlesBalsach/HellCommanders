using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCEnemySpaceBeetle : HCUnit
{
    const float NORMAL_ATTACK_DISTANCE = 0.15f;
    const float CHARGE_TIME = 2f;
    const float STOMP_TIME = 3.5f;
    const float CHARGE_SPEED = 3f;
    const int STOMP_DAMAGE_MULTIPLIER = 3;
    const float SMOKE_TRAIL_TIME = 0.33f;

    NetworkVariable<float> RedColor = new NetworkVariable<float>(0f);

    HCUnit _chargeTarget;
    bool _charging = false;
    float _chargeChrono = 0f;
    bool _stomping = false;
    float _stompChrono = 0f;
    Vector3 _stompDirection;
    int _smokesSpawned = 0;


    protected override void Update()
    {
        base.Update();

        float a = UnitSprite.color.a;
        Color c = Color.Lerp(Color.white, Color.red, RedColor.Value);
        c.a = a;
        UnitSprite.color = c;

        if(NetworkManager.Singleton.IsServer)
        {
            if(_charging && !_stomping)
            {
                _chargeChrono += Time.deltaTime;
                RedColor.Value = Mathf.Clamp(_chargeChrono / CHARGE_TIME, 0f, 1f);
                if(_chargeChrono > CHARGE_TIME)
                {
                    _stomping = true;
                    _stompChrono = 0f;
                    _smokesSpawned = 0;
                    _stompDirection = (_chargeTarget.transform.position - transform.position).normalized;
                }
            }
            if(_stomping)
            {
                if(_stompChrono > SMOKE_TRAIL_TIME * _smokesSpawned)
                {
                    _smokesSpawned++;
                    SpawnSmokeTrailClientRpc();
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(_stomping && NetworkManager.Singleton.IsServer)
        {
            _stompChrono += Time.fixedDeltaTime;
            if(_stompChrono < STOMP_TIME)
            {
                Rigidbody.MovePosition(transform.position + _stompDirection * CHARGE_SPEED * Time.fixedDeltaTime);
            }
            else
            {
                StopStomping();
            }
        }
    }

    protected override void AttackTarget()
    {
        if(_charging)
        {
            return;
        }
        
        if(TargetDistanceGap() <= NORMAL_ATTACK_DISTANCE)
        {
            _target.IsHit(Stats.Damage);
            AttackAnimationClientRpc();
        }
        else
        {
            _chargeTarget = _target;
            _charging = true;
            _chargeChrono = 0f;
        }
    }

    protected override void OrientateTowardsTarget()
    {
        if(_stomping)
        {
            float angle = Mathf.Atan2(_stompDirection.y, _stompDirection.x) * Mathf.Rad2Deg - 90f;
            Rigidbody.MoveRotation(angle);
        }
        else if(_charging)
        {
            Vector2 direction = _chargeTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Rigidbody.MoveRotation(angle);
        }
        else if(_target != null)
        {
            Vector2 direction = _target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Rigidbody.MoveRotation(angle);
        }
    }

    protected override void MoveTowardsTarget()
    {
        if(_target != null)
        {
            Vector2 direction = _target.transform.position - transform.position;
            direction.Normalize();
            Rigidbody.velocity = direction * Stats.Speed;
        }
    }

    void StopStomping()
    {
        _charging = false;
        _stomping = false;
        RedColor.Value = 0f;
        _attackCooldown = Stats.AttackSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(_stomping)
        {
            HCUnit unit = other.collider.GetComponent<HCUnit>();
            if(unit != null)
            {
                if(!unit.IsEnemy)
                {
                    unit.IsHit(Stats.Damage * STOMP_DAMAGE_MULTIPLIER);
                    SpawnHitEffectClientRpc((other.transform.position - transform.position).normalized);
                    StopStomping();
                }
            }
        }
    }

    [ClientRpc]
    void AttackAnimationClientRpc()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(UnitSprite.transform.DOLocalMove(Vector3.up * 0.2f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(UnitSprite.transform.DOLocalMove(Vector3.zero, 0.15f).SetEase(Ease.InQuad));
    }

    [ClientRpc]
    void SpawnSmokeTrailClientRpc()
    {
        SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(null);
        smoke.transform.localScale = Vector3.one;
        float rotationZ = transform.eulerAngles.z;
        float rotationZRadians = rotationZ * Mathf.Deg2Rad;
        Vector3 oppositeDirection = -new Vector2(-Mathf.Sin(rotationZRadians), Mathf.Cos(rotationZRadians)).normalized.ToV3();
        Vector3 position = transform.position + oppositeDirection * Stats.Radius;
        smoke.transform.position = position;
        smoke.transform.DOMove(position + oppositeDirection, 1f);
        smoke.DOFade(0f, 1f).OnComplete(()=>Destroy(smoke.gameObject));
    }

    [ClientRpc]
    void SpawnHitEffectClientRpc(Vector3 direction)
    {
        direction.Normalize();
        for (int i = -1; i <= 1; i++)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(null);
            smoke.transform.localScale = Vector3.one;
            Vector3 dir = HCUtils.RotateVector2(direction, i * 45f);
            Vector3 position = transform.position + dir;
            smoke.transform.position = position;
            smoke.transform.DOMove(position + dir, 1f);
            smoke.DOFade(0f, 1f).OnComplete(()=>Destroy(smoke.gameObject));
        }
    }
}
