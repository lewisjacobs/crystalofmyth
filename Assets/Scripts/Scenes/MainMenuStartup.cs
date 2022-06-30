using UnityEngine;
using System.Collections;

public class MainMenuStartup : MonoBehaviour {

	void Start () 
    {
        GameController.Instance.CurrentGameState = GameState.MAINMENUSETUP;
	}
}
