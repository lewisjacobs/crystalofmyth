using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StatusHaste : Status
{
    public StatusHaste(float power, float duration)
    {
        this.power = power;
        this.duration = duration;
        this.removeTime = GameController.Instance.ElapsedTime + duration;
        type = statusType.HASTE;
    }
}
