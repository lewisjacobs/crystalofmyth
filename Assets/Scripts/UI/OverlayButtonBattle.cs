using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OverlayButtonBattle : OverlayButton
{
    public enum BattleButtonType
    {
        RESUME,
        EXIT
    }

    public BattleButtonType thisButton;

    public override void TurnRed()
    {
        if ( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
        {
            AdventureController.Instance.CurrentButtonIndex = buttonIndex;
            AdventureController.Instance.SetButtonColours( -1 );
        }
        else
        {
            BattleController.Instance.CurrentButtonIndex = buttonIndex;
            BattleController.Instance.SetButtonColours( -1 );
        }
        Highlighted = true;
        text.color = Color.red;
    }

    public override void RevertColour()
    {
        Highlighted = false;
        text.color = Color.white;
    }

    public override void TextClicked()
    {
        switch (thisButton)
        {
            case BattleButtonType.RESUME:
                if ( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
                    AdventureController.Instance.CloseMenu();
                else
                    BattleController.Instance.CloseMenu();
                break;
            case BattleButtonType.EXIT:
                if ( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
                {
                    AdventureController.Instance.CloseMenu();
                    AdventureController.Instance.ExitBattle();
                }
                else
                {
                    BattleController.Instance.CloseMenu();
                    BattleController.Instance.ExitBattle();
                }
                break;
        }
    }
}