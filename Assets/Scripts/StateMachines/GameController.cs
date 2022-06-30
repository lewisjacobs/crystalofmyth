using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PigeonCoopToolkit.TIM;

public enum LevelTypes
{
    Character,
    DungeonMaster,
    Menu
}

public enum GameState
{
    NONE,
    MAINMENUSETUP,
    MAINMENU,
    LOBBYSETUP,
    ARENALOBBY,
    ADVENTURELOBBY,
    CharacterSelectionSetup,
    CharacterSelection,
    INGAME,
    STARTGAME,
    WAITINGFORPLAYERS,
    STARTGAME_ADVENTURE,
    WAITINGFORPLAYERS_ADVENTURE,
    INGAME_ADVENTURE,
    WAITING_TO_END_GAME,
    CINEMATIC_RUNNING
}

[Serializable]
public enum Scene
{
    NONE,
    MainMenu,
    Lobby,
    CharacterSelect,
    KinectCalibration,
    Arena,
    Adventure,
    Cinematic
}

public class GameController : Singleton<GameController>
{
    public bool UseTouchControls = false;
    public MysticCharacter MysticCharacterPrefab;
    public KnightCharacter KnightCharacterPrefab;
    public BarbarianCharacter BarbarianCharacterPrefab;
    public HuntressCharacter HuntressCharacterPrefab;
    public DungeonMasterCharacter DungeonMasterCharacterPrefab;


    public MysticCharacter AIMysticCharacterPrefab;
    public KnightCharacter AIKnightCharacterPrefab;
    public BarbarianCharacter AIBarbarianCharacterPrefab;
    public HuntressCharacter AIHuntressCharacterPrefab;

    public float ElapsedTime { get; private set; }
    public float GameTime { get; private set; }
    public bool GameBegun { get; private set; }
    public PlayerConnection MyPlayerConnection { get; set; }
    public GameState CurrentGameState { get; set; }

    private Vector3[] arenaSpawnPositions = 
    {
        new Vector3(-25, 11, -54),
        new Vector3(-40, 11, -41),
        new Vector3(-32, 11, -49),
        new Vector3(-15, 10.5f, -38),
        new Vector3(-9, 11, -14),
        new Vector3(16, 11, -14),
        new Vector3(16, 11, -45)
    };

    private Vector3[] aiArenaSpawnPositions = 
    {
        new Vector3(-32, 11, 0),
        new Vector3(15, 11, -55),
        new Vector3(40, 11, -20),
        new Vector3(30, 11, 18),
        new Vector3(-14, 11, 33),
        new Vector3(8, 11, -9),
        new Vector3(-5, 11, -26)
    };

    private Vector3[] adventureSpawnPositions = 
    {
        new Vector3(110, 2, 121),
        new Vector3(111, 2, 121),
        new Vector3(112, 2, 121),
        new Vector3(113, 2, 121),
        new Vector3(114, 2, 121),
        new Vector3(115, 2, 121),
        new Vector3(116, 2, 121),
        new Vector3(117, 2, 121),
    };

    private LevelTypes GameType;

    protected bool _SyncPlayers = false;

    public GameController()
    {
        CurrentGameState = GameState.NONE;
    }

    public void Start()
    {
        KinectController.Instance.Setup();

        ElapsedTime = 0.0f;

#if  UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
#else
        UseTouchControls = true;
#endif
        NetworkController.Instance.OnNetworkDisconnectedCallback += OnNetworkDisconnectionCallback;
    }

    public void Awake()
    {
        DontDestroyOnLoad( this.gameObject );
    }

    public void QuitApplication()
    {        
        if (StaticProperties.Instance.MultiPlayer)
        {
			if (StaticProperties.Instance.IsHost)
				NetworkController.Instance.DisconnectServer();
			else
				NetworkController.Instance.Disconnect();
				
        }
        PlayerController.Instance.ClearPlayers();
        Application.Quit();
    }

    public void QuitToMenu()
    {
        if (StaticProperties.Instance.MultiPlayer)
        {
			if (StaticProperties.Instance.IsHost)
				NetworkController.Instance.DisconnectServer();
			else
                NetworkController.Instance.Disconnect();
            MyPlayerConnection.Disconnect();
        }

        PlayerController.Instance.ClearPlayers();
        CurrentGameState = GameState.MAINMENU;
        LoadScene(Scene.MainMenu);
    }

    public void Update()
    {
        ElapsedTime += Time.deltaTime;
        if ( GameBegun ) GameTime += Time.deltaTime;

        HandleGameState();

        if(CurrentGameState == GameState.INGAME || CurrentGameState == GameState.INGAME_ADVENTURE)
        {
            UIController.Instance.UpdateLives();
        }
    }

