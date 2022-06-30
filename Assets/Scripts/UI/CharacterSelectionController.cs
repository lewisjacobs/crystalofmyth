using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class CharacterSelectionController : Singleton<CharacterSelectionController>
{
    public const int PLAYER_NOT_IN_GAME = -1;
    public const int MAX_NUM_PLAYERS = 8;

    public Sprite MysticPortrait;
    public Sprite KnightPortrait;
    public Sprite BarbarianPortrait;
    public Sprite HuntressPortrait;
    public Sprite OverlordPortrait;
    public Sprite DefaultPortrait;

    public Sprite WhiteFrame;
    public Sprite BlueFrame;
    public Sprite RedFrame;
    public Sprite YellowFrame;
    public Sprite GreenFrame;

    public float scaleFactor;
    public Image CurrentPortrait { get; set; }
    public Image CurrentFrame { get; set; }
    public bool KeyboardActive { get; private set; }

    private Canvas canvas;
    private Canvas keyboardCanvas;
    private Text hostText;
    private Text livesText;
    private Text numReadyText;
    private Slider livesSlider;

    public InputField NameField { get; private set; }

    public List<PlayerData> SlotData { get; set; }

    public PlayerConnection PlayerConnectionPrefab;
    public PlayerConnection OurConnection { get; private set; }

    protected bool _WaitingToDisconnect = false;

    public int CurrentButtonIndex { get; set; }
    private float moveCursorCooldown;
    private OverlayButtonCharSelect[] lobbyButtons;
    private KeyboardButton[] keyboardButtons;
    private int numberOfKeyboardKeys = 29;

    private float connectionTimer;
    public bool WaitingToConnect { get; private set; }

    public Text CharacterNameString { get; set; }

    private int numTotalPlayers;
    private int numReadyPlayers;

    public bool PopupActive { get; private set; }
    private Text errorTextField;
    private Canvas errorCanvas;
    private string errorMessage = string.Empty;

    private int aiIncrementer;

    public int CurrentAITeam { get; set; }

    private ChosenCharacter CurrentAIType { get; set; }

    private Text aiTypeText;

    public bool Readied { get; set; }

    public void ConnectControlerToSceneCharacterSelectScene()
    {
        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.MYSTIC;

        SlotData = new List<PlayerData>();
        Readied = false;

        aiIncrementer = 0;
        numTotalPlayers = 0;
        numReadyPlayers = 0;

        CurrentAIType = ChosenCharacter.MYSTIC;
        if(StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
        {
            CurrentAITeam = TeamColours.BLUE;
        }
        else
        {
            CurrentAITeam = TeamColours.NOTEAM;
        }

        GUISetup();

        if (StaticProperties.Instance.MultiPlayer)
        {
            ServerSetup();
            MultiplayerGUISetup();
        }
        else
        {
            SinglePlayerStartup();
        }
    }

    protected void GUISetup()
    {
        MysticPortrait = Resources.Load<Sprite>("Textures/mysticportrait");
        KnightPortrait = Resources.Load<Sprite>("Textures/knightportrait");
        BarbarianPortrait = Resources.Load<Sprite>("Textures/barbarianportrait");
        HuntressPortrait = Resources.Load<Sprite>("Textures/huntressportrait");
        OverlordPortrait = Resources.Load<Sprite>("Textures/overlordportrait"); 
        DefaultPortrait = Resources.Load<Sprite>("Textures/unselectedportrait");

        WhiteFrame = Resources.Load<Sprite>("Textures/whiteframe");
        BlueFrame = Resources.Load<Sprite>("Textures/blueframe");
        RedFrame = Resources.Load<Sprite>("Textures/redframe");
        YellowFrame = Resources.Load<Sprite>("Textures/yellowframe");
        GreenFrame = Resources.Load<Sprite>("Textures/greenframe");

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        keyboardCanvas = GameObject.Find("Keyboard Canvas").GetComponent<Canvas>();
        CharacterNameString = GameObject.Find("Input Text").GetComponent<Text>();
        errorCanvas = GameObject.Find("Error Canvas").GetComponent<Canvas>();
        errorTextField = errorCanvas.transform.Find("Error Text").gameObject.GetComponent<Text>();
        aiTypeText = GameObject.Find("AI Type Label").GetComponent<Text>();

        List<OverlayButtonCharSelect> temp = new List<OverlayButtonCharSelect>();

        temp.Add(GameObject.Find("Back Button").GetComponent<OverlayButtonCharSelect>());
        temp.Add(GameObject.Find("Ready Button").GetComponent<OverlayButtonCharSelect>());
        temp.Add(GameObject.Find("Keyboard Button").GetComponent<OverlayButtonCharSelect>());

        if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
        {
            temp.Add(GameObject.Find("Team Button").GetComponent<OverlayButtonCharSelect>());
            GameObject.Find("Team Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/blueteam");
            GameObject.Find("AI Team Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/blueteam");
        }
        else
        {
            DeactivateButton("Team Button");
            GameObject.Find("Team Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/noteam");
            GameObject.Find("AI Team Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Textures/noteam");
            GameObject.Find("Team Label").GetComponent<Text>().color = Color.gray;
        }

        if (!StaticProperties.Instance.IsHost)
        {
            if (!StaticProperties.Instance.IsHost)
                GameObject.Find("Ready Button").GetComponent<Text>().text = "Ready for battle!";
            else if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
                GameObject.Find("Num Lives Label").GetComponent<Text>().color = Color.gray;
            GameObject.Find("Lives Label").GetComponent<Text>().color = Color.gray;
            DeactivateButton("Lives Plus");
            DeactivateButton("Lives Minus");
        }
        else
        {
            temp.Add(GameObject.Find("Lives Minus").GetComponent<OverlayButtonCharSelect>());
            temp.Add(GameObject.Find("Lives Plus").GetComponent<OverlayButtonCharSelect>());
        }

        temp.Add(GameObject.Find("Mystic Button").GetComponent<OverlayButtonCharSelect>());
        temp.Add(GameObject.Find("Knight Button").GetComponent<OverlayButtonCharSelect>());
        temp.Add(GameObject.Find("Huntress Button").GetComponent<OverlayButtonCharSelect>());
        temp.Add(GameObject.Find("Barbarian Button").GetComponent<OverlayButtonCharSelect>());

        if (!KinectController.Instance.KinectInitialized || !StaticProperties.Instance.MultiPlayer)
        {
            DeactivateButton("Overlord Button");
        }
        else
        {
            temp.Add(GameObject.Find("Overlord Button").GetComponent<OverlayButtonCharSelect>());
        }

        if (!StaticProperties.Instance.IsHost || StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
        {
            DeactivateButton("AI Type");
            DeactivateButton("AI Team");
            GameObject.Find("AI Type Label").GetComponent<Text>().color = Color.grey;
            DeactivateButton("Add AI Button");
            DeactivateButton("Remove AI Button");
        }
        else
        {
            temp.Add(GameObject.Find("AI Type").GetComponent<OverlayButtonCharSelect>());
            if (StaticProperties.Instance.GameType == GameTypes.ARENA)
                DeactivateButton("AI Team");
            else
                temp.Add(GameObject.Find("AI Team").GetComponent<OverlayButtonCharSelect>());
            temp.Add(GameObject.Find("Add AI Button").GetComponent<OverlayButtonCharSelect>());
            temp.Add(GameObject.Find("Remove AI Button").GetComponent<OverlayButtonCharSelect>());
        }
        temp.Add(GameObject.Find("Confirm Button").GetComponent<OverlayButtonCharSelect>());

        lobbyButtons = new OverlayButtonCharSelect[temp.Count];
        for (int i = 0; i < temp.Count; i++)
        {
            lobbyButtons[i] = temp[i];
            lobbyButtons[i].buttonIndex = i;
        }

        keyboardButtons = new KeyboardButton[numberOfKeyboardKeys];

        for (int i = 1; i <= numberOfKeyboardKeys; i++)
        {
            keyboardButtons[i - 1] = GameObject.Find("Key " + i).GetComponent<KeyboardButton>();
            keyboardButtons[i - 1].buttonIndex = i - 1;
            keyboardButtons[i - 1].SceneType = KeyboardButton.SceneTypes.LOBBY;
        }

        Vector3 t = keyboardCanvas.transform.position;
        t.z = -1;
        keyboardCanvas.transform.position = t;

        CloseKeyboard();

        hostText = GameObject.Find("Host Text").GetComponent<Text>();
        livesSlider = GameObject.Find("Lives Slider").GetComponent<Slider>();
        StaticProperties.Instance.Lives = (int)livesSlider.value;
        livesText = GameObject.Find("Num Lives Label").GetComponent<Text>();
        numReadyText = GameObject.Find("Num Ready").GetComponent<Text>();
        if (StaticProperties.Instance.IsHost)
        {
            numReadyText.text = "0 / 0";
        }
        else
        {
            numReadyText.gameObject.SetActive(false);
        }

        if (!StaticProperties.Instance.IsHost)
        {
            livesSlider.gameObject.SetActive(false);
        }

        hostText.text = StaticProperties.Instance.HostText;

        float wFactor = ((100f / 1920f) * Screen.width) / 100f;
        float hFactor = ((100f / 1080f) * Screen.height) / 100f;
        if (wFactor < hFactor) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        canvas.scaleFactor = scaleFactor;
        keyboardCanvas.scaleFactor = scaleFactor;
        errorCanvas.scaleFactor = scaleFactor;

        CurrentButtonIndex = 0;
        if (StaticProperties.Instance.GameType == GameTypes.ARENA)
            StaticProperties.Instance.PlayerTeam = TeamColours.NOTEAM;
        else
            StaticProperties.Instance.PlayerTeam = TeamColours.BLUE;

        NameField = GameObject.Find("Name Input").GetComponent<InputField>();

        if (StaticProperties.Instance.MultiPlayer)
        {
            NameField.onEndEdit.AddListener((value) => NameEdit(value));
            LockInterface();
        }
        else WaitingToConnect = false;
        
        ClosePopup();
    }

    protected void MultiplayerGUISetup()
    {
        PlayerConnectionPrefab = Resources.Load<PlayerConnection>("Prefabs/PlayerLobbyConnection");
    }

    public void Update()
    {
        if (GameController.Instance.CurrentGameState == GameState.CharacterSelection)
        {
            if (errorMessage != string.Empty)
                ShowErrorMessage();
            if (StaticProperties.Instance.IsHost)
            {
                numReadyPlayers = 0;
                foreach (PlayerData p in SlotData)
                {
                    if (p.Name != string.Empty && p.Name != null && p.PlayerID != "0")
                    {
                        numReadyPlayers++;
                    }
                    numReadyText.text = numReadyPlayers + " / " + numTotalPlayers;
                }
            }
            if (WaitingToConnect && connectionTimer <= 0)
            {
                MainMenu.Instance.SetErrorMessage("Could not connect to Host");
                NetworkController.Instance.Disconnect();
                BackToLobby();
            }
            else if (connectionTimer >= 0) connectionTimer -= Time.deltaTime;

            if (_WaitingToDisconnect)
            {
                if (SlotData.Count == 1)
                {
                    OurConnection.Disconnect();
                    NetworkController.Instance.DestroyObject(OurConnection.gameObject);
                    OurConnection = null;

                    NetworkController.Instance.ServerInitializedCallback -= ServerCreatedCallback;
                    NetworkController.Instance.DisconnectServer();

                    _WaitingToDisconnect = false;
                    ArenaLobbyController.Instance.ClearHostList();
                    GameController.Instance.LoadScene( Scene.MainMenu );
                }
            }
            if (moveCursorCooldown <= 0 && !WaitingToConnect)
            {
                if (KeyboardActive)
                {
                    if (Input.GetButton("Submit") && keyboardButtons[CurrentButtonIndex].Highlighted)
                    {
                        moveCursorCooldown = 0.1f;
                        keyboardButtons[CurrentButtonIndex].TextClicked();
                    }
                    if ((Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0))
                    {
                        moveCursorCooldown = 0.1f;
                        if (keyboardButtons[CurrentButtonIndex].Highlighted)
                        {
                            CurrentButtonIndex++;
                        }
                        if (CurrentButtonIndex > numberOfKeyboardKeys - 1)
                        {
                            CurrentButtonIndex = 0;
                        }
                        SetKeyboardButtonColours(CurrentButtonIndex);
                    }
                    else if ((Input.GetAxis("VerticalArrow") > 0 || Input.GetAxis("HorizontalArrow") < 0))
                    {
                        moveCursorCooldown = 0.1f;
                        if (keyboardButtons[CurrentButtonIndex].Highlighted)
                        {
                            CurrentButtonIndex--;
                        }
                        if (CurrentButtonIndex < 0)
                        {
                            CurrentButtonIndex = numberOfKeyboardKeys - 1;
                        }
                        SetKeyboardButtonColours(CurrentButtonIndex);
                    }
                }
                else if (PopupActive)
                {
                    if (Input.GetButton("Submit") && lobbyButtons[CurrentButtonIndex].Highlighted)
                    {
                        lobbyButtons[CurrentButtonIndex].TextClicked();
                        SetButtonColours(CurrentButtonIndex);
                        moveCursorCooldown = 0.2f;
                    }
                    if (Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0)
                    {
                        moveCursorCooldown = 0.2f;
                        SetButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetAxis("VerticalArrow") > 0 || Input.GetAxis("HorizontalArrow") < 0)
                    {
                        moveCursorCooldown = 0.2f;
                        SetButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetButton("Back"))
                    {
                        ClosePopup();
                    }
                }
                else
                {
                    if (Input.GetButton("Submit") && lobbyButtons[CurrentButtonIndex].Highlighted)
                    {
                        moveCursorCooldown = 0.2f;
                        lobbyButtons[CurrentButtonIndex].TextClicked();
                    }
                    if (Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0)
                    {
                        if (lobbyButtons[CurrentButtonIndex].Highlighted)
                        {
                            if (CurrentButtonIndex < lobbyButtons.Length - 2)
                            {
                                CurrentButtonIndex++;
                            }
                        }
                        moveCursorCooldown = 0.2f;
                        SetButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetAxis("VerticalArrow") > 0 || Input.GetAxis("HorizontalArrow") < 0)
                    {
                        if (lobbyButtons[CurrentButtonIndex].Highlighted)
                        {
                            if (CurrentButtonIndex > 0)
                            {
                                CurrentButtonIndex--;
                            }
                        }
                        moveCursorCooldown = 0.2f;
                        SetButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetButton("Back"))
                    {
                        lobbyButtons[0].TextClicked();
                    }
                }
            }
            else moveCursorCooldown -= Time.deltaTime;
        }
    }

    protected void SinglePlayerStartup()
    {
        CharacterSelectionController.Instance.AddSinglePlayer();
    }

    protected void ServerSetup()
    {
        if (StaticProperties.Instance.IsHost)
        {
            CreateServer();
        }
        else
        {
            JoinServer(StaticProperties.Instance.ServerIndex);
        }
    }

    protected void JoinServer(int index)
    {
        //lock down interface and put a wait cursor
        NetworkController.Instance.ConnectedToServerCallback += JoinServerCallback;
        NetworkController.Instance.OnFailedToConnectCallback += FailedToConnectToServerCallback;
        CharacterSelectionController.Instance.LockInterface();
        NetworkController.Instance.JoinServerIndex(index);
    }

    protected void LockInterface()
    {
        connectionTimer = 10.0f;
        WaitingToConnect = true;
        NameField.enabled = false;
    }

    protected void UnlockInterface()
    {
        WaitingToConnect = false;
        NameField.enabled = true;
    }

    protected void CreateServer()
    {
        //lock interface 
        NetworkController.Instance.ServerInitializedCallback += ServerCreatedCallback;
        NetworkController.Instance.OnFailedToConnectCallback += FailedToConnectToServerCallback;

        if( StaticProperties.Instance.GameType == GameTypes.ARENA )
        {
            NetworkController.Instance.StartServer( "Crystal_Of_Myth_Arena", StaticProperties.Instance.ServerName );

        }
        else if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
        {
            NetworkController.Instance.StartServer( "Crystal_Of_Myth_Adventure", StaticProperties.Instance.ServerName );
        }
        else
        {
            NetworkController.Instance.StartServer("Crystal_Of_Myth_Team_Arena", StaticProperties.Instance.ServerName);
        }

    }

    public void ServerCreatedCallback()
    {
        OurConnection = (PlayerConnection)GameController.Instance.InstanciatePrefab(PlayerConnectionPrefab, Vector3.zero, Quaternion.identity);
        GameController.Instance.MyPlayerConnection = OurConnection;
        OurConnection.Setup();
        UnlockInterface();
        SetNumLives();

        NetworkController.Instance.ServerInitializedCallback -= ServerCreatedCallback;
        NetworkController.Instance.OnFailedToConnectCallback -= FailedToConnectToServerCallback;
    }

    public void JoinServerCallback()
    {
        OurConnection = (PlayerConnection)GameController.Instance.InstanciatePrefab(PlayerConnectionPrefab, Vector3.zero, Quaternion.identity);
        GameController.Instance.MyPlayerConnection = OurConnection;
        OurConnection.Setup();
        UnlockInterface();
        NetworkController.Instance.ConnectedToServerCallback -= JoinServerCallback;
        NetworkController.Instance.OnFailedToConnectCallback -= FailedToConnectToServerCallback;
    }

    public void FailedToConnectToServerCallback( NetworkConnectionError error )
    {
        NetworkController.Instance.ServerInitializedCallback -= ServerCreatedCallback;
        NetworkController.Instance.ConnectedToServerCallback -= JoinServerCallback;
        NetworkController.Instance.OnFailedToConnectCallback -= FailedToConnectToServerCallback;

        MainMenu.Instance.SetErrorMessage( "Could not connect to Host" );
        NetworkController.Instance.Disconnect();
        BackToLobby();
    }

    public void AddMultiplayerPlayer(string playerID)
    {
        PlayerData playerData = new PlayerData() { PlayerID = playerID, CharacterType = ChosenCharacter.MYSTIC, Team = StaticProperties.Instance.PlayerTeam };
        SlotData.Add(playerData);
        SetSlot(playerData);
        if (playerID != "0")
            numTotalPlayers++;
        numReadyText.text = numReadyPlayers + " / " + numTotalPlayers;
    }

    public void AddAIPlayer()
    {
        if (SlotData.Count < MAX_NUM_PLAYERS)
        {
            string id = "10" + aiIncrementer;
            PlayerData playerData = new PlayerData() { PlayerID = id, CharacterType = CurrentAIType, Team = CurrentAITeam, 
                IsAI = true, Name = "AI "+aiIncrementer};
            numTotalPlayers++;
            numReadyText.text = numReadyPlayers + " / " + numTotalPlayers;
            SlotData.Add(playerData);
            SetSlot(playerData);
            aiIncrementer++;
        }
    }

    public void RemoveAllAI()
    {
        List<PlayerData> aiPlayersToRemove = new List<PlayerData>();
        foreach (PlayerData pd in SlotData)
        {
            if (pd.IsAI)
            {
                aiPlayersToRemove.Add(pd);
            }
        }
        foreach(PlayerData pd in aiPlayersToRemove)
        {
            SlotData.Remove(pd);
        }
        aiPlayersToRemove.Clear();
    }

    public void RemoveAIPlayer()
    {
        foreach (PlayerData pd in SlotData)
        {
            if (pd.IsAI)
            {
                RemovePlayer(pd.PlayerID);
                break;
            }
        }
    }

    public void AddSinglePlayer()
    {
        StaticProperties.Instance.PlayerID = SlotData.Count.ToString();
        PlayerData playerData = new PlayerData() { PlayerID = SlotData.Count.ToString(), CharacterType = ChosenCharacter.MYSTIC, Team = StaticProperties.Instance.PlayerTeam };
        SlotData.Add(playerData);
        SetSlot(playerData);
    }

    public void RemovePlayer(string playerID)
    {
        int playerIndex = FindPlayerIndex(playerID);

        if (playerIndex != PLAYER_NOT_IN_GAME)
        {
            SlotData.RemoveAt(playerIndex);
            numTotalPlayers--;
            if (numReadyText != null)
                numReadyText.text = numReadyPlayers + " / " + numTotalPlayers;
        }
    }

    public void RefreshSlotData()
    {
        ResetSlots();
        foreach (PlayerData pd in SlotData)
        {
            UpdateSlot(pd);
            SetSlot(pd);
        }
    }

    public void SetSlot(PlayerData playerData)
    {
        int playerIndex = FindPlayerIndex(playerData.PlayerID);
        string s = "Character Portrait " + (playerIndex + 1);
        string frame = "Character Frame " + (playerIndex + 1);
        int colour = playerData.Team;

        CurrentPortrait = GameObject.Find(s).GetComponent<Image>();
        CurrentFrame = GameObject.Find(frame).GetComponent<Image>();

        switch (playerData.CharacterType)
        {
            //Lewis, other character here
            case ChosenCharacter.MYSTIC: CurrentPortrait.sprite = MysticPortrait; break;
            case ChosenCharacter.KNIGHT: CurrentPortrait.sprite = KnightPortrait; break;
            case ChosenCharacter.BARBARIAN: CurrentPortrait.sprite = BarbarianPortrait; break;
            case ChosenCharacter.HUNTRESS: CurrentPortrait.sprite = HuntressPortrait; break;
            case ChosenCharacter.DM: CurrentPortrait.sprite = OverlordPortrait; break;
            default: CurrentPortrait.sprite = DefaultPortrait; break;
        }

        switch(colour)
        {
            case TeamColours.NOTEAM: CurrentFrame.sprite = WhiteFrame; break;
            case TeamColours.BLUE: CurrentFrame.sprite = BlueFrame; break;
            case TeamColours.RED: CurrentFrame.sprite = RedFrame; break;
            case TeamColours.YELLOW: CurrentFrame.sprite = YellowFrame; break;
            case TeamColours.GREEN: CurrentFrame.sprite = GreenFrame; break;
            default: CurrentFrame.sprite = WhiteFrame; break;
        }
    }

    public void SetSlot(int index, ChosenCharacter character)
    {
        string s = "Character Portrait " + (index + 1);
        string frame = "Character Frame " + (index + 1);

        CurrentPortrait = GameObject.Find(s).GetComponent<Image>();
        CurrentFrame = GameObject.Find(frame).GetComponent<Image>();
        NameField = GameObject.Find("Name Input").GetComponent<InputField>();

        switch (character)
        {
            case ChosenCharacter.MYSTIC: CurrentPortrait.sprite = MysticPortrait; break;
            case ChosenCharacter.KNIGHT: CurrentPortrait.sprite = KnightPortrait; break;
            case ChosenCharacter.BARBARIAN: CurrentPortrait.sprite = BarbarianPortrait; break;
            case ChosenCharacter.HUNTRESS: CurrentPortrait.sprite = HuntressPortrait; break;
            case ChosenCharacter.DM: CurrentPortrait.sprite = OverlordPortrait; break;
            default: CurrentPortrait.sprite = DefaultPortrait; break;
        }

        CurrentFrame.sprite = WhiteFrame;
    }

    public void ResetSlots()
    {
        for (int i = 0; i < 8; i++)
        {
            SetSlot(i, ChosenCharacter.Default);
        }
    }

    protected int FindPlayerIndex(string playerID)
    {
        for (int i = 0; i < SlotData.Count; i++)
        {
            if (SlotData[i].PlayerID == playerID)
            {
                return i;
            }
        }

        return PLAYER_NOT_IN_GAME;
    }

    public void UpdateSlot(PlayerData playerData)
    {
        int playerIndex = FindPlayerIndex(playerData.PlayerID);
        SlotData[playerIndex].CharacterType = playerData.CharacterType;
    }

    public void SetHost(bool b, string name)
    {
        if (b) hostText.text = "You are the Host of " + name;
        else hostText.text = "You have joined " + name;
    }

    public void SetNumLives()
    {
        livesText.text = "" + livesSlider.value;
        StaticProperties.Instance.Lives = (int)livesSlider.value;
        if (StaticProperties.Instance.MultiPlayer)
            OurConnection.Host_SetLives(StaticProperties.Instance.Lives);
    }

    public void UpdateLivesSlider(bool up)
    {
        if (up)
        {
            livesSlider.value++;
        }
        else
        {
            livesSlider.value--;
        }
        SetNumLives();

    }

    public void UpdateLives()
    {
        GameObject.Find("Num Lives Label").GetComponent<Text>().text = "" + StaticProperties.Instance.Lives;
    }

    public void BackToLobby()
    {
        if (StaticProperties.Instance.MultiPlayer)
        {
            if (StaticProperties.Instance.IsHost)
            {
                RemoveAllAI();
                _WaitingToDisconnect = true;
                OurConnection.Host_TellClientsToDisconnect();
            }
            else
            {
                NetworkController.Instance.ConnectedToServerCallback -= JoinServerCallback;
                NetworkController.Instance.OnFailedToConnectCallback += FailedToConnectToServerCallback;
                NetworkController.Instance.Disconnect();
                ArenaLobbyController.Instance.ClearHostList();
                GameController.Instance.LoadScene( Scene.MainMenu );
            }

            StaticProperties.Instance.MultiPlayer = false;
        }
        else
        {
            GameController.Instance.LoadScene(Scene.MainMenu);
        }
        
        if (NameField != null)
            NameField.onEndEdit.RemoveAllListeners();
    }

    public int CountArenaTeams()
    {
        bool[] teams = {false, false, false, false};
        foreach(PlayerData pd in SlotData)
        {
            switch(pd.Team)
            {
                case TeamColours.BLUE: teams[0] = true; break;
                case TeamColours.RED: teams[1] = true; break;
                case TeamColours.GREEN: teams[2] = true; break;
                case TeamColours.YELLOW: teams[3] = true; break;
                default: break;
            }
        }
        int numTeams = 0;
        foreach(bool b in teams)
        {
            if (b) numTeams++;
        }
        return numTeams;
    }

    public void PlayGame()
    {
        if (NameField.text == string.Empty)
        {
            CharacterSelectionController.Instance.SetErrorMessage("Please enter a Character name");
            return;
        }
        else if (SlotData.Count < 2 && StaticProperties.Instance.GameType != GameTypes.ADVENTURE)
        {
            CharacterSelectionController.Instance.SetErrorMessage("There must be more than 1 player to start an Arena game");
            return;
        }
        else if(StaticProperties.Instance.GameType == GameTypes.ARENATEAM && CountArenaTeams() <= 1)
        {
            CharacterSelectionController.Instance.SetErrorMessage("There must be more than 1 team to start an Arena game");
            return;
        }
        else if (!AllPlayersReady() && StaticProperties.Instance.MultiPlayer)
        {
            CharacterSelectionController.Instance.SetErrorMessage("Please wait for all players to be ready");
            return;
        }
        if (StaticProperties.Instance.MultiPlayer)
        {
            if (AllPlayersReady())
            {
                OurConnection.Host_PlayGame();
            }
        }
        else
        {
            if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
            {
                GameController.Instance.LoadScene(Scene.Cinematic);
            }
            else
            {
                GameController.Instance.LoadScene(Scene.Arena);
            }
        }
    }

    protected bool AllPlayersReady()
    {
        foreach ( PlayerData player in SlotData )
        {
            if ( player.Name == null || player.Name == string.Empty )
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateName(string playerID, string name)
    {
        foreach (PlayerData data in SlotData)
        {
            if (data.PlayerID == playerID)
            {
                data.Name = name;
            }
        }
    }

    public void UpdateTeam(string playerID, int colour)
    {
        foreach (PlayerData data in SlotData)
        {
            if (data.PlayerID == playerID)
            {
                data.Team = colour;
            }
        }
    }

    public void DisatatchEvents()
    {
        NameField.onEndEdit.RemoveAllListeners();
        NetworkController.Instance.ServerInitializedCallback -= ServerCreatedCallback;
    }

    public void NameEdit(string name)
    {
        OurConnection.Client_UpdateNameWithHost(name);
    }

    public void TeamEdit()
    {
        if (StaticProperties.Instance.MultiPlayer)
            OurConnection.Client_UpdateTeamWithHost(StaticProperties.Instance.PlayerTeam);
    }

    public void SetButtonColours(int index)
    {
        for (int i = 0; i < lobbyButtons.Length; i++)
        {
            if (i == index) lobbyButtons[i].TurnRed();
            else lobbyButtons[i].RevertColour();
        }
    }

    public void SetKeyboardButtonColours(int index)
    {
        for (int i = 0; i < keyboardButtons.Length; i++)
        {
            if (i == index) keyboardButtons[i].TurnRed();
            else keyboardButtons[i].RevertColour();
        }
    }

    public void CreateKeyboard()
    {
        keyboardCanvas.gameObject.SetActive(true);
        KeyboardActive = true;
        moveCursorCooldown = 0.4f;
        CurrentButtonIndex = 0;
        SetKeyboardButtonColours(-1);
        SetKeyboardButtonColours(CurrentButtonIndex);
    }

    public void CloseKeyboard()
    {
        keyboardCanvas.gameObject.SetActive(false);
        KeyboardActive = false;
        moveCursorCooldown = 0.4f;
        CurrentButtonIndex = 1;
        SetKeyboardButtonColours(-1);
    }

    public void DeactivateButton(string s)
    {
        OverlayButtonCharSelect o = GameObject.Find(s).GetComponent<OverlayButtonCharSelect>();
        o.Inactive = true;
        o.text.color = Color.gray;
    }

    public void SetErrorMessage(string message)
    {
        errorMessage = message;
    }

    public void ShowErrorMessage()
    {
        errorCanvas.gameObject.SetActive(true);
        errorTextField.text = errorMessage;
        errorMessage = string.Empty;
        PopupActive = true;
        moveCursorCooldown = 0.2f;
        CurrentButtonIndex = lobbyButtons.Length - 1;
        SetButtonColours(CurrentButtonIndex);
    }

    public void ClosePopup()
    {
        errorTextField.text = "";
        errorCanvas.gameObject.SetActive(false);
        PopupActive = false;
        CurrentButtonIndex = 0;
    }

    public void SwitchAIType()
    {
        switch (CurrentAIType)
        {
            case ChosenCharacter.MYSTIC: CurrentAIType = ChosenCharacter.BARBARIAN; break;
            case ChosenCharacter.BARBARIAN: CurrentAIType = ChosenCharacter.KNIGHT; break;
            case ChosenCharacter.KNIGHT: CurrentAIType = ChosenCharacter.HUNTRESS; break;
            case ChosenCharacter.HUNTRESS: CurrentAIType = ChosenCharacter.MYSTIC; break;
            default: CurrentAIType = ChosenCharacter.MYSTIC; break;
        }
        aiTypeText.text = CurrentAIType.ToString();
    }
}
