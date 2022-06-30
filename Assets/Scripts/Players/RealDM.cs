using UnityEngine;
using System;
using System.Collections;

public class RealDM : DMPlayer
{
	public Camera PlayerCamera { get; set; }
	public DungeonMasterCharacter DungeonMasterCharacter { get; set; }
	
	protected int _ActiveSpellIndex = 0;
	
	protected bool _CanFire = false;
	protected bool _SwitchingSpell = false;

    protected bool _TrackHands = false;

    protected float _SwitchTime = 0.0f;

    protected float _HandSpeed;
    protected Vector3 _LastHandPos;
	
	public RealDM()
	{
        GestureController.Instance.SuccessfullGestureteCallback += GestureCallback;
        IsAI = false;
	}

    public void RemoveGestureListening()
    {
        GestureController.Instance.SuccessfullGestureteCallback -= GestureCallback;
    }

    public override void Destroy()
    {
        RemoveGestureListening();
    }
	
	public override void Update () 
	{
        CalculateHandSpeed();

		if( _SwitchingSpell )
		{
            _SwitchTime += Time.deltaTime;

            if ( _SwitchTime > 0.2f && _HandSpeed < 5.0f )
            {
                SwitchSpell();
            }
		}
        else
        {
            _SwitchTime = 0.0f;
        }

        if( _TrackHands )
        {
            Vector3 cursorPosition = UIController.Instance.SetCursorPosition( GestureController.Instance.HandMotorValues );

            Ray ray = Camera.main.ScreenPointToRay( cursorPosition ); 
            RaycastHit hit;
            if( Physics.Raycast( ray, out hit ) )
            {
                Vector3 forward = hit.point - DungeonMasterCharacter.transform.position;
                DungeonMasterCharacter.SetForward( forward.normalized );
            }
        }

        if(DungeonMasterCharacter.SpellCooldowns != null)
            UIController.Instance.UpdateOverlordUI(DungeonMasterCharacter.SpellCooldowns[_ActiveSpellIndex], _ActiveSpellIndex);
	}

    protected void CalculateHandSpeed()
    {
        Vector3 currentHandPos = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandRight );
        _HandSpeed = ( currentHandPos - _LastHandPos ).magnitude * 100;
        _LastHandPos = currentHandPos;
    }

    public override void GrantKill()
    {
        Kills++;
        GameController.Instance.PlayerKilled(PlayerID, 20, Kills);
    }
	
	public void GestureCallback( Type type )
	{
		if( type == typeof( LeftHandForward ) && _CanFire )
		{
            _TrackHands = false;
			_CanFire = false;
			FireSpell();
		}
        else if( type == typeof( LeftHandMiddle ) )
        {
            _TrackHands = true;
            _CanFire = true;
        }
		else if( type == typeof( LeftHandBack ) )
		{
			_CanFire = true;
            _TrackHands = false;
		}
		else if( type ==( typeof( RightHandForward ) ) )
		{
            _SwitchingSpell = true;
		}
        else if ( type == ( typeof( RightHandMiddle ) ) || type == ( typeof( RightHandBack ) ) )
		{
			_SwitchingSpell = false;
		}
	}
	
	protected void FireSpell()
	{
        switch ( _ActiveSpellIndex )
        {
            case 0: DungeonMasterCharacter.FireMeteor(PlayerID); break;
            case 1: DungeonMasterCharacter.FireIceRain( PlayerID ); break;
            case 2: DungeonMasterCharacter.FireFireball( PlayerID ); break;
            case 3: DungeonMasterCharacter.FirePolarity( PlayerID ); break;
        }
    }
	
	protected void SwitchSpell()
	{
		Vector3 rightHandPosition = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandRight );
        Vector3 rightShoulderPosition = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight );
		Vector3 centreHip = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter );
        Vector3 centreShoulder = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter );

        Vector3 centreDirection = centreHip - centreShoulder;
        Vector3 armDirection = rightHandPosition - rightShoulderPosition;
		
		float shoulderAngle = Vector3.Angle( centreDirection, armDirection );

        //Debug.LogError( shoulderAngle );

        if (Between(shoulderAngle, 90, 110))
        {
            _ActiveSpellIndex = 0;
        }
        else if (Between(shoulderAngle, 70, 90))
        {
            _ActiveSpellIndex = 1;
        }
        else if (Between(shoulderAngle, 50, 70))
        {
            _ActiveSpellIndex = 2;
        }
        else if (Between(shoulderAngle, 30, 50))
        {
            _ActiveSpellIndex = 3;
        }
	}
	
	protected bool Between( float val, float min, float max )
	{
		if( val >= min && val <= max )
		{
			return true;
		}
		
		return false;
	}
}
