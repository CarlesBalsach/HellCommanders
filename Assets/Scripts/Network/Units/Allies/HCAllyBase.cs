using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HCAllyBase : HCUnit
{
    protected override void Start()
    {
        base.Start();
        
        if(NetworkManager.Singleton.IsServer)
        {
            Stats.HP = HCBase.Instance.MaxHP.Value;
            HP.Value = HCBase.Instance.HP.Value;
        }
    }

    protected override void Update()
    {
        if(!NetworkManager.Singleton.IsServer || HCGameManager.Instance.State.Value != HCGameManager.GameState.GAME_ACTIVE || _dead)
        {
            return;
        }

        if(!_dead && IsDead())
        {
            _dead = true;

            HCMap.Instance.RemoveUnit(this);

            Collider.enabled = false;
            DeadAnimation();
        }
    }

    protected override void FindTarget()
    {
        return;
    }

    protected override void OrientateTowardsTarget()
    {
        return;
    }

    protected override void MoveTowardsTarget()
    {
        return;
    }

    public override void IsHit(int damage, bool ignoreArmor = false)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Base being hit in a non-server environment");
            return;
        }

        base.IsHit(damage, ignoreArmor);
        HCBase.Instance.SetHP(HP.Value);
    }
}
