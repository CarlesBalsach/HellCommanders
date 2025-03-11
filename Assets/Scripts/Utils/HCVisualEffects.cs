using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HCVisualEffects
{
    public static void BuildingExplosionEffect(Vector3 position, float radius, int sparks, int smokes)
    {
        for (int i = 0; i < sparks; i++)
        {
            Vector3 pos = HCUtils.GetPositionInArea(position, radius);
            SpriteRenderer spark = PrefabController.Instance.SpawnSpark(pos, radius / 3f);
            spark.color = Color.HSVToRGB(Random.Range(0, 55) / 360f, 1f, 1f);

            Vector3 direction = pos - position;
            Vector3 finalPos = pos + direction * 4f;
            spark.transform.DOMove(finalPos, 1f).SetEase(Ease.OutQuad);
            spark.transform.DOScale(0f, 1f).SetEase(Ease.InQuad).OnComplete(()=>GameObject.Destroy(spark.gameObject));
        }

        for (int i = 0; i < smokes; i++)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(null);
            Vector3 pos = HCUtils.GetPositionInArea(position, radius);
            smoke.transform.position = pos;
            smoke.transform.localScale = Vector3.one * radius;

            smoke.transform.DOMove(pos + Vector3.up * Random.Range(1f, 2f), 2f).SetEase(Ease.Linear);
            smoke.DOFade(0f, 2f).SetEase(Ease.InQuad).OnComplete(()=>GameObject.Destroy(smoke.gameObject));
        }
    }

    public static void ShellSparkEffect(Vector3 position, float radius)
    {
        Vector3 pos = HCUtils.GetPositionInArea(position, radius);
        SpriteRenderer spark = PrefabController.Instance.SpawnSpark(pos, radius / 4f);
        spark.color = HCColor.GetSparkColor();
        spark.transform.DOScale(0f, 1f).SetEase(Ease.InQuad).OnComplete(()=>GameObject.Destroy(spark.gameObject));
    }

    public static void ExplosionSmokeParticles(Vector3 position, float radius)
    {
        for (int i = 0; i < 360; i+=20)
        {
            SpriteRenderer smoke = PrefabController.Instance.SpawnSmokeParticle(null);
            smoke.transform.position = position;
            smoke.transform.localScale = Vector3.one * radius * 0.4f;

            Vector3 pos = position.ToV2() + HCUtils.RotateVector2(Vector2.up, i + Random.Range(-5, 6)) * radius * Random.Range(0.9f, 1.1f);
            smoke.transform.DOMove(pos, 2f).SetEase(Ease.OutQuint);
            smoke.DOFade(0f, 2f).SetEase(Ease.OutQuad).OnComplete(()=>GameObject.Destroy(smoke.gameObject));
        }
    }
}
