using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeamSelectButton : OverlayButtonCharSelect
{
    int teamIndex = 0;
    Image crystal; 

    void Start()
    {
        if (buttonType == ButtonTypeChar.TEAM)
            crystal = GameObject.Find("Team Image").GetComponent<Image>();
        else
            crystal = GameObject.Find("AI Team Image").GetComponent<Image>();
    }

    public override void TextClicked()
    {
        bool setSlot = false;

        if (!Inactive && !CharacterSelectionController.Instance.WaitingToConnect && !CharacterSelectionController.Instance.Readied)
        {
            if (teamIndex < 3) teamIndex++;
            else teamIndex = 0;

            if (buttonType == ButtonTypeChar.TEAM)
                setSlot = true;
            switch (teamIndex)
            {
                case 0:
                    crystal.sprite = Resources.Load<Sprite>("Textures/blueteam");
                    if(buttonType == ButtonTypeChar.AITEAM)
                    {
                        CharacterSelectionController.Instance.CurrentAITeam = TeamColours.BLUE;
                    }
                    else
                    {
                        StaticProperties.Instance.PlayerTeam = TeamColours.BLUE;
                        CharacterSelectionController.Instance.TeamEdit();
                    }
                    break;
                case 1:
                    crystal.sprite = Resources.Load<Sprite>("Textures/redteam");
                    if(buttonType == ButtonTypeChar.AITEAM)
                    {
                        CharacterSelectionController.Instance.CurrentAITeam = TeamColours.RED;
                    }
                    else
                    {
                        StaticProperties.Instance.PlayerTeam = TeamColours.RED;
                        CharacterSelectionController.Instance.TeamEdit();
                    }
                    break;
                case 2:
                    crystal.sprite = Resources.Load<Sprite>("Textures/yellowteam");
                    if(buttonType == ButtonTypeChar.AITEAM)
                    {
                        CharacterSelectionController.Instance.CurrentAITeam = TeamColours.YELLOW;
                    }
                    else
                    {
                        StaticProperties.Instance.PlayerTeam = TeamColours.YELLOW;
                        CharacterSelectionController.Instance.TeamEdit();
                    }
                    break;
                case 3:
                    crystal.sprite = Resources.Load<Sprite>("Textures/greenteam");
                    if (buttonType == ButtonTypeChar.AITEAM)
                    {
                        CharacterSelectionController.Instance.CurrentAITeam = TeamColours.GREEN;
                    }
                    else
                    {
                        StaticProperties.Instance.PlayerTeam = TeamColours.GREEN;
                        CharacterSelectionController.Instance.TeamEdit();
                    }
                    break;
            }


            if (!StaticProperties.Instance.MultiPlayer)
            {
                if (StaticProperties.Instance.IsHost && setSlot)
                {
                    PlayerData data = new PlayerData()
                    {
                        PlayerID = "0",
                        CharacterType = StaticProperties.Instance.SelectedCharacter,
                        Team = StaticProperties.Instance.PlayerTeam
                    };

                    CharacterSelectionController.Instance.UpdateSlot(data);
                    CharacterSelectionController.Instance.ResetSlots();
                    CharacterSelectionController.Instance.SetSlot(data);
                }
            }
            else if (StaticProperties.Instance.MultiPlayer && setSlot)
            {
                CharacterSelectionController.Instance.OurConnection.Client_TellHostToSetCharacter(StaticProperties.Instance.SelectedCharacter);
            }
        }
    }
}
