using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening.Core.Easing;
using Unity.Netcode;
using DG.Tweening;

public class ExplosiveShell : MonoBehaviour
{

    const float FLY_TIME = 1.5f;
    const float ALTITUDE = 2f;
    const float SCALE_DELTA = 1.5f;
    const float SPARK_DELTA = 1/30f;
    
    [SerializeField] float Scale;
    [SerializeField] float ExplosionRadius;
    [SerializeField] float ExplosionSecondaryRadius;

    Vector3 _start;
    Vector3 _end;
    float _scaleMin;
    float _scaleMax;
    int _damage;

    bool _flying = false;
    float _chrono = 0f;
    float _sparkChrono = 0f;

    public void Init(Vector3 targetPos, int damage)
    {
        _start = transform.position;
        _end = targetPos;
        _damage = damage;
        _flying = true;
        _scaleMin = Scale;
        _scaleMax = Scale * SCALE_DELTA;
    }

    void Update()
    {
        if(_flying)
        {
            _chrono += Time.deltaTime;
            if(_chrono >= FLY_TIME)
            {
                _flying = false;
                Explode();
            }
            else
            {
                float w = Mathf.InverseLerp(0f, FLY_TIME, _chrono);
                Vector3 pos = Vector3.Lerp(_start, _end, w);
                if(w < 2f/3f)
                {
                    float ease = EaseManager.Evaluate(DG.Tweening.Ease.OutQuad, null, w, 2f/3f, 1f, 1f);
                    pos.y += ease * ALTITUDE;
                    transform.localScale = Vector3.one * Mathf.Lerp(_scaleMin, _scaleMax, ease);
                }
                else
                {
                    float ease = EaseManager.Evaluate(DG.Tweening.Ease.InQuad, null, w - 2f/3f, 1f/3f, 1f, 1f);
                    pos.y += ALTITUDE - ease * ALTITUDE;
                    transform.localScale = Vector3.one * Mathf.Lerp(_scaleMax, _scaleMin, ease);
                }
                transform.position = pos;
                SpawnSparks();
            }
        }
    }

    void SpawnSparks()
    {
        _sparkChrono += Time.deltaTime;
        if(_sparkChrono >= SPARK_DELTA)
        {
            _sparkChrono -= SPARK_DELTA;
            HCVisualEffects.ShellSparkEffect(transform.position, transform.localScale.x / 2f);
        }
    }

    void Explode()
    {
        DealDamage();
        ExplosionAoE explosion = PrefabController.Instance.SpawnExplosionAoE(_end);
        explosion.Init(ExplosionSecondaryRadius);
        HCVisualEffects.ExplosionSmokeParticles(_end, ExplosionSecondaryRadius);
        Destroy(gameObject);
    }

    void DealDamage()
    {
        if(NetworkManager.Singleton.IsServer)
        {
            List<HCUnit> units = new List<HCUnit>(HCMap.Instance.UnitsSpawned);
            foreach(HCUnit unit in units)
            {
                if(HCUtils.InDistance2D(transform.position, unit.transform.position, ExplosionRadius))
                {
                    unit.IsHit(_damage);
                }
                else if(HCUtils.InDistance2D(transform.position, unit.transform.position, ExplosionSecondaryRadius))
                {
                    unit.IsHit(_damage / 2);
                }
            }
        }
    }
}
