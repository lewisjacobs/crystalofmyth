using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OverlayButtonCharSelect : OverlayButton
{
    public enum ButtonTypeChar
    {
        MYSTIC,
        KNIGHT,
        BARBARIAN,
        HUNTRESS,
        DUNGEONMASTER,
        READY,
        BACK,
        KEYBOARD,
        CONFIRMNAME,
        LIVESPLUS,
        LIVESMINUS,
        TEAM,
        CONFIRMERROR,
        ADDAI,
        REMOVEAI,
        AITYPE,
        AITEAM
    }

    public bool Inactive { get; set; }
    public ButtonTypeChar buttonType;
    public Sprite charPortrait;

    private bool ready;

    void Start()
    {
        ready = false;
    }

    public override void TurnRed()
    {
        if (!Inactive && ((!CharacterSelectionController.Instance.KeyboardActive && !CharacterSelectionController.Instance.PopupActive) || popupButton) && !CharacterSelectionController.Instance.WaitingToConnect)
        {
            CharacterSelectionController.Instance.CurrentButtonIndex = buttonIndex;
            CharacterSelectionController.Instance.SetButtonColours(-1);
            Highlighted = true;
            text.color = Color.red;
        }
    }

    public override void RevertColour()
    {
        if (!Inactive && !CharacterSelectionController.Instance.WaitingToConnect)
        {
            if (popupButton)
            {
                Highlighted = false;
                text.color = Color.black;
            }
            else if ((!CharacterSelectionController.Instance.KeyboardActive && !CharacterSelectionController.Instance.PopupActive) && ready)
            {
                Highlighted = false;
                text.color = Color.green;
            }
            else if (!CharacterSelectionController.Instance.KeyboardActive && !CharacterSelectionController.Instance.PopupActive)
            {
                Highlighted = false;
                text.color = Color.white;
            }
        }
    }
    
    public override void TextClicked()
    {
        bool setSlot = false;

        if (!Inactive && ((!CharacterSelectionController.Instance.KeyboardActive && !CharacterSelectionController.Instance.PopupActive) || popupButton) && !CharacterSelectionController.Instance.WaitingToConnect)
        {
            switch (buttonType)
            {
                case ButtonTypeChar.BACK:
                    CharacterSelectionController.Instance.BackToLobby();
                    break;

                case ButtonTypeChar.READY:
                    if (StaticProperties.Instance.MultiPlayer)
                    {
                        CharacterSelectionController.Instance.NameEdit(CharacterSelectionController.Instance.NameField.text);
                    }
                    if (StaticProperties.Instance.SelectedCharacter == ChosenCharacter.DM)
                    {
                        GameController.Instance.SetGameType( LevelTypes.DungeonMaster );
                    }
                    else
                    {
                        GameController.Instance.SetGameType( LevelTypes.Character );
                    }
                        
                    if (!StaticProperties.Instance.IsHost)
                    {
                        if (CharacterSelectionController.Instance.NameField.text.Trim() != "")
                        {
                            ready = !ready;
                            if (ready)
                            {
                                CharacterSelectionController.Instance.Readied = true;
                                CharacterSelectionController.Instance.NameField.enabled = false;
                                CharacterSelectionController.Instance.CurrentButtonIndex = 2;
                                CharacterSelectionController.Instance.SetButtonColours(2);
                            }
                            else
                            {
                                CharacterSelectionController.Instance.Readied = false;
                                CharacterSelectionController.Instance.NameEdit(string.Empty);
                                CharacterSelectionController.Instance.NameField.enabled = true;
                            }
                        }
                        else
                        {
                            CharacterSelectionController.Instance.SetErrorMessage("Please enter a Character name");
                        }
                    }
                    else
                    {
                        CharacterSelectionController.Instance.TeamEdit();
                        CharacterSelectionController.Instance.PlayGame();
                    }
                    break;

                case ButtonTypeChar.MYSTIC:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.MYSTIC;
                        GameObject.Find("Selection Image").transform.position = this.transform.position;
                        setSlot = true;
                    }
                    break;

                case ButtonTypeChar.KNIGHT:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.KNIGHT;
                        GameObject.Find("Selection Image").transform.position = this.transform.position;
                        setSlot = true;
                    }
                    break;

                case ButtonTypeChar.BARBARIAN:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.BARBARIAN;
                        GameObject.Find("Selection Image").transform.position = this.transform.position;
                        setSlot = true;
                    }
                    break;

                case ButtonTypeChar.HUNTRESS:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.HUNTRESS;
                        GameObject.Find("Selection Image").transform.position = this.transform.position;
                        setSlot = true;
                    }
                    break;

                case ButtonTypeChar.DUNGEONMASTER:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        StaticProperties.Instance.SelectedCharacter = ChosenCharacter.DM;
                        GameObject.Find("Selection Image").transform.position = this.transform.position;
                        setSlot = true;
                    }
                    break;

                case ButtonTypeChar.KEYBOARD:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        RevertColour();
                        CharacterSelectionController.Instance.CreateKeyboard();
                    }
                    break;

                case ButtonTypeChar.CONFIRMNAME:
                    if (!CharacterSelectionController.Instance.Readied)
                    {
                        CharacterSelectionController.Instance.CloseKeyboard();
                    }
                    break;

                case ButtonTypeChar.LIVESPLUS:
                    CharacterSelectionController.Instance.UpdateLivesSlider(true);
                    break;

                case ButtonTypeChar.LIVESMINUS:
                    CharacterSelectionController.Instance.UpdateLivesSlider(false);
                    break;

                case ButtonTypeChar.CONFIRMERROR:
                    CharacterSelectionController.Instance.ClosePopup();
                    break;

                case ButtonTypeChar.ADDAI:
                    CharacterSelectionController.Instance.AddAIPlayer();
                    setSlot = true;
                    break;

                case ButtonTypeChar.REMOVEAI:
                    CharacterSelectionController.Instance.RemoveAIPlayer();
                    setSlot = true;
                    break;

                case ButtonTypeChar.AITYPE:
                    CharacterSelectionController.Instance.SwitchAIType();
                    break;
            }

            if ( !StaticProperties.Instance.MultiPlayer )
            {
                if (StaticProperties.Instance.IsHost && setSlot)
                {
                    if (buttonType != ButtonTypeChar.ADDAI && buttonType != ButtonTypeChar.REMOVEAI)
                    {
                        PlayerData data = new PlayerData()
                        {
                            PlayerID = "0",
                            CharacterType = StaticProperties.Instance.SelectedCharacter,
                            Team = StaticProperties.Instance.PlayerTeam
                        };

                        CharacterSelectionController.Instance.UpdateSlot(data);
                        CharacterSelectionController.Instance.SetSlot(data);
                    }
                    CharacterSelectionController.Instance.RefreshSlotData();
                }
            }
            else if( StaticProperties.Instance.MultiPlayer && setSlot  )
            {
				CharacterSelectionController.Instance.OurConnection.Client_TellHostToSetCharacter( StaticProperties.Instance.SelectedCharacter );
            }
        }
    }
}
