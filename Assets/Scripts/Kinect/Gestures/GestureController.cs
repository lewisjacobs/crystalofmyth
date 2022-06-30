using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/////////////////////////////////////////////////
/// @class KinectController
/// @brief Singleton that maintains user joints position and orientation data retrieved from the Kinect SDK. Reference Rumen Filkov, RFilkov.com
////////////////////////////////////////////////
public class GestureController : Singleton<GestureController>
{
    public enum BodySide
    {
        Left,
        Right
    }

    public BodySide HandMotorSide = BodySide.Left;

    protected KinectGesture _CalibrationGesture; //! Gesture which is used to calculated arm and leg positions
    protected bool _GesturesCalibrated = false;  //! Flag to determine if the gesture has been calibrated, used in update
    protected List<KinectGesture> _GestureList = new List<KinectGesture>(); //! List of gestures to detected after calibration

    protected CalibrationStructure _CalibrationStructure; //! Structure that holds calibration data
    protected Vector3[] _BodyPositionCash; //! Cash of the last body data retrieved from the KinectController

    protected Vector2 _HandMotorValues = new Vector2(); //! Vector holding the left hand position percentage values

    public delegate void GestureCompleted( Type gesture );
    public event GestureCompleted SuccessfullGestureteCallback; //! gesture complete event

