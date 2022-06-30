using UnityEngine;
using System.Collections;

public class SingletonCheck : MonoBehaviour 
{
    public GameObject ControllerPrefab;

	void Awake() 
    {
        GameObject objController = GameObject.Find( "#Controller" );

	    if( objController == null )
        {
            GameObject obj = ( GameObject ) Instantiate( ControllerPrefab );

            obj.name = "#Controller";
        }
	}
}
