using UnityEngine;
using System.Collections;
using PigeonCoopToolkit.TIM;

public class InteratorObject : MonoBehaviour 
{
	void Update () 
    {

        string inter = Input.inputString.ToLower();
        bool container = inter.Contains( "f" );

        if ( container == true || TouchInput.GetButton( "Main", "FireLeft" ) || Input.GetButton( "FireLeft" ) )
        {
            RaycastHit hit;
            if ( Physics.Raycast( this.transform.position, this.transform.forward, out hit ) )
            {
                InteractableObject intObj = hit.collider.GetComponent<InteractableObject>();
                
                if( intObj != null )
                {
                    intObj.Interact();
                }
            }
                
                
        }
	}
}
