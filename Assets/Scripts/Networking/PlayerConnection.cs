using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerConnection : MonoBehaviour 
{
    public bool Connected { get; set; }
    public void Awake()
    {
        NetworkSync sync = this.GetComponent<NetworkSync>();

        DontDestroyOnLoad( this.gameObject );
    }

    public void Setup()
    {
        Connected = true;

        if( StaticProperties.Instance.IsHost )
        {
            NetworkController.Instance.OnPlayerConnectedCallback += Host_PlayerConnected;
            NetworkController.Instance.OnPlayerDisconnectedCallback += Host_PlayerDisconnected;
            
            CharacterSelectionController.Instance.AddMultiplayerPlayer( "0" );
            Client_AssignID( "0" );
        }
    }

    public void Disconnect()
    {
        NetworkController.Instance.OnPlayerDisconnectedCallback -= Host_PlayerDisconnected;
        NetworkController.Instance.OnPlayerConnectedCallback -= Host_PlayerConnected;
    }

    public void Host_PlayerConnected( NetworkPlayer networkPlayer )
    {
        if( StaticProperties.Instance.IsHost )
        {
            if ( CharacterSelectionController.Instance.SlotData.Count < CharacterSelectionController.MAX_NUM_PLAYERS )
            {
                CharacterSelectionController.Instance.AddMultiplayerPlayer( networkPlayer.ToString() );

                MessageData data = new MessageData();
                data.Add<string>( networkPlayer.ToString() );

                this.GetComponent<NetworkSync>().SendToSpecificClientAsSender( networkPlayer, "Client_AssignID", data );
                Host_Refresh();
                Host_SetLives( StaticProperties.Instance.Lives );
            }
            else
            {
                MessageData data = new MessageData();
                data.Add<string>( networkPlayer.ToString() );

                this.GetComponent<NetworkSync>().SendToSpecificClientAsSender( networkPlayer, "Client_GameFull", data );
            }
       }
    }

    public void Host_TellClientsToDisconnect()
    {
        if ( StaticProperties.Instance.IsHost )
        {
            Connected = false;
            this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_Disconnect" );
        }
    }

    public void Host_PlayerDisconnected( NetworkPlayer player )
    {
        GameController.Instance.ClientDisconnected( player );
    }

    public void Host_Refresh()
    {
        if ( StaticProperties.Instance.IsHost )
        {
            MessageData data = new MessageData();
            data.Add<List<PlayerData>>( CharacterSelectionController.Instance.SlotData );

            this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_RefreshSlotData", data );
        }
    }

    public void Host_SetSlot( MessageData data )
    {
        if ( StaticProperties.Instance.IsHost )
        {
            CharacterSelectionController.Instance.UpdateSlot( data.Get<PlayerData>( 0 ) );
            Host_Refresh();
        }
    }

    public void Host_SetLives( int lives )
    {
        MessageData data = new MessageData();
        data.Add<int>( lives );

        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_SetLives", data );
    }

    public void Host_UpdatePlayer( MessageData data )
    {
        if( StaticProperties.Instance.IsHost )
        {
            string id = data.Get<string>(0);
            int lives = data.Get<int>(1);
            int kills = data.Get<int>(2);
            PlayerController.Instance.UpdatePlayer( id, lives, kills );
            Host_UpdateClientPlayers(PlayerController.Instance.GatherIDs(), PlayerController.Instance.GatherLives(), PlayerController.Instance.GatherKills());
        }
    }

    public void Host_PlayGame()
    {
        MessageData data = new MessageData();

        if( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
        {
            data.Add<Scene>(Scene.Cinematic);
        }
        else
        {
            data.Add<Scene>(Scene.Arena);
        }

        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_LoadLevel", data );
    }

    public void Host_UpdateClientPlayers(List<string> ids, List<int> lives, List<int> kills)
    {
        if( StaticProperties.Instance.IsHost )
        {
            MessageData data = new MessageData();
            data.Add<List<string>>(ids);
            data.Add<List<int>>(lives);
            data.Add<List<int>>(kills);
            this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_UpdatePlayers", data );
        }
    }
    
    public void Host_SyncPlayers()
    {
        if( StaticProperties.Instance.IsHost )
        {
            MessageData data = new MessageData();
            data.Add<List<PlayerData>>( CharacterSelectionController.Instance.SlotData );
            this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_SyncPlayers", data );
        }
    }

    public void Host_UpdateName( MessageData messageData )
    {
        if ( StaticProperties.Instance.IsHost )
        {
            CharacterSelectionController.Instance.UpdateName(messageData.Get<string>(0), messageData.Get<string>(1));
        }
    }

    public void Host_UpdateTeam(MessageData messageData)
    {
        if (StaticProperties.Instance.IsHost)
        {
            CharacterSelectionController.Instance.UpdateTeam(messageData.Get<string>(0), messageData.Get<int>(1));
        }
    }

    public void Client_AssignID( string playerID )
    {
        StaticProperties.Instance.PlayerID = playerID;
    }

    public void Client_AssignID( MessageData data )
    {
        string playerID = data.Get<string>( 0 );

        StaticProperties.Instance.PlayerID = playerID;
    }

    protected void Client_RequestRefreshFromHost()
    {
        if ( !StaticProperties.Instance.IsHost )
        {
            this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Host_Refresh" );
        }
    }

    public void Client_RefreshSlotData( MessageData data )
    {
        List<PlayerData> slotData = data.Get<List<PlayerData>>( 0 );
        CharacterSelectionController.Instance.ResetSlots();
        CharacterSelectionController.Instance.SlotData = slotData;

        for ( int i = 0; i < slotData.Count; i++ )
        {
            CharacterSelectionController.Instance.SetSlot( slotData[ i ] );
        }
    }

    public void Client_TellHostToSetCharacter( ChosenCharacter charType )
    {
        MessageData data = new MessageData();

        data.Add<PlayerData>( new PlayerData() { PlayerID = StaticProperties.Instance.PlayerID, CharacterType = charType } );

        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Host_SetSlot", data  );
    }

    public void Client_LoadLevel( MessageData data )
    {
        CharacterSelectionController.Instance.DisatatchEvents();
        GameController.Instance.LoadScene( data.Get<Scene>( 0 ) );
    }

    public void Client_GameFull()
    {
        CharacterSelectionController.Instance.BackToLobby();
    }

    public void Client_SetLives( MessageData data )
    {
        int lives = data.Get<int>( 0 );

        StaticProperties.Instance.Lives = lives;
        CharacterSelectionController.Instance.UpdateLives();
    }

    public void Client_Disconnect()
    {
        if( !StaticProperties.Instance.IsHost )
        {
            MainMenu.Instance.SetErrorMessage("Host closed the Server");
            CharacterSelectionController.Instance.BackToLobby();
        }
	}
	
	public void Client_UpdatePlayerWithHost(string id, int lives, int kills)
	{
        MessageData data = new MessageData();
        data.Add<string>(id);
        data.Add<int>(lives);
        data.Add<int>(kills);
        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Host_UpdatePlayer", data  );	
	}

    public void Client_SyncPlayers( MessageData data )
    {
        if( !StaticProperties.Instance.IsHost )
        {
            List<PlayerData> players = data.Get<List<PlayerData>>( 0 );
            GameController.Instance.CreatePlayers( players );
        }
    }

    public void Client_SendKillToHost(string id)
    {
        MessageData data = new MessageData();
        data.Add<string>(id);
        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender("Client_RecieveKill", data);	
    }

    public void Client_RecieveKill(MessageData data)
    {
        string id = data.Get<string>(0);
        PlayerController.Instance.GrantKill(id);
    }

    public void Client_UpdatePlayers( MessageData data )
    {
        List<string> ids = data.Get<List<string>>(0);
        List<int> lives = data.Get<List<int>>(1);
        List<int> kills = data.Get<List<int>>(2);

        PlayerController.Instance.UpdatePlayers(ids, lives, kills);
    }

    public void Client_UpdateNameWithHost( string name)
    {
        MessageData data = new MessageData();
        data.Add<string>( StaticProperties.Instance.PlayerID );
        data.Add<string>(name);
        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Host_UpdateName", data );	
    }

    public void Client_UpdateTeamWithHost(int colour)
    {
        MessageData data = new MessageData();
        data.Add<string>(StaticProperties.Instance.PlayerID);
        data.Add<int>(colour);
        this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender("Host_UpdateTeam", data);	
    }

    public void Update()
    {
        if( this.GetComponent<NetworkView>().isMine && GameController.Instance.CurrentGameState == GameState.CharacterSelection )
        {
            MessageData data = new MessageData();
            data.Add<string>( "Connection " + StaticProperties.Instance.PlayerID ); 
            //this.GetComponent<NetworkSync>().SendMessageToCharacterAsSender( "Client_SetName", data );	
        }
    }

    public void Client_SetName( MessageData data )
    {
        this.gameObject.name = data.Get<string>( 0 );
    }
}
