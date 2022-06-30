using UnityEngine;
using System.Collections;

public class BattleArena : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
        if( StaticProperties.Instance.IsHost )
        {
            GameController.Instance.CurrentGameState = GameState.STARTGAME;
        }
        else
        {
            GameController.Instance.CurrentGameState = GameState.WAITINGFORPLAYERS;
        }

        GameController.Instance.EnableTouchControls();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
