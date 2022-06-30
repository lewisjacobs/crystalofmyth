using UnityEngine;
using System.Collections;

public class IntoCave : MonoBehaviour 
{

    protected float _time = 0.0f;

    public void Start()
    {
        GameController.Instance.CurrentGameState = GameState.CINEMATIC_RUNNING;
    }
	
	void Update ()
    {
        _time += Time.deltaTime;

        if( _time > 107 || Input.GetKey( KeyCode.Escape ) )
        {
            GameController.Instance.LoadScene( Scene.Adventure );
        }
	}
}
