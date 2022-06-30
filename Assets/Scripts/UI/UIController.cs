using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public enum UIType
{
    MYSTIC,
    KNIGHT,
    HUNTRESS,
    BARBARIAN,
    DUNGEONMASTER,
    LOBBY
}

public class UIController : Singleton<UIController>
{
    private Canvas canvas;
    private Text textHealth;
    private Text textMana;
    private Text textSpellLeft;
    private Text textSpellRight;
    private Text textSpellMiddle;
    private Image healthbar;
    private Image manabar;
    private Image portrait;
    private List<Image> currentStatusImages;
    private List<Image> currentStatusUnderlays;
    private List<float> currentStatusDurations;
    private List<float> currentStatusTimeToRemove;
    private Vector3[] conditionLocation;
    private float scaleFactor;
    private Sprite charSprite;
    private Text[] livesArray;
    private Image screenFade;
    private float screenFadeTimer;
    private bool screenFaderBeenReset;

    protected Sprite[] leftSprites;
    protected Sprite[] rightSprites;
    protected Sprite middleSprite;
    protected Image leftIcon;
    protected Image rightIcon;
    protected Image middleIcon;

    public Sprite[] mysticSprites;
    public Sprite mysticmiddleSprite;
    public Sprite[] knightleftSprites;
    public Sprite[] knightrightSprites;
    public Sprite knightmiddleSprite;
    public Sprite[] barbarianleftSprites;
    public Sprite[] barbarianrightSprites;
    public Sprite barbarianmiddleSprite;
    public Sprite[] huntressleftSprites;
    public Sprite[] huntressrightSprites;
    public Sprite huntressmiddleSprite;
    public Sprite[] overlordSprites;
    public Image attackIconPrefab;
    public Image defenceIconPrefab;
    public Image crippleIconPrefab;
    public Image frozenIconPrefab;
    public Image chargeIconPrefab;
    public Image hasteIconPrefab;
    public Image cursor;

    public void ClearUI()
    {
        if( StaticProperties.Instance.SelectedCharacter != ChosenCharacter.DM )
        {
            for ( int i = 0; i < currentStatusImages.Count; i++ )
            {
                Destroy( currentStatusImages[ i ] );
                Destroy( currentStatusUnderlays[ i ] );
            }
            currentStatusImages.Clear();
            currentStatusUnderlays.Clear();
            currentStatusDurations.Clear();
            currentStatusTimeToRemove.Clear();
        }
    }

    public void Awake()
    {
		DontDestroyOnLoad( this.gameObject );
    }

