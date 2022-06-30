using UnityEngine;
using System.Collections;

public class NPC : InteractableObject
{
    protected MainDungeonStateMachine _machine;
    public Animator Anim;

	public void Start () 
    {
        _machine = GameObject.Find( "MainDungeonStateMachine" ).GetComponent<MainDungeonStateMachine>();
	}
	
	public void Update () 
    {
	
	}

    public override void Interact()
    {
        if( _machine.CurrentState == MainDungeonStateMachine.AdventureState.End_Cinematic )
        {
            _machine.SetAdventureState( MainDungeonStateMachine.AdventureState.NPC1 );
            Anim.SetBool( "Talk", true );
        }
        else if ( _machine.CurrentState == MainDungeonStateMachine.AdventureState.NPC1_Finished )
        {
            _machine.SetAdventureState( MainDungeonStateMachine.AdventureState.NPC2 );
            Anim.SetBool( "Talk", true );
        }
        else if ( _machine.CurrentState == MainDungeonStateMachine.AdventureState.NPC2_Finished )
        {
            _machine.SetAdventureState( MainDungeonStateMachine.AdventureState.NPC3 );
            Anim.SetBool( "Talk", true );
        }

        else if ( _machine.CurrentState == MainDungeonStateMachine.AdventureState.NPC3_Finished )
        {
            _machine.SetAdventureState( MainDungeonStateMachine.AdventureState.NPC4 );
            Anim.SetBool( "Talk", true );
        }
    }
}
