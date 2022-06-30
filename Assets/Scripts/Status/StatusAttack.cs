using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StatusAttack : Status
{
    public StatusAttack(float power, float duration)
    {
        this.power = power / 100;
        this.duration = duration;
        this.removeTime = GameController.Instance.ElapsedTime + duration;
        type = statusType.ATTACK;
    }
}
