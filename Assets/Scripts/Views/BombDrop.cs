using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BombDrop : MonoBehaviour
{
    [SerializeField] SpriteRenderer SparkSprite;
    [SerializeField] SpriteRenderer SmokeSprite;

    public event Action<Vector3> OnBombDropped;

    SpriteRenderer _bombSprite;
    Vector3 _from;
    Vector3 _to;
    float _scale;
    float _radius;
    bool _dropping = false;
    
    public void Init(Vector3 from, Vector3 to, float scale, float radius)
    {
        _bombSprite = GetComponent<SpriteRenderer>();
        _from = from;
        transform.position = from;
        _to = to;
        _scale = scale;
        transform.localScale = new Vector3(scale, 1f, 1f);
        _radius = radius;
        SetAngle();
        DropBomb();
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

    void SetAngle()
    {
        Vector3 direction = _to - _from;

        // Calculate the angle from the negative y-axis (downwards) to the direction vector
        // atan2 gives the angle in radians relative to the positive x-axis, moving counterclockwise
        float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;

        // Apply rotation around the z-axis
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void DropBomb()
    {
        transform.DOMove(_to, 1f).SetEase(Ease.Linear).OnComplete(BombDropped);
        _dropping = true;
    }

    void BombDropped()
    {
        _dropping = false;
        OnBombDropped?.Invoke(_to);
        SpawnSmokeParticles();
        Destroy(gameObject);
    }

    void SpawnSpark()
    {
        GameObject spark = Instantiate(SparkSprite.gameObject) as GameObject;
        spark.gameObject.SetActive(true);
        spark.transform.position = transform.position + (_to - _from).normalized * 0.5f;
        spark.transform.localScale = spark.transform.localScale * _scale;
        int randm11 = 1 - UnityEngine.Random.Range(0, 2) * 2;
        float angle = UnityEngine.Random.Range(90f, 140f) * randm11;
        Vector3 direction = HCUtils.RotateVector2(_to - _from, angle).normalized;

        spark.transform.DOMove(spark.transform.position + direction * _scale, 1f).SetEase(Ease.OutQuad);
        spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InQuad).OnComplete(() => Destroy(spark));
    }

    void SpawnSmokeParticles()
    {
        for (int i = 0; i < 360; i += 10)
        {
            GameObject smoke = Instantiate(SmokeSprite.gameObject) as GameObject;
            smoke.gameObject.SetActive(true);
            smoke.transform.position = transform.position;
            smoke.transform.localScale = Vector3.one * _scale;

            Vector3 direction = HCUtils.RotateVector2(Vector2.up, i + UnityEngine.Random.Range(-3f, 3f)).normalized;
            Vector3 finalPos = transform.position + direction * (_radius + _scale * 0.5f) * UnityEngine.Random.Range(0.9f, 1.1f);
            smoke.transform.DOMove(finalPos, 2f).SetEase(Ease.OutQuint);
            smoke.GetComponent<SpriteRenderer>().DOFade(0f, 2f).SetEase(Ease.OutQuad).OnComplete(() => Destroy(smoke));
        }
    }
}
