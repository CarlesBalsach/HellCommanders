using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Diagnostics;

public class ShipAnimations : MonoBehaviour
{
    const int SMOKE_PARTICLES = 100;
    const int EXPLOSION_PARTICLES = 200;
    const float EXPLOSION_DELAY = 2f;
    const float DEPARTURE_PREPARATION = 2f;

    [SerializeField] SpriteRenderer ShipSprite;
    [SerializeField] SpriteRenderer HoleSprite;
    [SerializeField] SpriteRenderer DomeSprite;
    [SerializeField] SpriteRenderer SparkPrefab;

    bool _landing = false;
    bool _departing = false;
    
    void Awake()
    {
        DOTween.SetTweensCapacity(10000, 2000);

        transform.position = Vector3.zero;
        HoleSprite.gameObject.SetActive(false);
        DomeSprite.gameObject.SetActive(false);
        ShipSprite.transform.position = new Vector3(0f, HCMap.SIZE + ShipSprite.size.y / 2f, 0f);
    }

    void OnDestroy()
    {
        
    }

    void Update()
    {
        if(_landing)
        {
            SpawnLandingSparks();
        }
        if(_departing)
        {
            SpawnDepartingSparks();
        }
    }

    public void StartShipLandAnimation()
    {
        _landing = true;
        Sequence seq = DOTween.Sequence();
        seq.Append(ShipSprite.transform.DOMoveY(ShipSprite.size.y / 2f, 1f).SetEase(Ease.Linear));
        seq.AppendCallback(ShipLandedAnimation);
    }

    void ShipLandedAnimation()
    {
        _landing = false;
        HoleSprite.gameObject.SetActive(true);
        ShipSprite.gameObject.SetActive(false);

        for(int i = 0; i < 360; i += 5)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(transform);
            float angle = Random.Range(0f, 360f);
            smoke.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            smoke.transform.position = transform.position;

            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle);
            dir *= Random.Range(3f, 4f);

