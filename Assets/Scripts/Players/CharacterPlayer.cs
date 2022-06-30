using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[Serializable]
public class CharacterPlayer : Player
{
    public Character Character { get; set; }

    public bool Alive { get; set; }

    public int Lives { get; set; }

    public float RespawnTimer { get; set; }

    protected bool idle;
    protected float switchLeftCooldown = 0;
    protected float switchRightCooldown = 0;
    
    public void InitialiseStats()
    {
        Lives = StaticProperties.Instance.Lives;
        Kills = 0;
        Alive = true;
    }

    public override void Update()
    {
        base.Update();
        if (RespawnTimer > 0 && !Alive)
            RespawnTimer -= Time.deltaTime;
    }
    
    public void CharacterEventCallback(Character.CharacterEventType t, string sourcePlayerID)
    {
        if (PlayerID == StaticProperties.Instance.PlayerID || (this is AICharacter && StaticProperties.Instance.IsHost))
        {
            switch (t)
            {
                case Character.CharacterEventType.DEATH:
                    if (Alive)
                    {
                        Lives--;
                        Alive = false;
                        RespawnTimer = 5.0f;
                        GameController.Instance.PlayerDied(PlayerID, Lives, Kills);
                        GameController.Instance.SendKill(sourcePlayerID);
                    }
                    break;
            }
        }
    }
    
    public override void GrantKill()
    {
        Kills++;
        GameController.Instance.PlayerKilled(PlayerID, Lives, Kills);
    }
}