using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StatusFreeze : Status
{
    public StatusFreeze(float power, float duration)
    {
        this.power = power;
        this.duration = duration;
        this.removeTime = GameController.Instance.ElapsedTime + duration;
        type = statusType.FREEZE;
    }
}
