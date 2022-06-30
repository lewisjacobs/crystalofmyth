using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class PlayerData
{
    public string PlayerID;
    public ChosenCharacter CharacterType;
    public string Name;
    public int Team;
    public bool IsAI = false;
}
