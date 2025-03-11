using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuipLeavePlannet : HCQuip
{
    protected override void ShowQuipCallPosition()
    {
        CallLine.enabled = false;
    }

    protected override void Activate()
    {
        if(OwnerClientId == HCNetworkManager.Instance.GetLocalPlayedId())
        {
            HCNetworkManager.Instance.ChangePlayerReadyStatus(true);
        }
    }
}
