using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using PigeonCoopToolkit.TIM;
using PigeonCoopToolkit;

public class AdventureController : Singleton<AdventureController>
{
    public bool MenuActive { get; private set; }
    public int CurrentButtonIndex { get; set; }
    public bool FadeGameOverScreen { get; private set; }

    private float moveCursorCooldown;
    private OverlayButtonBattle[] menuButtons;
    private Canvas menuCanvas;
    private float scaleFactor;

    private Canvas endGameCanvas;
    private float endGameTimer;
    private bool gameOver;
    protected bool _bossDefeated = false;

    public void AttachControllerToAdventure()
    {
        endGameTimer = 10.0f;

        CurrentButtonIndex = 0;
        moveCursorCooldown = 0;

        MenuActive = true;
        gameOver = false;
        FadeGameOverScreen = false;

        menuCanvas = GameObject.Find("Menu Canvas").GetComponent<Canvas>();
        endGameCanvas = GameObject.Find("Endgame Canvas").GetComponent<Canvas>();

        Vector3 temp = menuCanvas.transform.position;
        temp.z = -1;
        menuCanvas.transform.position = temp;

        temp = endGameCanvas.transform.position;
        temp.z = -1;
        endGameCanvas.transform.position = temp;

        menuButtons = new OverlayButtonBattle[2];

        menuButtons[0] = GameObject.Find("Resume Button").GetComponent<OverlayButtonBattle>();
        menuButtons[1] = GameObject.Find("Exit Button").GetComponent<OverlayButtonBattle>();

        CloseMenu();

        float wFactor = ( ( 100f / 1920f ) * Screen.width ) / 100f;
        float hFactor = ( ( 100f / 1080f ) * Screen.height ) / 100f;
        if ( wFactor < hFactor ) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        menuCanvas.scaleFactor = scaleFactor;
        endGameCanvas.scaleFactor = scaleFactor;

        endGameCanvas.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (GameController.Instance.CurrentGameState == GameState.INGAME_ADVENTURE)
        {
            TestForRoundOver();

            if (moveCursorCooldown <= 0)
            {
                if (Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0)
                {
                    if (menuButtons[CurrentButtonIndex].Highlighted)
                    {
                        if (CurrentButtonIndex < 1)
                        {
                            CurrentButtonIndex++;
                        }
                    }
                    moveCursorCooldown = 0.2f;
                    SetButtonColours(CurrentButtonIndex);
                }
                else if (Input.GetAxis("VerticalArrow") > 0 || Input.GetAxis("HorizontalArrow") < 0)
                {
                    if (menuButtons[CurrentButtonIndex].Highlighted)
                    {
                        if (CurrentButtonIndex > 0)
                        {
                            CurrentButtonIndex--;
                        }
                    }
                    moveCursorCooldown = 0.2f;
                    SetButtonColours(CurrentButtonIndex);
                }
                else if (Input.GetButton("Submit") && menuButtons[CurrentButtonIndex].Highlighted)
                {
                    moveCursorCooldown = 0.2f;
                    menuButtons[CurrentButtonIndex].TextClicked();
                }
            }
            else moveCursorCooldown -= Time.deltaTime;
        }
    }

    public void TestForRoundOver()
    {
        if (gameOver)
        {
            endGameTimer -= Time.deltaTime;
            if (endGameTimer <= 0)
            {
                ExitBattle();
            }
            else if (endGameTimer <= 5.0f)
            {
                UIController.Instance.ClearUI();
                FadeGameOverScreen = true;
            }
        }
        else if( _bossDefeated && endGameTimer == 10.0f )
        {
            endGameCanvas.gameObject.SetActive( true );

            for ( int i = 1; i <= 8; i++ )
            {
                GameObject.Find( "Score " + i ).gameObject.SetActive( false );
            }

            GameObject.Find( "Winner Text" ).GetComponent<Text>().text = "Congratulations you have claimed the Crystal, use it wisely...";
            gameOver = true;
        }

        else if (!gameOver && endGameTimer == 10.0f)
        {
            int numPlayersAlive = 0;
            foreach (Player p in PlayerController.Instance.Players)
            {
                if( p is RealCharacter )
                {
                    RealCharacter cp = ( RealCharacter ) p;
                    if (cp.Lives > 0)
                    {
                        numPlayersAlive++;
                    }
                }
            }
            if (numPlayersAlive <= 0)
            {
                endGameCanvas.gameObject.SetActive(true);

                for (int i = 1; i <= 8; i++)
                {
                    GameObject.Find("Score " + i).gameObject.SetActive(false);
                }
                GameObject.Find("Winner Text").GetComponent<Text>().text = "You are defeated, and the crystal remains unclaimed...";
                gameOver = true;
            }
        }
    }

    public void SetButtonColours(int index)
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == index) menuButtons[i].TurnRed();
            else menuButtons[i].RevertColour();
        }
    }
    
    public void OpenMenu()
    {
        menuCanvas.gameObject.SetActive(true);
        MenuActive = true;
        moveCursorCooldown = 0.2f;
        CurrentButtonIndex = 0;
        SetButtonColours(CurrentButtonIndex);
    }

    public void CloseMenu()
    {
#if  UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
#else
       TouchInput.SetActive( "Main" );
#endif

        menuCanvas.gameObject.SetActive(false);
        MenuActive = false;
    }

    public void ExitBattle()
    {
        CurrentButtonIndex = 0;
        gameOver = false;
        MenuActive = false;
        FadeGameOverScreen = false;
        moveCursorCooldown = 0;
        endGameTimer = -1;
        GameController.Instance.QuitToMenu();
    }

    public void BossKilled()
    {
        _bossDefeated = true;
    }
}