    public void SetupUIController(UIType charType)
    {
        screenFaderBeenReset = true;
        screenFadeTimer = 0;
        
        float wFactor = ((100f / 1920f) * Screen.width) / 100f;
        float hFactor = ((100f / 1080f) * Screen.height) / 100f;
        if (wFactor < hFactor) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        leftSprites = new Sprite[5];
        rightSprites = new Sprite[5];

        if (charType == UIType.MYSTIC)
        {
            charSprite = Resources.Load<Sprite>("Textures/mysticportrait");

            for (int i = 0; i < mysticSprites.Length; i++ )
            {
                leftSprites[i] = mysticSprites[i];
                rightSprites[i] = mysticSprites[i];
            }

            middleSprite = mysticmiddleSprite;
        }
        else if (charType == UIType.KNIGHT)
        {
            charSprite = Resources.Load<Sprite>("Textures/knightportrait");

            for (int i = 0; i < knightleftSprites.Length; i++)
            {
                leftSprites[i] = knightleftSprites[i];
            }

            for (int i = 0; i < knightrightSprites.Length; i++)
            {
                rightSprites[i] = knightrightSprites[i];
            }

            middleSprite = knightmiddleSprite;
        }
        else if (charType == UIType.BARBARIAN)
        {
            charSprite = Resources.Load<Sprite>("Textures/barbarianportrait");

            for (int i = 0; i < barbarianleftSprites.Length; i++)
            {
                leftSprites[i] = barbarianleftSprites[i];
            }

            for (int i = 0; i < barbarianrightSprites.Length; i++)
            {
                rightSprites[i] = barbarianrightSprites[i];
            }

            middleSprite = barbarianmiddleSprite;
        }
        else if (charType == UIType.HUNTRESS)
        {
            charSprite = Resources.Load<Sprite>("Textures/huntressportrait");

            for (int i = 0; i < huntressleftSprites.Length; i++)
            {
                leftSprites[i] = huntressleftSprites[i];
            }

            for (int i = 0; i < huntressrightSprites.Length; i++)
            {
                rightSprites[i] = huntressrightSprites[i];
            }

            middleSprite = huntressmiddleSprite;
        }
        else if( charType == UIType.DUNGEONMASTER )
        {
            charSprite = Resources.Load<Sprite>("Textures/huntressportrait");

            cursor = GameObject.Find("Cursor").GetComponent<Image>();
            canvas = GameObject.Find("Overlord HUD").GetComponent<Canvas>();
            textSpellLeft = GameObject.Find("Spell Text").GetComponent<Text>();
            leftIcon = GameObject.Find("Equipped Spell").GetComponent<Image>();

            for (int i = 0; i < overlordSprites.Length; i++)
            {
                leftSprites[i] = overlordSprites[i];
            }

            GameObject.Find("Character HUD").gameObject.SetActive(false);
        }

        if (charType != UIType.LOBBY)
        {
            if (charType != UIType.DUNGEONMASTER)
            {
                textHealth = GameObject.Find("Text Health").GetComponent<Text>();
                textMana = GameObject.Find("Text Mana").GetComponent<Text>();
                textSpellLeft = GameObject.Find("Spell Left Text").GetComponent<Text>();
                textSpellRight = GameObject.Find("Spell Right Text").GetComponent<Text>();
                textSpellMiddle = GameObject.Find("Spell Middle Text").GetComponent<Text>();
                healthbar = GameObject.Find("HUD Health Bar").GetComponent<Image>();
                manabar = GameObject.Find("HUD Mana Bar").GetComponent<Image>();
                leftIcon = GameObject.Find("Spell Left").GetComponent<Image>();
                rightIcon = GameObject.Find("Spell Right").GetComponent<Image>();
                middleIcon = GameObject.Find("Spell Middle").GetComponent<Image>();
                canvas = GameObject.Find("Character HUD").GetComponent<Canvas>();

                currentStatusImages = new List<Image>();
                currentStatusUnderlays = new List<Image>();
                currentStatusDurations = new List<float>();
                currentStatusTimeToRemove = new List<float>();

                rightIcon.sprite = rightSprites[0];
                middleIcon.sprite = middleSprite;

                float xCounter = 150 * scaleFactor;
                conditionLocation = new Vector3[15];
                for (int counter = 0; counter < 15; counter++)
                {
                    conditionLocation[counter] = new Vector3(xCounter, (Screen.height) - (120 * scaleFactor), 0);
                    xCounter += 30 * scaleFactor;
                }

                if(!(StaticProperties.Instance.GameType == GameTypes.ADVENTURE))
                    GameObject.Find("Overlord HUD").gameObject.SetActive(false);
            }

            screenFade = GameObject.Find("Screen Fade").GetComponent<Image>();
            Vector3 temp = screenFade.transform.position;
            temp.z = -1;
            screenFade.transform.position = temp;

            portrait = GameObject.Find("Character Portrait").GetComponent<Image>();

            livesArray = new Text[PlayerController.Instance.Players.Count];

            int i = 0;
            foreach (Player p in PlayerController.Instance.Players)
            {
                if ( !( StaticProperties.Instance.GameType == GameTypes.ADVENTURE && p is AICharacter ) )
                {
                    livesArray[ i ] = GameObject.Find( "Text P" + ( i + 1 ) ).GetComponent<Text>();

                    if ( p is CharacterPlayer )
                    {
                        CharacterPlayer cp = ( CharacterPlayer ) p;
                        livesArray[ i ].text = cp.Name + "\t\tL: " + cp.Lives + "\t\tK: " + cp.Kills;
                    }
                    else
                    {
                        livesArray[ i ].text = p.Name + "\t\tL: " + "∞" + "\t\tK: " + p.Kills;
                    }
                    i++;
                }
            }

            for (int j = i; j < 8; j++)
            {
                GameObject.Find("Text P" + (j+1)).SetActive(false);
            }

            portrait.sprite = charSprite;

            canvas.scaleFactor = scaleFactor;
            GameObject.Find("HUD Canvas").GetComponent<Canvas>().scaleFactor = scaleFactor;
            
            Color c = screenFade.color;
            c.a = 0;
            screenFade.color = c;
        }
    }