    public bool BeginProcessing { get; set; } //! Flag to tell controller to start looking for gestures

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Property that returns _HandMotorValues
    ////////////////////////////////////////////////
    public Vector2 HandMotorValues
    {
        get
        {
            return _HandMotorValues;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Calls BuildGestures
    ////////////////////////////////////////////////
    public void Start()
    {
        if( BeginProcessing != true )
        {
            BeginProcessing = false;
        }

        HandMotorSide = BodySide.Left;
        BuildGestures();
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Calls ProcessGestures and HandMotor
    ////////////////////////////////////////////////
    public void Update()
    {
        if ( BeginProcessing )
        {
            ProcessGestures();
            HandMotor();
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Calls Creates instances of the gestures classes and adds them to the Gesture List
    ////////////////////////////////////////////////
    public void BuildGestures()
    {
        // calibration gesture
        _CalibrationGesture = new TPoseGesture();
        _CalibrationGesture.CompletedCallback += CalibrationGestureCallback;

        // right hand forward gesture
        RightHandForward rightHandForwardGesture = new RightHandForward();
        rightHandForwardGesture.CompletedCallback += GestureCompleteCallback;

        _GestureList.Add( rightHandForwardGesture );


        // right hand back gesture
        RightHandBack rightHandBackGesture = new RightHandBack();
        rightHandBackGesture.CompletedCallback += GestureCompleteCallback;

        _GestureList.Add( rightHandBackGesture );


        //left hand forward gesture
        LeftHandForward leftHandForwardGesture = new LeftHandForward();
        leftHandForwardGesture.CompletedCallback += GestureCompleteCallback;

        _GestureList.Add( leftHandForwardGesture );

        LeftHandMiddle leftHandMiddleGesture = new LeftHandMiddle();
        leftHandMiddleGesture.CompletedCallback += GestureCompleteCallback;

        _GestureList.Add( leftHandMiddleGesture );

        // left hand back gesture
        LeftHandBack leftHandBackGesture = new LeftHandBack();
        leftHandBackGesture.CompletedCallback += GestureCompleteCallback;

        _GestureList.Add( leftHandBackGesture );
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Loops through the gesture list calling CalibrateGesture passing the calibration data
    ////////////////////////////////////////////////
    public void CalibrateGestures()
    {
        foreach( KinectGesture gesture in _GestureList )
        {
            gesture.CalibrateGesture( _CalibrationStructure );
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Loops through the gesture list calling CheckCompletion if calibration has been performed
    ////////////////////////////////////////////////
    public void ProcessGestures()
    {
        Vector3[] playerOnePositions = KinectController.Instance.GetJointPositions( KinectController.Instance.Player1ID );
        _BodyPositionCash = playerOnePositions;

        if( playerOnePositions != null )
        {
            if ( _GesturesCalibrated )
            {
                foreach ( KinectGesture gesture in _GestureList )
                {
                    gesture.CheckCompletion( ref playerOnePositions );
                }
            }
            else
            {
                _CalibrationGesture.CheckCompletion( ref playerOnePositions );
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param id ID of the gesture that fired the callback
    /// @brief A callback fired from a calibration gesture when a gesture has completed successfully
    ////////////////////////////////////////////////
    public void CalibrationGestureCallback( Type gestureType )
    {
        if( !_GesturesCalibrated )
        {
            Debug.Log( "Calibration Gesture Complete" );
            _CalibrationStructure = new CalibrationStructure();
            Vector3 armVector = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandRight ] - _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight ];
            Vector3 legVector = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.FootRight ] - _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HipRight ];

            _CalibrationStructure.ArmLength = Mathf.Abs( armVector.magnitude );
            _CalibrationStructure.LegLength = Mathf.Abs( legVector.magnitude );

            CalibrateGestures();

            _GesturesCalibrated = true;

            if (SuccessfullGestureteCallback != null)
            {
                SuccessfullGestureteCallback(gestureType);
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param id ID of the gesture that fired the callback
    /// @brief A callback fired from gestures when a gesture has completed successfully
    ////////////////////////////////////////////////
    public void GestureCompleteCallback( Type gestureType )
    {
        if( SuccessfullGestureteCallback != null )
        {
            SuccessfullGestureteCallback( gestureType );
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets the cursor position based off the HandMotorVals acquired from the KinectController
    ////////////////////////////////////////////////
    public void HandMotor()
    {
        if ( _GesturesCalibrated )
        {
            if( HandMotorSide == BodySide.Right )
            {
                HandMotorRight();
            }
            else
            {
                HandMotorLeft();
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets hand motor values based on left hand
    ////////////////////////////////////////////////
    protected void HandMotorLeft()
    {
        try
        {
            Vector3 leftHandPostion = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft ];
            Vector3 leftShoulderPostion = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Spine ];
            Vector3 distanceVector = leftHandPostion - leftShoulderPostion;

            if ( leftHandPostion.x < leftShoulderPostion.x )
            {
                // distance between hand and shoulder / calibrated arm distance * 100
                float xPersentage = Mathf.Abs( distanceVector.x ) / _CalibrationStructure.ArmLength * 100;
                xPersentage = Mathf.Clamp( xPersentage, 0, 100 );

                _HandMotorValues.x = xPersentage;
            }

            if ( leftHandPostion.y > leftShoulderPostion.y )
            {
                // distance between hand and shoulder / calibrated arm distance * 100
                float yPersentage = Mathf.Abs( distanceVector.y ) / _CalibrationStructure.ArmLength * 100;
                yPersentage = Mathf.Clamp( yPersentage, 0, 100 );

                _HandMotorValues.y = yPersentage;
            }
        }
        catch( Exception )
        {
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets hand motor values based on right hand
    ////////////////////////////////////////////////
    protected void HandMotorRight()
    {
        Vector3 rightHandPostion = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HandRight ];
        Vector3 rightShoulderPostion = _BodyPositionCash[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Spine ];
        Vector3 distanceVector = rightHandPostion - rightShoulderPostion;

        if ( rightHandPostion.x > rightShoulderPostion.x )
        {
            // distance between hand and shoulder / calibrated arm distance * 100
            float xPersentage = Mathf.Abs( distanceVector.x ) / _CalibrationStructure.ArmLength * 100;
            xPersentage = Mathf.Clamp( xPersentage, 0, 100 );

            _HandMotorValues.x = xPersentage;
        }

        if ( rightHandPostion.y > rightShoulderPostion.y )
        {
            // distance between hand and shoulder / calibrated arm distance * 100
            float yPersentage = Mathf.Abs( distanceVector.y ) / _CalibrationStructure.ArmLength * 100;
            yPersentage = Mathf.Clamp( yPersentage, 0, 100 );

            _HandMotorValues.y = yPersentage;
        }
    }

    public bool GetCalibrated()
    {
        return _GesturesCalibrated;
    }
}
