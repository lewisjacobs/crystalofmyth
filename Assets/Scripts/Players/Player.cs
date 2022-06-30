using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[Serializable]
public class Player
{
    public string PlayerID { get; set; }

    public bool IsAI { get; set; }

    public ChosenCharacter MyCharacter { get; set; }

    public string Name { get; set; }

    public int PlayerTeam { get; set; }

    public int Kills { get; set; }

    public virtual void Update()
    {

    }

    public virtual void GrantKill()
    {
    }

    public virtual void Destroy()
    {
    }

    public virtual void CreateWayPointSystem()
    {

    }
}