            smoke.transform.DOMove(transform.position + dir, 2f).SetEase(Ease.OutQuad);
            smoke.transform.DOScale(Vector3.one * Random.Range(1f, 1.2f), 2f).SetEase(Ease.InQuad);
            smoke.DOFade(0f, 2f).SetEase(Ease.InQuint).OnComplete(()=>Destroy(smoke.gameObject));
        }

        DomeSprite.gameObject.SetActive(true);
        DomeSprite.color = new Color(1f, 1f, 1f, 0f);
        DomeSprite.DOFade(1f, 4f).SetEase(Ease.InCubic);
    }

    void SpawnLandingSparks()
    {
        for(int i = 0; i < 2; i++)
        {
            SpriteRenderer spark = Instantiate(SparkPrefab, transform);
            spark.gameObject.SetActive(true);
            spark.transform.position = ShipSprite.transform.position + Vector3.down;

            float angle = Random.Range(30f, 60f) * (i % 2 == 0 ? 1 : -1);
            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle) * 1f;

            spark.transform.localScale = Vector3.one * 0.1f;

            spark.transform.DOMove(spark.transform.position + dir, 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InCubic);
            spark.DOFade(0f, 1f).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark.gameObject));
        }
    }

    public void StartShipDestroyedAnimation()
    {
        for(int i = 0; i < SMOKE_PARTICLES; i++)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(transform);
            float angle = Random.Range(0f, 360f);
            smoke.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            smoke.transform.localScale = Vector3.zero;
            
            angle = Random.Range(0f, 360f);
            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle);
            dir *= Random.Range(0f, 1f);
            smoke.transform.position = transform.position + dir;

            smoke.transform.DOMove(smoke.transform.position + Vector3.up * Random.Range(3f, 6f), 5f).SetDelay(i * 0.05f).SetLoops(10, LoopType.Restart);
            smoke.transform.DOScale(Vector3.one * Random.Range(0.7f, 1f), 5f).SetDelay(i * 0.05f).SetLoops(10, LoopType.Restart);
            smoke.DOFade(0f, 5f).SetEase(Ease.InQuad).SetDelay(i * 0.05f).SetLoops(10, LoopType.Restart);
        }

        StartCoroutine(ExplosionEffect());
    }

    IEnumerator ExplosionEffect()
    {
        yield return new WaitForSeconds(EXPLOSION_DELAY);

        for(int i = 0; i < EXPLOSION_PARTICLES; i++)
        {
            SpriteRenderer spark = Instantiate(SparkPrefab, transform);
            spark.gameObject.SetActive(true);

            float angle = Random.Range(0f, 360f);
            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle);
            dir *= Random.Range(0.2f, 1f);
            spark.transform.position = transform.position + dir;

            spark.transform.localScale *= Random.Range(1f, 2f);
            spark.color = HCUtils.GetColorFromHSVRange(0, 55);

            spark.transform.DOMove(spark.transform.position + dir * 8f, 4f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(Vector3.zero, 4.1f).SetEase(Ease.OutCubic).OnComplete(()=>Destroy(spark.gameObject));
        }

        DomeSprite.gameObject.SetActive(false);
        HoleSprite.gameObject.SetActive(true);
    }

    public void StartShipDepartingAniation()
    {
        for(int i = 0; i < EXPLOSION_PARTICLES; i++)
        {
            SpriteRenderer spark = Instantiate(SparkPrefab, transform);
            spark.gameObject.SetActive(true);
            spark.transform.position = transform.position;

            float angle = Random.Range(0f, 360f);
            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle);
            dir *= Random.Range(3f, 4f);

            float delay = DEPARTURE_PREPARATION * i / (float)EXPLOSION_PARTICLES;
            spark.transform.DOMove(dir, 1f).SetEase(Ease.OutQuad).SetDelay(delay);
            spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InCubic).SetDelay(delay).OnComplete(()=>Destroy(spark.gameObject));
        }

        ShipSprite.transform.position = transform.position + Vector3.up;
        Color c = ShipSprite.color;
        c.a = 0f;
        ShipSprite.color = c;
        ShipSprite.gameObject.SetActive(true);
        ShipSprite.DOFade(1f, 1f);

        DomeSprite.DOFade(0f, 1f);
        
        c = HoleSprite.color;
        c.a = 0f;
        HoleSprite.color = c;
        HoleSprite.gameObject.SetActive(true);
        HoleSprite.DOFade(1f, 1f);

        StartCoroutine(ShipDeparture());
    }

    IEnumerator ShipDeparture()
    {
        yield return new WaitForSeconds(DEPARTURE_PREPARATION);

        for(int i = 0; i < 360; i += 5)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(transform);
            float angle = Random.Range(0f, 360f);
            smoke.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            smoke.transform.position = transform.position;

            Vector3 dir = HCUtils.RotateVector2(Vector2.up, angle);
            dir *= Random.Range(3f, 4f);

            smoke.transform.DOMove(transform.position + dir, 2f).SetEase(Ease.OutQuad);
            smoke.transform.DOScale(Vector3.one * Random.Range(1f, 1.2f), 2f).SetEase(Ease.InQuad);
            smoke.DOFade(0f, 2f).SetEase(Ease.Linear).OnComplete(()=>Destroy(smoke.gameObject));
        }

        _departing = true;
        ShipSprite.transform.DOMove(Vector3.up * 50f, 2f).SetEase(Ease.Linear).OnComplete(()=>_departing=false);
    }

    void SpawnDepartingSparks()
    {
        //Top Sparks
        for(int i = 0; i < 2; i++)
        {
            SpriteRenderer spark = Instantiate(SparkPrefab, transform);
            spark.gameObject.SetActive(true);
            spark.transform.position = ShipSprite.transform.position + Vector3.up;

            float angle = Random.Range(30f, 60f) * (i % 2 == 0 ? 1 : -1);
            Vector3 dir = HCUtils.RotateVector2(Vector2.down, angle) * 1f;

            spark.transform.localScale = Vector3.one * 0.1f;

            spark.transform.DOMove(spark.transform.position + dir, 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InCubic);
            spark.DOFade(0f, 1f).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark.gameObject));
        }

        //Bottom Sparks
        for(int i = 0; i < 2; i++)
        {
            SpriteRenderer spark = Instantiate(SparkPrefab, transform);
            spark.gameObject.SetActive(true);
            Vector3 pos = ShipSprite.transform.position + Vector3.down;
            pos.x += Random.Range(-.5f, 0.5f);
            spark.transform.position = pos;
            spark.transform.localScale = Vector3.one * 0.1f;

            Vector3 dir = Vector3.down;
            spark.transform.DOMove(spark.transform.position + dir, 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InCubic);
            spark.DOFade(0f, 1f).SetEase(Ease.InCubic).OnComplete(()=>Destroy(spark.gameObject));
        }
    }
}
