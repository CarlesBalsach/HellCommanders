using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ExplosionAoE : MonoBehaviour
{
    public void Init(float radius)
    {
        transform.localScale = Vector3.zero;

        SpriteRenderer aoeSprite = GetComponent<SpriteRenderer>();
        transform.DOScale(radius * 2f, 1f).SetEase(Ease.OutQuad);
        aoeSprite.DOFade(0f, 2f).SetEase(Ease.OutQuad).OnComplete(() => Destroy(gameObject));
    }
}
