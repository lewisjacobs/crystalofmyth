using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasSwitcher : MonoBehaviour 
{
    protected int _CurrentDelay;
    public int[] Delays;
    public float PlayTime;
    public float EndTime;
    public Image[] Art;

	// Use this for initialization
	void Start () 
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        _CurrentDelay = 0;
        PlayTime = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
    {
        PlayTime += Time.deltaTime;

        if( PlayTime > EndTime || Input.GetKey( KeyCode.Escape ) || Input.GetButton( "Cancel" ) || Input.GetMouseButton(0) )
        {
            Application.LoadLevel( "MainMenu" );
        }

        if( _CurrentDelay >= Delays.Length )
        {
            return;
        }

        if( PlayTime > Delays[_CurrentDelay] )
        {
            Art[_CurrentDelay].gameObject.SetActive( false );
            _CurrentDelay++;
            Art[ _CurrentDelay ].gameObject.SetActive( true );
        }
	}
}
