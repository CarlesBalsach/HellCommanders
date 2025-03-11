using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCOrbitalLaser : NetworkBehaviour
{
    const float DURATION = 10f;
    const float SPEED = 3f;
    const float ATTACK_SPEED = 0.9f;
    const float MIN_DISTANCE = 0.25f;
    const float SPARK_SPEED = 0.05f;

    [SerializeField] LineRenderer OuterLaser;
    [SerializeField] LineRenderer InnerLaser;
    [SerializeField] GameObject SparkPrefab;

    bool _active = false;
    QuipData _quip;
    bool _despawning = false;
    float _chrono = 0;
    float _attackChrono = 0f;
    float _sparkChrono = 0f;

    HCUnit _target = null;


    public void Init(QuipData quip) // Server only
    {
        SetQuipIdClientRpc(quip.Id);
    }

    [ClientRpc]
    void SetQuipIdClientRpc(int quipId)
    {
        _quip = GameData.Instance.GetQuipData(quipId);
        OuterLaser.startWidth = _quip.Damage / 100f;
        OuterLaser.endWidth = _quip.Damage / 100f;
        InnerLaser.startWidth = 0.6f * _quip.Damage / 100f;
        InnerLaser.endWidth = 0.6f * _quip.Damage / 100f;
        _active = true;
    }

    public void Update()
    {
        if(_despawning || !_active)
            return;

        if(NetworkManager.Singleton.IsServer)
        {
            if(HCGameManager.Instance.State.Value != HCGameManager.GameState.GAME_ACTIVE)
            {
                _despawning = true;
                GetComponent<NetworkObject>().Despawn(true);
            }
            else
            {
                FindClosestTarget();
                MoveToTarget();
                AttackTarget();

                _chrono += Time.deltaTime;
                _attackChrono = Mathf.Max(-ATTACK_SPEED, _attackChrono - Time.deltaTime);
                if(_chrono >= DURATION)
                {
                    _despawning = true;
                    GetComponent<NetworkObject>().Despawn(true);
                }
            }
        }

        SpawnSparks();
    }

    void FindClosestTarget()
    {
        if(_target == null || _target.IsDead())
        {
            float min_distance = HCMap.SIZE;
            foreach(HCUnit enemy in HCMap.Instance.EnemyUnits)
            {
                if(!enemy.IsDead())
                {
                    float distance = (enemy.transform.position.ToV2() - transform.position.ToV2()).magnitude;
                    if(distance < min_distance)
                    {
                        _target = enemy;
                        min_distance = distance;
                    }
                }
            }
        }
    }

    void MoveToTarget()
    {
        if(_target != null)
        {
            float step = SPEED * Time.deltaTime;
            Vector2 direction = _target.transform.position.ToV2() - transform.position.ToV2();
            float distance = direction.magnitude;
            if(distance < step)
            {
                transform.position = _target.transform.position;
            }
            else
            {
                transform.position = transform.position + direction.ToV3() * step;
            }
        }
    }

    void AttackTarget()
    {
        if(_target != null)
        {
            float distance = (_target.transform.position.ToV2() - transform.position.ToV2()).magnitude;
            if(_attackChrono <= 0f && distance <= MIN_DISTANCE)
            {
                _target.IsHit(_quip.Damage);
                _attackChrono += ATTACK_SPEED;
            }
        }
    }

    void SpawnSparks()
    {
        _sparkChrono += Time.deltaTime;
        if(_sparkChrono >= SPARK_SPEED)
        {
            _sparkChrono -= SPARK_SPEED;
            GameObject spark = Instantiate(SparkPrefab);
            spark.gameObject.SetActive(true);
            spark.transform.position = transform.position;

            float rand = Random.Range(0,2);
            if(rand == 0)
            {
                spark.GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                spark.GetComponent<SpriteRenderer>().color = Color.red;
            }

            spark.transform.localScale = spark.transform.localScale * _quip.Damage / 10f;

            float angle = Random.Range(0f, 360f);
            float distance = _quip.Damage / (10f * 2f);
            Vector3 position = transform.position + HCUtils.RotateVector2(Vector2.up, angle).ToV3();
            spark.transform.DOMove(position, 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(0f, 1f).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark));
        }
    }
}
