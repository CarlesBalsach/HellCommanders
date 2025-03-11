using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCFireArea : NetworkBehaviour
{
    const float DURATION = 10f;
    const float BURN_TICK = 0.2f;
    const float SPARK_TICK = 0.2f;
    const float SPARK_DURATION = 1f;

    [SerializeField] SpriteRenderer SparkPrefab;

    bool _active = false;
    float _chrono = 0f;
    int _burnTicks = 0;
    int _sparkTicks = 0;

    NetworkVariable<float> _radius = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        _active = true;
        SparkPrefab.gameObject.SetActive(false);
    }

    public void Init(float radius) // Server
    {
        _radius.Value = radius;
    }

    private void Update()
    {
        if(!_active)
        {
            return;
        }

        _chrono += Time.deltaTime;
        SpawnParticles();

        if(IsServer)
        {
            int burnTicks = (int)(_chrono / BURN_TICK);
            if(burnTicks > _burnTicks)
            {
                _burnTicks++;
                BurnUnits();
            }
            
            if(_chrono >= DURATION)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    public void BurnUnits()
    {
        List<HCUnit> units = new List<HCUnit>();
        units.AddRange(HCMap.Instance.AllyUnits);
        units.AddRange(HCMap.Instance.EnemyUnits);

        foreach(HCUnit unit in units)
        {
            if(!unit.IsDead() && !unit.Stats.IsConstruct)
            {
                if(HCUtils.InDistance2D(transform.position, unit.transform.position, _radius.Value + unit.Stats.Radius))
                {
                    unit.Burn();
                }
            }
        }
    }

    void SpawnParticles()
    {
        int sparkTicks = (int)(_chrono / SPARK_TICK);
        if(_sparkTicks < sparkTicks)
        {
            Vector3 pos = HCUtils.GetPositionInArea(transform.position, _radius.Value);
            SpriteRenderer spark = Instantiate(SparkPrefab);
            spark.transform.position = pos;
            spark.gameObject.SetActive(true);
            spark.transform.DOMove(pos + Vector3.up, SPARK_DURATION).SetEase(Ease.Linear);
            spark.transform.DORotate(Vector3.forward * 360f * HCUtils.Random1m1(), SPARK_DURATION).SetEase(Ease.Linear);
            spark.transform.DOScale(0f, SPARK_DURATION).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark));
        }
    }
}
