using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : Singleton<MainMenu>
{
    public bool InfoActive { get; private set; }
    public bool ControlsActive { get; private set; }
    public int CurrentButtonIndex { get; set; }

    public Sprite[] mysticSpellSprites;
    public Sprite[] knightSpellSprites;
    public Sprite[] huntressSpellSprites;
    public Sprite[] barbarianSpellSprites;
    public Sprite[] overlordSpellSprites;

    public Image CursorImage { get; private set; }

    private Canvas buttonCanvas;
    private Canvas titleCanvas;
    private Canvas infoCanvas;
    private Canvas controlsCanvas;
    private Text nameText;
    private Text classText;
    private Text[] spellText;
    private Image[] spellSprites;
    private float scaleFactor;
    private float moveCursorCooldown;
    private OverlayButtonMainMenu[] menuButtons;
    private int infoIndex;

    public bool PopupActive { get; private set; }
    private Text errorTextField;
    private Canvas errorCanvas;
    private string errorMessage = string.Empty;

    public void AttachControllerToMainMenu()
    {
        CurrentButtonIndex = 0;
        moveCursorCooldown = 0;
        infoIndex = 0;
        
        CursorImage = GameObject.Find("Cursor Icon").GetComponent<Image>();
        CursorImage.gameObject.SetActive(false);

        buttonCanvas = GameObject.Find( "Menu Button Canvas" ).GetComponent<Canvas>();
        titleCanvas = GameObject.Find( "Menu Title Canvas" ).GetComponent<Canvas>();
        errorCanvas = GameObject.Find("Error Canvas").GetComponent<Canvas>();
        infoCanvas = GameObject.Find("Info Canvas").GetComponent<Canvas>();
        controlsCanvas = GameObject.Find("Controls Canvas").GetComponent<Canvas>();
        errorTextField = errorCanvas.transform.Find( "Error Text" ).gameObject.GetComponent<Text>();

        menuButtons = new OverlayButtonMainMenu[9];

        menuButtons[0] = GameObject.Find("Adventure Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[1] = GameObject.Find("Arena Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[2] = GameObject.Find("Team Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[3] = GameObject.Find("Info Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[4] = GameObject.Find("Controls Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[5] = GameObject.Find("Quit Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[6] = GameObject.Find("Confirm Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[7] = GameObject.Find("Next Button").GetComponent<OverlayButtonMainMenu>();
        menuButtons[8] = GameObject.Find("Done Button").GetComponent<OverlayButtonMainMenu>();

        nameText = GameObject.Find("Name Text").GetComponent<Text>();
        classText = GameObject.Find("Class Text").GetComponent<Text>();

        spellText = new Text[6];

        spellText[0] = GameObject.Find("Spell Text 1").GetComponent<Text>();
        spellText[1] = GameObject.Find("Spell Text 2").GetComponent<Text>();
        spellText[2] = GameObject.Find("Spell Text 3").GetComponent<Text>();
        spellText[3] = GameObject.Find("Spell Text 4").GetComponent<Text>();
        spellText[4] = GameObject.Find("Spell Text 5").GetComponent<Text>();
        spellText[5] = GameObject.Find("Spell Text 6").GetComponent<Text>();

        spellSprites = new Image[6];

        spellSprites[0] = GameObject.Find("Spell Icon 1").GetComponent<Image>();
        spellSprites[1] = GameObject.Find("Spell Icon 2").GetComponent<Image>();
        spellSprites[2] = GameObject.Find("Spell Icon 3").GetComponent<Image>();
        spellSprites[3] = GameObject.Find("Spell Icon 4").GetComponent<Image>();
        spellSprites[4] = GameObject.Find("Spell Icon 5").GetComponent<Image>();
        spellSprites[5] = GameObject.Find("Spell Icon 6").GetComponent<Image>();

        ClosePopup();
        CloseInfo();
        CloseControls();

        float wFactor = ( ( 100f / 1920f ) * Screen.width ) / 100f;
        float hFactor = ( ( 100f / 1080f ) * Screen.height ) / 100f;
        if ( wFactor < hFactor ) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        buttonCanvas.scaleFactor = scaleFactor;
        titleCanvas.scaleFactor = scaleFactor;
        errorCanvas.scaleFactor = scaleFactor;
        infoCanvas.scaleFactor = scaleFactor;
        controlsCanvas.scaleFactor = scaleFactor;
    }

    public void Update()
    {
        if (GameController.Instance.CurrentGameState == GameState.MAINMENU)
        {
            if (errorMessage != string.Empty)
                ShowErrorMessage();
            if (moveCursorCooldown <= 0)
            {
                if (PopupActive || InfoActive || ControlsActive)
                {
                    if (Input.GetButton("Submit") && menuButtons[CurrentButtonIndex].Highlighted)
                    {
                        menuButtons[CurrentButtonIndex].TextClicked();
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
                        if (PopupActive)
                        {
                            ClosePopup();
                        }
                        if(InfoActive)
                        {
                            CloseInfo();
                        }
                        else CloseControls();
                    }
                }
                else
                {
                    if (Input.GetAxis("VerticalArrow") < 0 || Input.GetAxis("HorizontalArrow") > 0)
                    {
                        if (menuButtons[CurrentButtonIndex].Highlighted)
                        {
                            if (CurrentButtonIndex < 5)
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
                        menuButtons[CurrentButtonIndex].TextClicked();
                    }
                }
            }
            else moveCursorCooldown -= Time.deltaTime;
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
        CurrentButtonIndex = 6;
        SetButtonColours(CurrentButtonIndex);
    }

    public void ClosePopup()
    {
        errorTextField.text = "";
        errorCanvas.gameObject.SetActive(false);
        PopupActive = false;
        CurrentButtonIndex = 0;
    }

    public void OpenInfo()
    {
        infoCanvas.gameObject.SetActive(true);
        InfoActive = true;
        SetNextText();
        moveCursorCooldown = 0.2f;
        CurrentButtonIndex = 7;
        SetButtonColours(CurrentButtonIndex);
    }

    public void CloseInfo()
    {
        infoIndex = 0;
        infoCanvas.gameObject.SetActive(false);
        InfoActive = false;
        CurrentButtonIndex = 0;
        menuButtons[7].text.text = "Next";
    }
    public void OpenControls()
    {
        controlsCanvas.gameObject.SetActive(true);
        ControlsActive = true;
        moveCursorCooldown = 0.2f;
        CurrentButtonIndex = 8;
        SetButtonColours(CurrentButtonIndex);
    }

    public void CloseControls()
    {
        controlsCanvas.gameObject.SetActive(false);
        ControlsActive = false;
        CurrentButtonIndex = 0;
    }

    public void SetNextText()
    {
        string[] textArray = null;
        Sprite[] spriteArray = null;
        bool applyText = true;

        switch (infoIndex)
        {
            case 0:
                textArray = InfoText.Instance.MysticText;
                spriteArray = mysticSpellSprites;
                break;
            case 1:
                textArray = InfoText.Instance.KnightText;
                spriteArray = knightSpellSprites;
                break;
            case 2:
                textArray = InfoText.Instance.HuntressText;
                spriteArray = huntressSpellSprites;
                break;
            case 3:
                textArray = InfoText.Instance.BarbarianText;
                spriteArray = barbarianSpellSprites;
                break;
            case 4:
                textArray = InfoText.Instance.OverlordText;
                spriteArray = overlordSpellSprites;
                break;
            case 5:
                CloseInfo();
                applyText = false;
                break;
        }

        if (applyText)
        {
            nameText.text = textArray[0];
            classText.text = textArray[1];

            for (int i = 0; i < spellText.Length; i++)
            {
                spellText[i].text = textArray[i + 2];
                spellSprites[i].sprite = spriteArray[i];
            }

            infoIndex++;

            if (infoIndex == 5)
            {
                menuButtons[7].text.text = "Close";
            }
        }
    }

    public void ActivateCursor(Vector3 pos)
    {
        if (!CursorImage.isActiveAndEnabled)
            CursorImage.gameObject.SetActive(true);
        CursorImage.transform.position = pos;
    }

    public void DeactivateCursor()
    {
        if (CursorImage.isActiveAndEnabled)
            CursorImage.gameObject.SetActive(false);
    }
}

