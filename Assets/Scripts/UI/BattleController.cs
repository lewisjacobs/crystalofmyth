using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PigeonCoopToolkit.TIM;
using PigeonCoopToolkit;

public class BattleController : Singleton<BattleController>
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

    public void AttachControllerToBattle()
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
        if (GameController.Instance.CurrentGameState == GameState.INGAME)
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
        if (gameOver && GameController.Instance.CurrentGameState == GameState.INGAME)
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
        else if (!gameOver && endGameTimer == 10.0f)
        {
            int numPlayersAlive = 0;
            int numTeamsAlive = 0;
            bool greenAlive = false;
            bool blueAlive = false;
            bool yellowAlive = false;
            bool redAlive = false;
            foreach (Player p in PlayerController.Instance.Players)
            {
                if (p is CharacterPlayer)
                {
                    CharacterPlayer cp = (CharacterPlayer) p;
                    if (cp.Lives > 0)
                    {
                        numPlayersAlive++;
                        if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
                        {
                            switch(cp.PlayerTeam)
                            {
                                case TeamColours.BLUE:
                                    blueAlive = true;
                                    break;
                                case TeamColours.GREEN:
                                    greenAlive = true;
                                    break;
                                case TeamColours.RED:
                                    redAlive = true;
                                    break;
                                case TeamColours.YELLOW:
                                    yellowAlive = true;
                                    break;
                            }
                        }
                    }
                }
            }
            if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
            {
                if (blueAlive) numTeamsAlive++;
                if (greenAlive) numTeamsAlive++;
                if (redAlive) numTeamsAlive++;
                if (yellowAlive) numTeamsAlive++;
            }
            if (numPlayersAlive <= 1 || (StaticProperties.Instance.GameType == GameTypes.ARENATEAM && numTeamsAlive <= 1))
            {
                endGameCanvas.gameObject.SetActive(true);

                int i = 0;
                Player winningPlayer = null;
                Player highestKiller = null;
                string[] teams = { "Blue", "Red", "Yellow", "Green" };
                string reason = "";
                int winningTeam = 0;
                foreach (Player player in PlayerController.Instance.Players)
                {
                    i++;
                    if (player is CharacterPlayer)
                    {
                        CharacterPlayer cPlayer = (CharacterPlayer) player;
                        GameObject.Find("Score " + i).GetComponent<Text>().text = cPlayer.Name + " achieved " + cPlayer.Kills + " kills and finished with " + cPlayer.Lives + " lives!";
                        if (cPlayer.Lives > 0)
                        {
                            winningPlayer = cPlayer;
                            winningTeam = cPlayer.PlayerTeam;
                            if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
                                reason = " won as they were the last team standing!";
                            else
                                reason = " won as they were the last person standing!";
                        }
                    }
                    else
                    {
                        GameObject.Find("Score " + i).GetComponent<Text>().text = player.Name + " was the Overlord and achieved " + player.Kills + " kills!";
                    }
                    if(highestKiller == null || player.Kills > highestKiller.Kills)
                    {
                        highestKiller = player;
                    }
                }
                if (winningPlayer == null || highestKiller is DMPlayer)
                {
                    winningPlayer = highestKiller;
                    winningTeam = highestKiller.PlayerTeam;
                    reason = " won as they had the most kills!";
                }
                i++;
                if (i < 8)
                {
                    for (int j = i; j <= 8; j++)
                    {
                        GameObject.Find("Score " + j).gameObject.SetActive(false);
                    }
                }
                if (StaticProperties.Instance.GameType == GameTypes.ARENATEAM)
                {
                    GameObject.Find("Winner Text").GetComponent<Text>().text = teams[winningTeam - 1] + reason;
                }
                else
                {
                    GameObject.Find("Winner Text").GetComponent<Text>().text = winningPlayer.Name + reason;
                }
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

        if( StaticProperties.Instance.IsHost )
        {
            GameController.Instance.CurrentGameState = GameState.WAITING_TO_END_GAME;
        }
        else
        {
            GameController.Instance.QuitToMenu();
        }
    }
}

