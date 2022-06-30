using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ArenaLobbyController : Singleton<ArenaLobbyController>
{
    public float scaleFactor;
    public OverlayButtonArenaLobby lobbyTextPrefab;
    public float pollDelay = 1;
    public bool PopupActive { get; private set; }
    public bool KeyboardActive { get; private set; }

    private Canvas buttonCanvas;
    private Canvas titleCanvas;
    private Canvas roomCanvas;
    private Canvas popupCanvas;
    private Canvas keyboardCanvas;
    private Vector3[] lobbyLocation;
    private int numShownLobbies;

    private List<OverlayButtonArenaLobby> overlayButtonList = new List<OverlayButtonArenaLobby>();
    
    public int CurrentButtonIndex { get; set; }
    private float moveCursorCooldown;
    private OverlayButtonArenaLobby[] lobbyButtons;
    private KeyboardButton[] keyboardButtons;
    private int numberOfKeyboardKeys = 29;

    public Text ServerNameString { get; set; }

    public void AttachControllerToLobby()
    {
        numShownLobbies = 0;

        NetworkController.Instance.HostListRefresh += RefreshCallback;

        buttonCanvas = GameObject.Find( "Button Canvas" ).GetComponent<Canvas>();
        titleCanvas = GameObject.Find( "Title Canvas" ).GetComponent<Canvas>();
        roomCanvas = GameObject.Find( "Lobby Canvas" ).GetComponent<Canvas>();
        popupCanvas = GameObject.Find( "Popup Canvas" ).GetComponent<Canvas>();
        keyboardCanvas = GameObject.Find( "Keyboard Canvas" ).GetComponent<Canvas>();
        ServerNameString = GameObject.Find( "Input Text" ).GetComponent<Text>();

        lobbyButtons = new OverlayButtonArenaLobby[ 7 ];

        lobbyButtons[ 0 ] = GameObject.Find( "Back Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 1 ] = GameObject.Find( "Refresh Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 2 ] = GameObject.Find( "Solo Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 3 ] = GameObject.Find( "Create Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 4 ] = GameObject.Find( "Confirm Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 5 ] = GameObject.Find( "Keyboard Button" ).GetComponent<OverlayButtonArenaLobby>();
        lobbyButtons[ 6 ] = GameObject.Find( "Cancel Button" ).GetComponent<OverlayButtonArenaLobby>();

        keyboardButtons = new KeyboardButton[ numberOfKeyboardKeys ];

        for ( int i = 1; i <= numberOfKeyboardKeys; i++ )
        {
            keyboardButtons[ i - 1 ] = GameObject.Find( "Key " + i ).GetComponent<KeyboardButton>();
            keyboardButtons[ i - 1 ].buttonIndex = i - 1;
            keyboardButtons[ i - 1 ].SceneType = KeyboardButton.SceneTypes.SERVERLIST;
        }

        float wFactor = ( ( 100f / 1920f ) * Screen.width ) / 100f;
        float hFactor = ( ( 100f / 1080f ) * Screen.height ) / 100f;
        if ( wFactor < hFactor ) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        buttonCanvas.scaleFactor = scaleFactor;
        titleCanvas.scaleFactor = scaleFactor;
        roomCanvas.scaleFactor = scaleFactor;
        popupCanvas.scaleFactor = scaleFactor;
        keyboardCanvas.scaleFactor = scaleFactor;


        Vector3 t1 = keyboardCanvas.transform.position;
        t1.z = -1;
        keyboardCanvas.transform.position = t1;

        Vector3 t2 = popupCanvas.transform.position;
        t2.z = -1;
        popupCanvas.transform.position = t2;

        ClosePopup();
        CloseKeyboard();

        float yCounter = 720;
        lobbyLocation = new Vector3[ 10 ];

        for ( int i = 0; i < 10; i++ )
        {
            lobbyLocation[ i ] = new Vector3( ( Screen.width / 2 ), ( 0 + yCounter ) * scaleFactor, 0 );
            yCounter -= 50;
        }

        CurrentButtonIndex = 0;

        GameObject.Find("Title Text").GetComponent<Text>().text = StaticProperties.Instance.ArenaLobbyText;
    }

    public void Update()
    {
        if ( GameController.Instance.CurrentGameState == GameState.ARENALOBBY )
        {
            if (moveCursorCooldown <= 0)
            {
                if (PopupActive && !KeyboardActive)
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
                            if (CurrentButtonIndex < lobbyButtons.Length - 1)
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
                            if (CurrentButtonIndex > lobbyButtons.Length - 3)
                            {
                                CurrentButtonIndex--;
                            }
                        }
                        moveCursorCooldown = 0.2f;
                        SetButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetButton("Back"))
                    {
                        ClosePopup();
                    }

                }
                else if (KeyboardActive)
                {
                    if (Input.GetButton("Submit") && keyboardButtons[CurrentButtonIndex].Highlighted)
                    {
                        moveCursorCooldown = 0.1f;
                        keyboardButtons[CurrentButtonIndex].TextClicked();
                    }

                    if (Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0)
                    {
                        if (keyboardButtons[CurrentButtonIndex].Highlighted)
                        {
                            CurrentButtonIndex++;
                        }
                        if (CurrentButtonIndex > numberOfKeyboardKeys - 1)
                        {
                            CurrentButtonIndex = 0;
                        }
                        moveCursorCooldown = 0.1f;
                        SetKeyboardButtonColours(CurrentButtonIndex);
                    }
                    else if (Input.GetAxis("VerticalArrow") > 0 || Input.GetAxis("HorizontalArrow") < 0)
                    {
                        if (keyboardButtons[CurrentButtonIndex].Highlighted)
                        {
                            CurrentButtonIndex--;
                        }
                        if (CurrentButtonIndex < 0)
                        {
                            CurrentButtonIndex = numberOfKeyboardKeys - 1;
                        }
                        moveCursorCooldown = 0.1f;
                        SetKeyboardButtonColours(CurrentButtonIndex);
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
                            if (CurrentButtonIndex < lobbyButtons.Length - 4)
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

    private void CreateHostList( ref List<string> hostList )
    {
		ClearHostList ();
        StaticProperties.Instance.NumberOfServers = 0;

        int hostIndex = 0;
        foreach (string s in hostList)
        {
            AddNewLobby(string.Format("{0}. {1}", hostIndex, s));
            hostIndex++;
        }

        List<OverlayButtonArenaLobby> temp = new List<OverlayButtonArenaLobby>();

        temp.Add(lobbyButtons[0]);

        for (int i = 0; i < overlayButtonList.Count; i++)
        {
            temp.Add(overlayButtonList[i]);
        }

        temp.Add(lobbyButtons[lobbyButtons.Length - 6]);
        temp.Add(lobbyButtons[lobbyButtons.Length - 5]);
        temp.Add(lobbyButtons[lobbyButtons.Length - 4]);
        temp.Add(lobbyButtons[lobbyButtons.Length - 3]);
        temp.Add(lobbyButtons[lobbyButtons.Length - 2]);
        temp.Add(lobbyButtons[lobbyButtons.Length - 1]);

        lobbyButtons = new OverlayButtonArenaLobby[temp.Count];

        for (int i = 0; i < temp.Count; i++)
        {
            temp[i].buttonIndex = i;
            lobbyButtons[i] = temp[i];
        }
    }

    public void AddNewLobby(string name)
    {
        OverlayButtonArenaLobby t = (OverlayButtonArenaLobby) Instantiate(lobbyTextPrefab, lobbyLocation[StaticProperties.Instance.NumberOfServers], Quaternion.identity);
        StaticProperties.Instance.NumberOfServers++;
        numShownLobbies++;
        t.text.text = name;
        t.text.rectTransform.localScale *= scaleFactor;
        t.text.rectTransform.SetParent(GameObject.Find("Lobby Canvas").GetComponent<Canvas>().transform );
        overlayButtonList.Add(t);
    }

    public void RefreshCallback( ref List<string> hostList )
    {
        CreateHostList( ref hostList );
    }

    public void ClearHostList()
	{
		foreach (OverlayButtonArenaLobby o in overlayButtonList)
		{
			if (o != null)
				Canvas.Destroy(o.gameObject);
		}

        overlayButtonList.Clear();
    }

    public void StopRefreshingHosts()
    {
        NetworkController.Instance.HostListRefresh -= RefreshCallback;
        ClearHostList();
    }

    public void CreatePopup()
    {
        popupCanvas.gameObject.SetActive(true);
        PopupActive = true;
        moveCursorCooldown = 0.4f;
        CurrentButtonIndex = lobbyButtons.Length - 3;
        SetButtonColours(-1);
    }

    public void ClosePopup()
    {
        popupCanvas.gameObject.SetActive(false);
        PopupActive = false;
        moveCursorCooldown = 0.4f;
        CurrentButtonIndex = lobbyButtons.Length - 4;
        SetButtonColours(-1);
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
        SetButtonColours(-1);
        SetKeyboardButtonColours(CurrentButtonIndex);
    }

    public void CloseKeyboard()
    {
        keyboardCanvas.gameObject.SetActive(false);
        KeyboardActive = false;
        moveCursorCooldown = 0.4f;
        CurrentButtonIndex = lobbyButtons.Length - 3;
        SetKeyboardButtonColours(-1);
    }
}
