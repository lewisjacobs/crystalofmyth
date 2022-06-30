using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class OverlayButtonArenaLobby : OverlayButton
{
    public enum ButtonTypeLobby
    {
        BACK,
        SINGLEPLAYER,
        CREATE,
        GAME,
        REFRESH,
        CONFIRMCREATE,
        CANCELCREATE,
        KEYBOARD
    }

    public ButtonTypeLobby buttonType;
         
    void Start()
    {
        if (buttonType == ButtonTypeLobby.GAME)
        {
            buttonIndex = StaticProperties.Instance.NumberOfServers;
        }
    }         

    public override void TurnRed()
    {
        if ( ( !ArenaLobbyController.Instance.PopupActive && !ArenaLobbyController.Instance.KeyboardActive ) || popupButton )
        {
            ArenaLobbyController.Instance.CurrentButtonIndex = buttonIndex;
            ArenaLobbyController.Instance.SetButtonColours(-1);
            Highlighted = true;
            text.color = Color.red;
        }
    }

    public override void RevertColour()
    {
        if (popupButton)
        {
            Highlighted = false;
            text.color = Color.black;
        }
        else if ( !ArenaLobbyController.Instance.PopupActive && !ArenaLobbyController.Instance.KeyboardActive )
        {
            Highlighted = false;
            text.color = Color.white;
        }
    }
    
    public override void TextClicked()
    {
        Scene levelToLoad = Scene.NONE;

        if ( ( !ArenaLobbyController.Instance.PopupActive && !ArenaLobbyController.Instance.KeyboardActive ) || popupButton )
        {
            switch (buttonType)
            {
                case ButtonTypeLobby.BACK:
                    GameController.Instance.CurrentGameState = GameState.MAINMENU;
                    ArenaLobbyController.Instance.ClearHostList();
                    ArenaLobbyController.Instance.StopRefreshingHosts();
                    NetworkController.Instance.ClearHostData();
                    NetworkController.Instance.StopRefreshingHostList();
                    levelToLoad = Scene.MainMenu;
                    break;

                case ButtonTypeLobby.SINGLEPLAYER:
                    StaticProperties.Instance.HostText = "This is a Single Player Lobby";
                    StaticProperties.Instance.MultiPlayer = false;
                    StaticProperties.Instance.IsHost = true;
                    levelToLoad = Scene.CharacterSelect;
                    break;

                case ButtonTypeLobby.CREATE:
                    RevertColour();
                    ArenaLobbyController.Instance.CreatePopup();
                    break;

                case ButtonTypeLobby.CONFIRMCREATE:
                    StaticProperties.Instance.IsHost = true;
                    StaticProperties.Instance.MultiPlayer = true;
                    StaticProperties.Instance.ServerName = GameObject.Find("Server Name Input").GetComponent<InputField>().text;
                    if (StaticProperties.Instance.ServerName == "")
                    {
                        if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
                        {
                            StaticProperties.Instance.ServerName = "Crystal of Myth Adventure Server";

                        }
                        else if (StaticProperties.Instance.GameType == GameTypes.ARENA)
                        {
                            StaticProperties.Instance.ServerName = "Crystal of Myth Arena Deathmatch Server";
                        }
                        else if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
                        {
                            StaticProperties.Instance.ServerName = "Crystal of Myth Team Arena Server";
                        }
                    }
                    StaticProperties.Instance.HostText = "You are the Host of " + StaticProperties.Instance.ServerName;
                    ArenaLobbyController.Instance.ClosePopup();
                    levelToLoad = Scene.CharacterSelect;
                    break;

                case ButtonTypeLobby.CANCELCREATE:
                    ArenaLobbyController.Instance.ClosePopup();
                    break;

                case ButtonTypeLobby.GAME:
                    StaticProperties.Instance.HostText = "You are a Member of this Lobby";
                    StaticProperties.Instance.IsHost = false;
                    StaticProperties.Instance.MultiPlayer = true;
                    StaticProperties.Instance.ServerIndex = int.Parse(this.text.text.Substring(0, this.text.text.IndexOf('.')));
                    levelToLoad = Scene.CharacterSelect;
                    break;

                case ButtonTypeLobby.REFRESH:
                    ArenaLobbyController.Instance.ClearHostList();

                    if ( StaticProperties.Instance.GameType == GameTypes.ARENA )
                    {
                        NetworkController.Instance.RefreshHostList( "Crystal_Of_Myth_Arena" );
                    }
                    else if ( StaticProperties.Instance.GameType == GameTypes.ARENATEAM )
                    {
                        NetworkController.Instance.RefreshHostList( "Crystal_Of_Myth_Team_Arena" );
                    }
                    else
                    {
                        NetworkController.Instance.RefreshHostList( "Crystal_Of_Myth_Adventure" );
                    }

                    break;

                case ButtonTypeLobby.KEYBOARD:
                    ArenaLobbyController.Instance.SetButtonColours( -1 );
                    ArenaLobbyController.Instance.CreateKeyboard();
                    break;
            }
        }

        if ( levelToLoad != Scene.NONE )
        {
            ArenaLobbyController.Instance.StopRefreshingHosts();
            GameController.Instance.LoadScene( levelToLoad );
        }
    }
}
