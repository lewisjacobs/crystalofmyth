using UnityEngine;
using System;
using System.Collections;

public class AIDM : DMPlayer
{
	public Camera PlayerCamera { get; set; }
	public DungeonMasterCharacter DungeonMasterCharacter { get; set; }
	
	protected int _ActiveSpellIndex = 0;
	
	protected bool _CanFire = false;
	protected bool _SwitchingSpell = false;
	
	public AIDM()
    {
        IsAI = true;		
	}
	
	public override void Update () 
	{
		if( _SwitchingSpell )
		{
			SwitchSpell();
		}
	}
	
	public void GestureCallback( Type type )
	{
		if( type == typeof( RightHandForward ) && _CanFire )
		{
			_CanFire = false;
			FireSpell();
		}
		else if( type == typeof( RightHandBack ) && !_CanFire )
		{
			_CanFire = true;
		}
		else if( type == ( typeof( LeftHandMiddle ) ) || type ==( typeof( LeftHandForward ) ) )
		{
			_SwitchingSpell = true;
		}
		else if( type == ( typeof( LeftHandBack ) ) )
		{
			_SwitchingSpell = false;
		}
	}
	
	protected void FireSpell()
	{
		switch( _ActiveSpellIndex )
		{
		case 0: DungeonMasterCharacter.FireMeteor(this.PlayerID); break;
        case 1: DungeonMasterCharacter.FireIceRain(this.PlayerID); break;
		}
	}
	
	protected void SwitchSpell()
	{
		Vector3 leftHandPosition = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft );
		Vector3 leftShoulderPosition = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft );
		
		float shoulderAngle = Vector3.Angle( leftShoulderPosition, leftHandPosition );
		
		if( Between( shoulderAngle, 0, 10 ) )
		{
			_ActiveSpellIndex = 0;
		}
        else if( Between( shoulderAngle, 10, 20 ) )
        {
            _ActiveSpellIndex = 1;
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
