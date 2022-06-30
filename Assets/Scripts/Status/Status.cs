using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[Serializable]
public class Status
{
    public enum statusType
    {
        ATTACK,
        DEFENCE,
        CRIPPLE,
        FREEZE,
        CHARGE,
        HASTE
    }

    public statusType type;
    public float duration;
    public float removeTime;
    public float power;
}
