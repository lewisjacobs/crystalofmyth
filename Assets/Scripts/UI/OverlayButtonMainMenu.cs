using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OverlayButtonMainMenu : OverlayButton
{
    public enum MenuButtonType
    {
        ADVENTURE,
        ARENA,
        ARENATEAM,
        QUIT,
        CONFIRMERROR,
        INFOCREATE,
        INFONEXT,
        CONTROLS,
        CONTROLSCLOSE
    }

    public MenuButtonType thisButton;

    public override void TurnRed()
    {
        if ((!MainMenu.Instance.PopupActive && !MainMenu.Instance.InfoActive && !MainMenu.Instance.ControlsActive) || popupButton)
        {
            MainMenu.Instance.CurrentButtonIndex = buttonIndex;
            MainMenu.Instance.SetButtonColours(-1);
            Highlighted = true;
            text.color = Color.red;
            if (!popupButton)
                MainMenu.Instance.ActivateCursor(this.transform.position);
        }
    }

    public override void RevertColour()
    {
        if (thisButton == MenuButtonType.CONFIRMERROR)
            text.color = Color.black;
        else
            text.color = Color.white;
        if (Highlighted)
            MainMenu.Instance.DeactivateCursor();
        
        Highlighted = false;
    }

    public override void TextClicked()
    {
        switch (thisButton)
        {
            case MenuButtonType.QUIT:
                Application.Quit();
                break;

            case MenuButtonType.ADVENTURE:
                StaticProperties.Instance.GameType = GameTypes.ADVENTURE;
                StaticProperties.Instance.ArenaLobbyText = "Adventure";
                GameController.Instance.LoadScene( Scene.Lobby );
                break;

            case MenuButtonType.ARENA:
                StaticProperties.Instance.GameType = GameTypes.ARENA;
                StaticProperties.Instance.ArenaLobbyText = "Arena Deathmatch";
                if (KinectController.Instance.KinectInitialized && !GestureController.Instance.GetCalibrated())
                {
                    GameController.Instance.LoadScene( Scene.KinectCalibration );
                }
                else
                {
                    GameController.Instance.LoadScene( Scene.Lobby );
                }
                break;

            case MenuButtonType.ARENATEAM:
                StaticProperties.Instance.GameType = GameTypes.ARENATEAM;
                StaticProperties.Instance.ArenaLobbyText = "Team Arena";
                if (KinectController.Instance.KinectInitialized && !GestureController.Instance.GetCalibrated())
                {
                    GameController.Instance.LoadScene(Scene.KinectCalibration);
                }
                else
                {
                    GameController.Instance.LoadScene(Scene.Lobby);
                }
                break;

            case MenuButtonType.CONFIRMERROR:
                MainMenu.Instance.ClosePopup();
                break;

            case MenuButtonType.INFOCREATE:
                MainMenu.Instance.OpenInfo();
                break;

            case MenuButtonType.INFONEXT:
                MainMenu.Instance.SetNextText();
                break;

            case MenuButtonType.CONTROLS:
                MainMenu.Instance.OpenControls();
                break;

            case MenuButtonType.CONTROLSCLOSE:
                MainMenu.Instance.CloseControls();
                break;
        }
    }
}
