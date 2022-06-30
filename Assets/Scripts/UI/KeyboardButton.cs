using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KeyboardButton : OverlayButton
{
    public enum SceneTypes
    {
        LOBBY,
        SERVERLIST
    }

    public string letter;
    public SceneTypes SceneType { get; set; }        

    public override void TurnRed()
    {
        if (SceneType == SceneTypes.LOBBY)
        {
            CharacterSelectionController.Instance.CurrentButtonIndex = buttonIndex;
            CharacterSelectionController.Instance.SetKeyboardButtonColours(-1);
        }
        else if (SceneType == SceneTypes.SERVERLIST)
        {
            ArenaLobbyController.Instance.CurrentButtonIndex = buttonIndex;
            ArenaLobbyController.Instance.SetKeyboardButtonColours( -1 );
        }
        Highlighted = true;
        text.color = Color.red;
    }

    public override void RevertColour()
    {
        Highlighted = false;
        text.color = Color.black;
    }
    
    public override void TextClicked()
    {
        if (SceneType == SceneTypes.LOBBY)
        {
            if (letter.Equals("CLEAR"))
            {
                CharacterSelectionController.Instance.CharacterNameString.text = "";
            }
            else if (letter.Equals("CONFIRM"))
            {

                GameObject.Find("Name Input").GetComponent<InputField>().text = CharacterSelectionController.Instance.CharacterNameString.text;

                CharacterSelectionController.Instance.CloseKeyboard();
            }
            else CharacterSelectionController.Instance.CharacterNameString.text += letter;
        }
        else if (SceneType == SceneTypes.SERVERLIST)
        {
            if (letter.Equals("CLEAR"))
            {
                ArenaLobbyController.Instance.ServerNameString.text = "";
            }
            else if (letter.Equals("CONFIRM"))
            {

                GameObject.Find("Server Name Input").GetComponent<InputField>().text = ArenaLobbyController.Instance.ServerNameString.text;

                ArenaLobbyController.Instance.CloseKeyboard();
            }
            else ArenaLobbyController.Instance.ServerNameString.text += letter;
        }
    }
}
