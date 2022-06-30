using UnityEngine;
using System.Collections;

public class AdventureBattle : MonoBehaviour 
{
    public GameObject Lava1;
    public GameObject Lava2;

	void Start () 
    {
        if( StaticProperties.Instance.IsHost )
        {
            GameController.Instance.CurrentGameState = GameState.STARTGAME_ADVENTURE;
        }
        else
        {
            GameController.Instance.CurrentGameState = GameState.WAITINGFORPLAYERS_ADVENTURE;
        }

        GameController.Instance.EnableTouchControls();
	}
	
	void Update () 
    {
	
	}
}
