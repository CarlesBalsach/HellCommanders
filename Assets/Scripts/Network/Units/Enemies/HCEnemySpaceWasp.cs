using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class HCEnemySpaceWasp : HCUnit
{
    protected override void AttackTarget()
    {
        _target.IsHit(Stats.Damage);
        AttackAnimationClientRpc();
    }

    [ClientRpc]
    void AttackAnimationClientRpc()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(UnitSprite.transform.DOLocalMove(Vector3.up * 0.125f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(UnitSprite.transform.DOLocalMove(Vector3.zero, 0.15f).SetEase(Ease.InQuad));
    }
}
