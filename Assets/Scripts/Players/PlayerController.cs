using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : Singleton<PlayerController>
{
    public List<Player> Players {get; set;}

    public PlayerController()
    {
        Players = new List<Player>();
    }

    public void AddPlayer(Player player)
    {
        Players.Add( player );
    }

	public void Update()
    {
		foreach( Player player in Players )
        {
            if( player.PlayerID == StaticProperties.Instance.PlayerID || (player.IsAI && StaticProperties.Instance.IsHost) )
            {
                player.Update();
            }
		}
    }
    public void GrantKill(string id)
    {
        foreach (Player p in PlayerController.Instance.Players)
        {
            if (p.PlayerID == id)
            {
                p.GrantKill();
            }
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad( this.gameObject );
    }

    public List<string> GatherIDs()
    {
        List<string> idList = new List<string>();
        foreach (Player p in Players)
        {
            idList.Add(p.PlayerID);
        }
        return idList;
    }

    public List<int> GatherLives()
    {
        List<int> livesList = new List<int>();
        foreach (Player p in Players)
        {
            if (p is CharacterPlayer)
            {
                CharacterPlayer cp = (CharacterPlayer) p;
                livesList.Add(cp.Lives);
            }
            else
            {
                livesList.Add( 20 );
            }
        }
        return livesList;
    }

    public List<int> GatherKills()
    {
        List<int> killsList = new List<int>();
        foreach (Player p in Players)
        {
            killsList.Add(p.Kills);
        }
        return killsList;
    }

    public void KillPlayer( string id )
    {
        foreach( Player player in Players )
        {
            if (player.PlayerID == id && player is CharacterPlayer)
            {
                CharacterPlayer cp = (CharacterPlayer) player;
                cp.Lives--;
                cp.Alive = false;
            }
        }
    }

    public void UpdatePlayers(List<string> ids, List<int> lives, List<int> kills)
    {
        for(int i = 0; i < ids.Count; i++)
        {
            UpdatePlayer( ids[ i ], lives[ i ], kills[ i ] );  
        }
    }

    public void UpdatePlayer(string id, int lives, int kills)
    {
        {
            foreach (Player lPlayer in Players)
            {
                if (id == lPlayer.PlayerID)
                {
                    if (lPlayer is CharacterPlayer)
                    {
                        CharacterPlayer cp = (CharacterPlayer)lPlayer;
                        cp.Lives = lives;
                    }
                    lPlayer.PlayerID = id;
                    lPlayer.Kills = kills;
                    break;
                }
            }
        }
    }

    public void ClearPlayers()
    {
        foreach( Player player in Players )
        {
            player.Destroy();
        }

        Players.Clear();
    }

    public void RemovePlayer( string id )
    {
        Player toRemove = null;

        foreach( Player player in Players )
        {
            if( player.PlayerID == id )
            {
                toRemove = player;
                break;
            }
        }

        if( toRemove != null )
        {
            Players.Remove( toRemove );
        }
    }

    public Player GetPlayer( string playerID )
    {
        foreach( Player player in Players )
        {
            if( player.PlayerID == playerID )
            {
                return player;
            }
        }

        return null;
    }

    public Player GetPlayer(Character character)
    {
        foreach(Player player in Players)
        {
            if(!(player is DMPlayer))
            {
                CharacterPlayer cp = (CharacterPlayer) player;
                if(cp.Character == character)
                {
                    return player;
                }
            }
        }
        return null;
    }

    public int NumOfRealPlayers()
    {
        int counter = 0;

        foreach( Player player in Players )
        {
            if( player is RealCharacter || player is DMPlayer )
            {
                counter++;
            }
        }

        return counter;
    }
}
