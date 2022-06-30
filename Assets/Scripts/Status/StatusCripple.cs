using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StatusCripple : Status
{
    public StatusCripple(float power, float duration)
    {
        this.power = power / 100;
        this.duration = duration;
        this.removeTime = GameController.Instance.ElapsedTime + duration;
        type = statusType.CRIPPLE;
    }
}
