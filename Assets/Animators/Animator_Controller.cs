using UnityEngine;
using System.Collections;

public class Animator_Controller : MonoBehaviour {

	private Animator anim;
	// Use this for initialization
	void Start () {
		anim=GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if ( Input.GetKey("c") )
		{
			anim.SetBool("Damage",true);
		}
		else
		{
			anim.SetBool("Damage",false);
		}

		if(Input.GetKey ("w"))
		{
			anim.SetBool("Forward_Run",true);
		}
		else
		{
			anim.SetBool("Forward_Run",false);
		}

		if(Input.GetKey ("a"))
		{
			anim.SetBool("Left_Strafe",true);
		}
		else
		{
			anim.SetBool("Left_Strafe",false);
		}

		if(Input.GetKey ("d"))
		{
			anim.SetBool("Right_Strafe",true);
		}
		else
		{
			anim.SetBool("Right_Strafe",false);
		}

		if ( Input.GetButton( "FireLeft" ) )
		{
			anim.SetBool("Left_Att",true);
		}
		else
		{
			anim.SetBool("Left_Att",false);
		}

		if ( Input.GetButton( "FireRight" ) )
		{
			anim.SetBool("Right_Att",true);
		}
		else
		{
			anim.SetBool("Right_Att",false);
		}

		if ( Input.GetKey("s") )
		{
			anim.SetBool("Backward_Run",true);
		}
		else
		{
			anim.SetBool("Backward_Run",false);
		}


	}
}