    public void UpdateUI(int statCurrentHealth, int statBaseHealth, int statCurrentMana, int statBaseMana, float cdTimerLeft, float cdTimerRight, float cdTimerMiddle, int leftSpellID,
        int rightSpellID, bool addAttack, bool addCripple, bool addDefence, bool addFreeze, bool addCharge, bool addHaste, float removeAttack, float removeCripple, float removeDefence, 
        float removeFreeze, float removeCharge, float removeHaste, bool Alive, bool PlayerDead)
    {
        if (addAttack) AddIcon(removeAttack, attackIconPrefab);
        if (addCripple) AddIcon(removeCripple, crippleIconPrefab);
        if (addDefence) AddIcon(removeDefence, defenceIconPrefab);
        if (addFreeze) AddIcon(removeFreeze, frozenIconPrefab);
        if (addCharge) AddIcon(removeCharge, chargeIconPrefab);
        if (addHaste) AddIcon(removeHaste, hasteIconPrefab);
 
        textHealth.text = statCurrentHealth + " / " + statBaseHealth;
        textMana.text = statCurrentMana + " / " + statBaseMana;
        textSpellLeft.text = "" + Math.Round(cdTimerLeft, 1);
        textSpellRight.text = "" + Math.Round(cdTimerRight, 1);
        textSpellMiddle.text = "" + Math.Round(cdTimerMiddle, 1);

        float healthPercentage = (100f / statBaseHealth) * statCurrentHealth;
        if (healthPercentage >= 0)
            healthbar.fillAmount = healthPercentage / 100f;
        float manaPercentage = (100f / statBaseMana) * statCurrentMana;
        if (manaPercentage >= 0)
            manabar.fillAmount = manaPercentage / 100f;

        leftIcon.sprite = leftSprites[leftSpellID];
        rightIcon.sprite = rightSprites[rightSpellID];

        bool statusTimedOut = false;

        for (int i = 0; i < currentStatusDurations.Count; i++)
        {
            float durationPercentage = (100f / currentStatusDurations[i]) * (currentStatusTimeToRemove[i] - GameController.Instance.ElapsedTime);
            currentStatusImages[i].fillAmount = durationPercentage / 100f;
            if (GameController.Instance.ElapsedTime > currentStatusTimeToRemove[i])
            {
                currentStatusDurations.RemoveAt(i);
                currentStatusTimeToRemove.RemoveAt(i);
                currentStatusImages[i].gameObject.SetActive(false);
                GameController.Instance.DestroyObject(currentStatusImages[i].gameObject);
                currentStatusImages.RemoveAt(i);
                currentStatusUnderlays[i].gameObject.SetActive(false);
                GameController.Instance.DestroyObject(currentStatusUnderlays[i].gameObject);
                currentStatusUnderlays.RemoveAt(i);
                statusTimedOut = true;
            }
        }

        if (statusTimedOut)
        {
            for (int i = 0; i < currentStatusDurations.Count; i++)
            {
                currentStatusImages[i].transform.position = conditionLocation[i];
                currentStatusUnderlays[i].transform.position = conditionLocation[i];
            }
        }

        if ((!Alive && !PlayerDead) || BattleController.Instance.FadeGameOverScreen)
            FadeOutScreen();
        else if (!screenFaderBeenReset)
            FadeInScreen();
    }

