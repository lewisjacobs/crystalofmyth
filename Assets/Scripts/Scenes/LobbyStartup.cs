using UnityEngine;
using System.Collections;

public class LobbyStartup : MonoBehaviour 
{
	public void Awake () 
    {
        GameController.Instance.CurrentGameState = GameState.LOBBYSETUP;
	}
}
