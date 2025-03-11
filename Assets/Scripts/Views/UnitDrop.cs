using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class UnitDrop : MonoBehaviour
{
    [SerializeField] SpriteRenderer SparkSprite;
    [SerializeField] SpriteRenderer SmokeSprite;

    public event Action OnUnitDropped;

    SpriteRenderer _capsuleSprite;
    Vector3 _from;
    Vector3 _to;
    float _size;
    bool _dropping = false;

    public void Init(Vector3 to, float size)
    {
        _capsuleSprite = GetComponent<SpriteRenderer>();
        _to = to;
        _from = _to + Vector3.up * HCMap.SIZE;
        transform.position = _from;
        _size = size;
        transform.localScale = new Vector3(_size / 2f, _size / 2f, 1f);
        DropUnit();
    }

    void Update()
    {
        if(_dropping)
        {
            SpawnSpark();
            SpawnSpark();
            SpawnSpark();
            SpawnSpark();
        }
    }

    void DropUnit()
    {
        transform.DOMove(_to, 1f).SetEase(Ease.Linear).OnComplete(UnitDropped);
        _dropping = true;
    }

    void UnitDropped()
    {
        _dropping = false;
        OnUnitDropped?.Invoke();
        SpawnSmokeParticles();
        Destroy(gameObject);
    }

    void SpawnSpark()
    {
        GameObject spark = Instantiate(SparkSprite.gameObject) as GameObject;
        spark.gameObject.SetActive(true);
        spark.transform.position = transform.position + (_to - _from).normalized * 0.5f;
        spark.transform.localScale = spark.transform.localScale * _size / 2f;
        int randm11 = 1 - UnityEngine.Random.Range(0, 2) * 2;
        float angle = UnityEngine.Random.Range(90f, 140f) * randm11;
        Vector3 direction = HCUtils.RotateVector2(_to - _from, angle).normalized;

        spark.transform.DOMove(spark.transform.position + direction * _size, 1f).SetEase(Ease.OutQuad);
        spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InQuad).OnComplete(() => Destroy(spark));
    }

    void SpawnSmokeParticles()
    {
        for (int i = 0; i < 360; i += 10)
        {
            GameObject smoke = Instantiate(SmokeSprite.gameObject) as GameObject;
            smoke.gameObject.SetActive(true);
            smoke.transform.position = transform.position;
            smoke.transform.localScale = Vector3.one * _size / 2f;

            Vector3 direction = HCUtils.RotateVector2(Vector2.up, i + UnityEngine.Random.Range(-3f, 3f)).normalized;
            Vector3 finalPos = transform.position + direction * (_size * 1.5f) * UnityEngine.Random.Range(0.9f, 1.1f);
            smoke.transform.DOMove(finalPos, 2f).SetEase(Ease.OutCubic);
            smoke.GetComponent<SpriteRenderer>().DOFade(0f, 2f).SetEase(Ease.OutQuint).OnComplete(() => Destroy(smoke));
        }

        for (int i = 0; i < 360; i += 30)
        {
            GameObject smoke = Instantiate(SmokeSprite.gameObject) as GameObject;
            smoke.gameObject.SetActive(true);
            smoke.transform.position = transform.position;
            smoke.transform.localScale = Vector3.one * _size / 2f;

            Vector3 direction = HCUtils.RotateVector2(Vector2.up, i + UnityEngine.Random.Range(-5f, 5f)).normalized;
            Vector3 finalPos = transform.position + direction * UnityEngine.Random.Range(0.4f, 0.6f);
            smoke.transform.DOMove(finalPos, 2f).SetEase(Ease.Linear);
            smoke.GetComponent<SpriteRenderer>().DOFade(0f, 2f).SetEase(Ease.OutQuint).OnComplete(() => Destroy(smoke));
        }
    }
}
