using UnityEngine;
using System.Collections;

public class CharacterSelectionScene : MonoBehaviour 
{
	public void Start () 
    {
        GameController.Instance.CurrentGameState = GameState.CharacterSelectionSetup;
	}
}
