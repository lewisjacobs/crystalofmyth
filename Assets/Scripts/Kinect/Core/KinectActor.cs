using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/////////////////////////////////////////////////
/// @class KinectActor
/// @brief An actor can be attached as a component to a game object, it then creates and position the assigned joint prefabs based off the KinectController joint data
///////////////////////////////////////////////
public class KinectActor : MonoBehaviour 
{
    public Image UICursor;                       //! Canvas cursor image, used when tracking the left hand
    public GameObject JointRepresentationPrefab; //! An object prefab that represents a body joint
    public int User = 1;                         //! User to build Actor off

    protected List<GameObject> _JointRepresentations = new List<GameObject>(); //! List of JointObject used to display the actor
    protected System.Array _JoingEnumVals;  //! Array containing the values in KinectMSWrapper.NuiSkeletonPositionIndex

    protected bool TrackHand = false; //! Track hand flag, set by had gestures

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Builds _JoingEnumVals, creates join prefabs and listens for gesture events from the GestureController
    /////////////////////////////////////////////////
	public void Start() 
    {
        KinectController.Instance.Setup();

        _JoingEnumVals = Enum.GetValues( typeof( KinectMSWrapper.NuiSkeletonPositionIndex ) );

         for ( int i = 0; i < _JoingEnumVals.Length - 1; i++ )
         {
             GameObject obj = ( GameObject ) Instantiate( JointRepresentationPrefab, Vector3.zero, Quaternion.identity );
             obj.name = _JoingEnumVals.GetValue( i ).ToString();
             _JointRepresentations.Add( obj );
         }

        // Listen for cursor tracking gestures
        GestureController.Instance.SuccessfullGestureteCallback += GestureCallback;
        GestureController.Instance.BeginProcessing = true;
	}

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Calls PositionPoints and CursorTracking 
    /////////////////////////////////////////////////
	public void Update() 
    {
        PositionPoints();
        CursorTracking();
	}
    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Positions the joint prefabs based of KinectController Data
    /////////////////////////////////////////////////
    protected void PositionPoints()
    {
        int iIter = 0;

        for ( int i = 0; i < _JoingEnumVals.Length - 1; i++ )
        {
            Vector3 v3NewPos;

            if ( User == 1 )
            {
                v3NewPos = KinectController.Instance.GetJointPosition( KinectController.Instance.Player1ID, iIter );
            }
            else
            {
                v3NewPos = KinectController.Instance.GetJointPosition( KinectController.Instance.Player2ID, iIter );
            }

            _JointRepresentations[ iIter ].transform.position = v3NewPos;

            iIter++;
        }

        Canvas.FindObjectsOfType<Text>()[ 1 ].text = KinectController.Instance.GetJointOrientation( KinectController.Instance.Player1ID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandRight, false ).ToString();

    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets the cursor position from the KinectController HandMotorVals
    /////////////////////////////////////////////////
    protected void CursorTracking()
    {
        if( TrackHand )
        {
            if ( UICursor != null )
            {
                RectTransform objectRectTransform = GameObject.Find( "Canvas" ).GetComponent<RectTransform>();

                // Multiply canvas pos by 2 to get width in world space
                Vector3 canvasWorldPos = objectRectTransform.position * 2;

                // Divide Kinect pos by 100 to get multiple factor
                Vector2 multiFactor = GestureController.Instance.HandMotorValues / 100;

                // Multiply Canvas world width by factor to get world location
                Vector3 worldLocation = new Vector3();

                if ( GestureController.Instance.HandMotorSide == GestureController.BodySide.Left )
                {
                    worldLocation.x = ( -canvasWorldPos.x * ( multiFactor.x ) ) + canvasWorldPos.x;
                }
                else
                {
                    worldLocation.x = -canvasWorldPos.x * ( multiFactor.x );
                }

                worldLocation.y = canvasWorldPos.y * multiFactor.y;

                UICursor.transform.position = worldLocation;
            }
        }
    }

    public void GestureCallback( Type type )
    {
        if( type == typeof( LeftHandForward ) || type == typeof( LeftHandMiddle ) )
        {
            TrackHand = true;
        }
        else if ( type == typeof( LeftHandBack  ) )
        {
            TrackHand = false;
        }
    }
}