    public void EnableTouchControls()
    {
        if ( UseTouchControls )
        {
            TouchInput.SetVisible( LayerID.Main );
            TouchInput.SetActive( LayerID.Main );
        }
    }

    protected void HandleGameState()
    {
        if ( CurrentGameState == GameState.MAINMENUSETUP )
        {
            Cursor.lockState =  CursorLockMode.None;
            Cursor.visible = true;

            MainMenu.Instance.AttachControllerToMainMenu();
            CurrentGameState = GameState.MAINMENU;
        }
        else if ( CurrentGameState == GameState.LOBBYSETUP )
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ArenaLobbyController.Instance.AttachControllerToLobby();
            CurrentGameState = GameState.ARENALOBBY;
        }
        else if ( CurrentGameState == GameState.CharacterSelectionSetup )
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            CharacterSelectionController.Instance.ConnectControlerToSceneCharacterSelectScene();
            CurrentGameState = GameState.CharacterSelection;
        }
        else if ( CurrentGameState == GameState.STARTGAME )
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            BattleController.Instance.AttachControllerToBattle();
            CurrentGameState = GameState.INGAME;
            HandleCharacterSelectionController();
            SpawnPlayer();
            SpawnAIPlayers();
        }
        else if ( CurrentGameState == GameState.WAITINGFORPLAYERS && _SyncPlayers )
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            BattleController.Instance.AttachControllerToBattle();
            CurrentGameState = GameState.INGAME;
            HandleCharacterSelectionControllerClient();
            SpawnPlayer();
        }
        else if (CurrentGameState == GameState.STARTGAME_ADVENTURE)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            AdventureController.Instance.AttachControllerToAdventure();
            Debug.Log("Inside Startgame_Adventure");
            CurrentGameState = GameState.INGAME_ADVENTURE;
            HandleCharacterSelectionController();
            SpawnPlayer();
        }
        else if (CurrentGameState == GameState.WAITINGFORPLAYERS_ADVENTURE && _SyncPlayers)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            AdventureController.Instance.AttachControllerToAdventure();
            Debug.Log("Inside Waiting_for_players");
            CurrentGameState = GameState.INGAME_ADVENTURE;
            HandleCharacterSelectionControllerClient();
            SpawnPlayer();
        }
        else if( CurrentGameState == GameState.WAITING_TO_END_GAME )
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if( PlayerController.Instance.NumOfRealPlayers() <= 1 )
            {
                QuitToMenu();
            }
        }
    }

    protected void HandleCharacterSelectionController()
    {
        if ( StaticProperties.Instance.IsHost )
        {
            foreach ( PlayerData playerData in CharacterSelectionController.Instance.SlotData )
            {
                Player player = null;
                if (playerData.IsAI)
                {
                    player = new AICharacter( playerData.CharacterType ) { PlayerID = playerData.PlayerID, Lives = StaticProperties.Instance.Lives, Alive = true, MyCharacter = playerData.CharacterType, IsAI = true };
                }
                else
                {
                    switch (playerData.CharacterType)
                    {
                        case ChosenCharacter.DM:
                            {
                                player = new RealDM() { PlayerID = playerData.PlayerID, MyCharacter = ChosenCharacter.DM }; break;
                            }
                        default:
                            {
                                player = new RealCharacter() { Lives = StaticProperties.Instance.Lives, Alive = true, PlayerID = playerData.PlayerID, 
                                    MyCharacter = StaticProperties.Instance.SelectedCharacter }; break;
                            }
                    }
                }

                player.Name = playerData.Name;
                player.PlayerTeam = playerData.Team;

                player.CreateWayPointSystem();

                PlayerController.Instance.AddPlayer(player);
            }

            if ( StaticProperties.Instance.MultiPlayer )
            {
                this.MyPlayerConnection.Host_SyncPlayers();
            }
        }
    }

    protected void HandleCharacterSelectionControllerClient()
    {
        foreach ( PlayerData playerData in CharacterSelectionController.Instance.SlotData )
        {
            Player player = null;

            switch ( playerData.CharacterType )
            {
                case ChosenCharacter.DM:
                    {
                        player = new RealDM()
                        {
                            PlayerID = playerData.PlayerID,
                            Name = playerData.Name,
                            PlayerTeam = playerData.Team,
                            MyCharacter = ChosenCharacter.DM
                        };
                        break;
                    }
                default:
                    {
                        player = new RealCharacter()
                        {
                            Lives = StaticProperties.Instance.Lives,
                            Alive = true,
                            PlayerID = playerData.PlayerID,
                            Name = playerData.Name,
                            PlayerTeam = playerData.Team,
                            MyCharacter = StaticProperties.Instance.SelectedCharacter
                        };
                        break;
                    }
            }

            PlayerController.Instance.AddPlayer( player );
        }

        _SyncPlayers = false;
    }

    public void CreatePlayers( List<PlayerData> slotDataList )
    {
        CharacterSelectionController.Instance.SlotData = slotDataList;
        _SyncPlayers = true;
    }

    public void SetGameType( LevelTypes type )
    {
        GameType = type;
    }

    public void StartRound()
    {
        GameTime = 0;
        GameBegun = true;
    }

    public void PlayerDied(string id, int lives, int kills)
    {
        if (MyPlayerConnection != null)
            MyPlayerConnection.Client_UpdatePlayerWithHost(id, lives, kills);
    }

    public void PlayerKilled(string id, int lives, int kills)
    {
        if (MyPlayerConnection != null)
            MyPlayerConnection.Client_UpdatePlayerWithHost(id, lives, kills);
    }

    public Vector3 GetSpawnPoint(bool random)
    {
        int index = 0;
        if (random) index = UnityEngine.Random.Range(0, arenaSpawnPositions.Length - 1);
        else index = int.Parse(StaticProperties.Instance.PlayerID);

        if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
            return adventureSpawnPositions[index];
        return arenaSpawnPositions[index];
    }

    public Vector3 GetAISpawnPoint(int idIndex, bool random)
    {
        int index = 0;
        if (random) index = UnityEngine.Random.Range(0, aiArenaSpawnPositions.Length - 1);
        else index = idIndex;
        return aiArenaSpawnPositions[index];
    }

    protected void SpawnAIPlayers()
    {
        int index = 0;

        if(StaticProperties.Instance.IsHost)
        {
            foreach(Player p in PlayerController.Instance.Players)
            {
                if(p.IsAI)
                {
                    Vector3 position = GetAISpawnPoint(index, false);
                    index++;
                    Character aiCharacter = null;
                    if (p.MyCharacter == ChosenCharacter.MYSTIC)
                    {
                        aiCharacter = (MysticCharacter)InstanciatePrefab(AIMysticCharacterPrefab, position, Quaternion.identity);
                    }
                    else if (p.MyCharacter == ChosenCharacter.KNIGHT)
                    {
                        aiCharacter = (KnightCharacter)InstanciatePrefab(AIKnightCharacterPrefab, position, Quaternion.identity);
                    }
                    else if (p.MyCharacter == ChosenCharacter.BARBARIAN)
                    {
                        aiCharacter = (BarbarianCharacter)InstanciatePrefab(AIBarbarianCharacterPrefab, position, Quaternion.identity);
                    }
                    else if (p.MyCharacter == ChosenCharacter.HUNTRESS)
                    {
                        aiCharacter = (HuntressCharacter)InstanciatePrefab(AIHuntressCharacterPrefab, position, Quaternion.identity);
                    }
                    
                    AICharacter player = (AICharacter)PlayerController.Instance.GetPlayer(p.PlayerID);

                    if (!StaticProperties.Instance.MultiPlayer)
                        aiCharacter.Team = p.PlayerTeam;

                    aiCharacter.Team = p.PlayerTeam;
                    player.SetCharacter(aiCharacter);
                    player.PlayerTeam = p.PlayerTeam;
                }
            }
        }
    }

    protected void SpawnPlayer()
    {
        Vector3 position = GetSpawnPoint(false);

        switch ( GameType )
        {
            case LevelTypes.Character:
                {
                    Character playerOneCharacter = null;
                    if ( StaticProperties.Instance.SelectedCharacter == ChosenCharacter.MYSTIC )
                    {
                        playerOneCharacter = ( MysticCharacter ) InstanciatePrefab( MysticCharacterPrefab, position, Quaternion.identity );
                        UIController.Instance.SetupUIController( UIType.MYSTIC );
                    }
                    else if ( StaticProperties.Instance.SelectedCharacter == ChosenCharacter.KNIGHT )
                    {
                        playerOneCharacter = ( KnightCharacter ) InstanciatePrefab( KnightCharacterPrefab, position, Quaternion.identity );
                        UIController.Instance.SetupUIController( UIType.KNIGHT );
                    }
                    else if ( StaticProperties.Instance.SelectedCharacter == ChosenCharacter.BARBARIAN )
                    {
                        playerOneCharacter = ( BarbarianCharacter ) InstanciatePrefab( BarbarianCharacterPrefab, position, Quaternion.identity );
                        UIController.Instance.SetupUIController( UIType.BARBARIAN );
                    }
                    else if ( StaticProperties.Instance.SelectedCharacter == ChosenCharacter.HUNTRESS )
                    {
                        playerOneCharacter = ( HuntressCharacter ) InstanciatePrefab( HuntressCharacterPrefab, position, Quaternion.identity );
                        UIController.Instance.SetupUIController( UIType.HUNTRESS );
                    }

                    RealCharacter player = ( RealCharacter ) PlayerController.Instance.GetPlayer( StaticProperties.Instance.PlayerID );

                    if (!StaticProperties.Instance.MultiPlayer)
                        playerOneCharacter.Team = StaticProperties.Instance.PlayerTeam;

                    playerOneCharacter.Team = StaticProperties.Instance.PlayerTeam;
                    player.SetCharacter( playerOneCharacter );
                    player.PlayerTeam = StaticProperties.Instance.PlayerTeam;

                    player.PlayerCamera = GameObject.Find( "Player 1 Camera" ).GetComponent<Camera>();
                    player.PlayerCamera.name = playerOneCharacter.name + " Camera";
                    break;
                }
            case LevelTypes.DungeonMaster:
                {
                    RealDM player = ( RealDM ) PlayerController.Instance.GetPlayer( StaticProperties.Instance.PlayerID );

                    player.PlayerCamera = GameObject.Find( "Player 1 Camera" ).GetComponent<Camera>();
                    player.PlayerCamera.transform.position = new Vector3( 0, 100, -10 );
                    player.PlayerCamera.transform.eulerAngles = new Vector3( 90.0f, 0.0f, 0.0f );

                    player.DungeonMasterCharacter = ( DungeonMasterCharacter ) InstanciatePrefab( DungeonMasterCharacterPrefab,
                                                                                        player.PlayerCamera.transform.position,
                                                                                        player.PlayerCamera.transform.rotation );
                    UIController.Instance.SetupUIController( UIType.DUNGEONMASTER );
                    break;

                }
            case LevelTypes.Menu:
                {
                    UIController.Instance.SetupUIController( UIType.LOBBY );
                    break;
                }
        }
    }

    public UnityEngine.Object InstanciatePrefab( UnityEngine.Object prefab, Vector3 v3Position, Quaternion qRotation )
    {
        if ( !StaticProperties.Instance.MultiPlayer )
        {
            return Instantiate( prefab, v3Position, qRotation );
        }
        else
        {
            return NetworkController.Instance.Instanciate( prefab, v3Position, qRotation );
        }
    }

    public void DestroyObject( GameObject obj )
    {
        if ( StaticProperties.Instance.MultiPlayer )
        {
            NetworkController.Instance.DestroyObject( obj );
        }
        else
        {
            Destroy( obj );
        }
    }

    public void OnNetworkDisconnectionCallback( NetworkDisconnection info )
    {
        if ( CurrentGameState == GameState.INGAME || CurrentGameState == GameState.INGAME_ADVENTURE || CurrentGameState == GameState.STARTGAME )
        {
            MyPlayerConnection.Disconnect();
            CurrentGameState = GameState.MAINMENUSETUP;
            PlayerController.Instance.ClearPlayers();
            CharacterSelectionController.Instance.SlotData.Clear();
            GameController.Instance.LoadScene(Scene.MainMenu);

            if(!StaticProperties.Instance.IsHost && info == NetworkDisconnection.LostConnection )
            {
                MainMenu.Instance.SetErrorMessage( "Lost connection to the Host" );
            }
        }
    }

    public void ClientDisconnected( NetworkPlayer player )
    {
        if ( CurrentGameState == GameState.CharacterSelection )
        {
            CharacterSelectionController.Instance.RemovePlayer( player.ToString() );

            MessageData data = new MessageData();
            data.Add<string>( player.ToString() );

            MyPlayerConnection.Host_Refresh();
        }
        else if ( CurrentGameState == GameState.INGAME || CurrentGameState == GameState.INGAME_ADVENTURE || CurrentGameState == GameState.STARTGAME || 
            CurrentGameState == GameState.WAITING_TO_END_GAME)
        {
            CharacterSelectionController.Instance.RemovePlayer( player.ToString() );
            PlayerController.Instance.RemovePlayer( player.ToString() );
            MyPlayerConnection.Host_SyncPlayers();
        }
    }

    public void SendKill(string id)
    {
        if (StaticProperties.Instance.MultiPlayer)
            GameController.Instance.MyPlayerConnection.Client_SendKillToHost(id);
        else
            PlayerController.Instance.GrantKill(id);
    }

    public void LoadScene( Scene scene )
    {
        switch ( scene )
        {
            case Scene.MainMenu: Application.LoadLevel( "MainMenu" ); break;
            case Scene.Lobby: Application.LoadLevel( "ArenaLobby" ); break;
            case Scene.KinectCalibration: Application.LoadLevel( "KinectCalibration" ); break;
            case Scene.CharacterSelect: Application.LoadLevel( "CharacterSelection" ); break;
            case Scene.Arena: Application.LoadLevel( "Arena" ); break;
            case Scene.Adventure: Application.LoadLevel( "Adventure" ); break;
            case Scene.Cinematic: Application.LoadLevel( "Cinematics" ); break;
            default: throw new NotImplementedException( "Scene not yet implemented" );
        }
    }
}
