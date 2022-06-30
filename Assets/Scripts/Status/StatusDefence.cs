using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StatusDefence : Status
{
    public StatusDefence(float power, float duration)
    {
        this.power = power / 100;
        this.duration = duration;
        this.removeTime = GameController.Instance.ElapsedTime + duration;
        type = statusType.DEFENCE;
    }
}