    public void UpdateOverlordUI(float cdTimer, int spellID)
    {
        leftIcon.sprite = leftSprites[spellID];
        textSpellLeft.text = "" + Math.Round(cdTimer, 1);
    }

    public void UpdateLives()
    {
        int it = 0;
        foreach (Player p in PlayerController.Instance.Players)
        {
            if( ! ( StaticProperties.Instance.GameType == GameTypes.ADVENTURE && p is AICharacter ) )
            {
                if ( p is CharacterPlayer )
                {
                    CharacterPlayer cp = ( CharacterPlayer ) p;
                    livesArray[ it ].text = cp.Name + "\t\tL: " + cp.Lives + "\t\tK: " + cp.Kills;
                }
                else
                {
                    livesArray[ it ].text = p.Name + "\t\tL: " + "∞" + "\t\tK: " + p.Kills;
                }
                it++;
            }
        }
    }
    
    public void FadeOutScreen()
    {
        Color color = screenFade.color;

        float fadePercentage;
        if (screenFadeTimer <= 2.5f)
        {
            fadePercentage = (100f / 2.5f) * screenFadeTimer;
        }
        else fadePercentage = 100;

        screenFadeTimer += Time.deltaTime;
        color.a = fadePercentage / 100f;
        screenFade.color = color;
        if (screenFadeTimer >= 4.0f)
            screenFaderBeenReset = false;
    }

    public void FadeInScreen()
    {
        Color color = screenFade.color;

        float fadePercentage = (100f / 2.5f) * (6.5f - screenFadeTimer);

        screenFadeTimer += Time.deltaTime;
        color.a = fadePercentage / 100f;
        screenFade.color = color;
        if (screenFadeTimer >= 6.5f)
        {
            resetScreenFader();
        }
    }

    public void resetScreenFader()
    {
        Color color = screenFade.color;
        screenFadeTimer = 0;
        color.a = 0;
        screenFade.color = color;
        screenFaderBeenReset = true;
    }
    
    public void AddIcon(float d, Image prefab)
    {
        Image o = ((Image) Instantiate (prefab, conditionLocation[currentStatusImages.Count], Quaternion.identity));
        Color c = o.color;
        c.a = 0.4f;
        o.color = c;
        Image i = ((Image) Instantiate (prefab, conditionLocation[currentStatusImages.Count], Quaternion.identity));
        o.rectTransform.sizeDelta *= scaleFactor;
        o.rectTransform.SetParent(canvas.transform);
        i.rectTransform.sizeDelta *= scaleFactor;
        i.rectTransform.SetParent(canvas.transform);
        currentStatusUnderlays.Add(o);
        currentStatusDurations.Add(d);
        currentStatusTimeToRemove.Add(d + GameController.Instance.ElapsedTime);
        currentStatusImages.Add(i);
    }	
    
    public Vector3 SetCursorPosition( Vector2 persentagePostion )
    {
        if ( cursor != null )
        {
            RectTransform objectRectTransform = GameObject.Find( "Overlord HUD" ).GetComponent<RectTransform>();

            // Multiply canvas pos by 2 to get width in world space
            Vector3 canvasWorldPos = objectRectTransform.position * 2;

            // Divide Kinect pos by 100 to get multiple factor
            Vector2 multiFactor = persentagePostion / 100;

            // Multiply Canvas world width by factor to get world location
            Vector3 worldLocation = new Vector3();

            if ( GestureController.Instance.HandMotorSide == GestureController.BodySide.Left )
            {
                worldLocation.x = ( -canvasWorldPos.x * ( multiFactor.x ) ) + canvasWorldPos.x;
            }
            else
            {
                worldLocation.x = -canvasWorldPos.x * ( multiFactor.x );
            }

            worldLocation.y = canvasWorldPos.y * multiFactor.y;

            cursor.transform.position = worldLocation;

            return worldLocation;
        }

        return Vector3.zero;
    }
}


