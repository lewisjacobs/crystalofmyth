using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/////////////////////////////////////////////////
/// @class KinectController
/// @brief Singleton that maintains user joints position and orientation data retrieved from the Kinect SDK. Reference Rumen Filkov, RFilkov.com
/////////////////////////////////////////////////
public class KinectController : Singleton<KinectController> 
{
    public bool Ready { get; protected set; }
    public bool KinectInitialized { get; protected set; } //! Initialization flag

    //KinectWrapper SDK constants
    protected const int NO_ERROR = 0; //! Constant value that defines an Kinect SDK error
    protected const int STATE_TRACKED = ( int ) KinectMSWrapper.NuiSkeletonPositionTrackingState.Tracked; //! Constant value of the Kinect SDK Tracking Identifier, stored here for performance
    protected const int STATE_NOT_TRACKED = ( int ) KinectMSWrapper.NuiSkeletonPositionTrackingState.NotTracked; //! Constant value of the Kinect SDK NOT Tracking Identifier, stored here for performance

    public enum Smoothing : int { None, Default, Medium, Aggressive }

    public int SensorAngle { get; set; }    //! Sensor height from the ground, default value is 90
    public int SensorHeight { get; set; }   //! Sensor height from the ground, default estimation is 1

    public Smoothing smoothing = Smoothing.Default; //! Inspector set value to determine smoothing level, actual values set in Setup function

    public bool TwoUsers = false; //! Inspector set value to determine number of users

    public uint Player1ID { get; protected set; } //! ID of player two skeleton in the Kinect Skeleton Data
    public uint Player2ID { get; protected set; } //! ID of player two skeleton in the Kinect Skeleton Data

    protected int Player1Index { get; set; } //! Index of player one skeleton in the Kinect Skeleton list
    protected int Player2Index { get; set; } //! Index of player two skeleton in the Kinect Skeleton list

    protected bool Player1Calibrated = false; //! Calibration flag for player 1
    protected bool Player2Calibrated = false; //! Calibration flag for player 2

    public bool UseBoneOrientationsConstraint = true;   //! Determine if to apply orientation constraints 

    public bool IgnoreInferredJoints = true;    //! Determines to use only the tracked joints ignoring the inferred/estated ones
    public bool DetectClosestUser = true;       //! Public Bool to determine whether to detect only the closest user or not, this is done using hip position
    public float MinUserDistance = 1.0f;        //! Set Minimum user distance
    public float MaxUserDistance = 0f;          //! Maximum user distance, if any. 0 means no max-distance limitation

    protected bool _allPlayersCalibrated = false;

    protected Vector3[] _player1JointsPos; //! Array of all player one joint positions, set in Calibrate
    protected Vector3[] _player2JointsPos; //! Array of all player two joint positions, set in ProcessSkelton
    protected Matrix4x4[] _player1JointsOri; //! Array of all player one joint orientations, set in ProcessPlayerOneOrientations
    protected Matrix4x4[] _player2JointsOri; //! Array of all player two joint orientations, set in ProcessPlayerTwoOrientations
    protected bool[] _player1JointsTracked; //! Array of all bool that represent current tracked joints, set int ProcessPlayerOneOrientations
    protected bool[] _player2JointsTracked; //! Array of all player one joint positions, et int ProcessPlayerTwoOrientations
    protected Vector3 _player1Pos; //! Hip position of player 1
    protected Vector3 _player2Pos; //! Hip position of player 2

    public Matrix4x4 Player1Ori { get; protected set; } //! Property to get the players orientation, assigned in ProcessPlayerOneOrientation
    public Matrix4x4 Player2Ori { get; protected set; } //! Property to get the players orientation, assigned in ProcessPlayerTwoOrientation

    protected bool[] _player1PrevTracked; //! Array that tells the filters which joints comparisons can be conducted upon
    protected bool[] _player2PrevTracked; //! Array that tells the filters which joints comparisons can be conducted upon;

    protected KinectMSWrapper.NuiSkeletonFrame _skeletonFrame;                //! Skeleton data retrieved from the Kinect SDK
    protected KinectMSWrapper.NuiTransformSmoothParameters _smoothParameters; //! Smoothing values sent to the Kinect SDK on initialization

    protected List<uint> _trackedPlayersList; //! List of currently tracked users, users are removed if they leave the Kinect's View scene

    protected Matrix4x4 _kinectToWorld; //! Used to transform Kinect space into World Space
    protected Matrix4x4 _flipMatrix;    //! Used to transform the joint orientation from Kinect Space into world Space (Look to Quaternion functions).

    //Filters
    protected TrackingStateFilter[] _trackingStateFilter; //! TrackingStateFilter filter, see filter documentation for usage
    protected BoneOrientationsConstraint _boneConstraintsFilter; //! BoneOrientationsConstraint filter, see filter documentation for usage

    protected int[] _mustBeTrackedJoints = { 
		(int)KinectMSWrapper.NuiSkeletonPositionIndex.AnkleLeft,
		(int)KinectMSWrapper.NuiSkeletonPositionIndex.FootLeft,
		(int)KinectMSWrapper.NuiSkeletonPositionIndex.AnkleRight,
		(int)KinectMSWrapper.NuiSkeletonPositionIndex.FootRight,
	}; //! Joints that must be traced for skeleton Data to be recorded and filtered

    private float _lastNuiTime; //! Calibration time used in ProcessSkeleton (Time.deltaTime to be tested instead)

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Returns joint position of the specific user
    /////////////////////////////////////////////////
    protected KinectController()
    {
        KinectInitialized = false;
    }

    public Vector3[] GetJointPositions( uint userID )
    {
        if( Ready )
        {
            if ( userID == Player1ID )
            {
                return _player1JointsPos;
            }
            else if ( userID == Player2ID )
            {
                return _player2JointsPos;
            }
            else
            {
                Debug.LogError( "Requesting invalid player data from Kinect" );
            }
        }

        return null;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userId Id of the user
    /// @param join Index position of the joint
    /// @brief Returns joint position of the specific user
    /////////////////////////////////////////////////
    public Vector3 GetJointPosition( uint userId, int joint )
    {
        if( Ready )
        {
            if ( userId == Player1ID )
            {
                return joint >= 0 && joint < _player1JointsPos.Length ? _player1JointsPos[ joint ] : Vector3.zero;

            }
            else if ( userId == Player2ID )
            {
                return joint >= 0 && joint < _player2JointsPos.Length ? _player2JointsPos[ joint ] : Vector3.zero;
            }
            else
            {
                Debug.LogError( "Requesting invalid player data from Kinect" );
            }

            return Vector3.zero;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userId Id of the user
    /// @param join Index position of the joint
    /// @brief Returns the raw unmodified joint position with no filters applied
    ////////////////////////////////////////////////
    public Vector3 GetRawSkeletonJointPos( uint UserId, int joint )
    {
        if ( UserId == Player1ID )
        {
            return joint >= 0 && joint < _player1JointsPos.Length ? ( Vector3 ) _skeletonFrame.SkeletonData[Player1Index].SkeletonPositions[joint] : Vector3.zero;
        }
        else if ( UserId == Player2ID )
        {
            return joint >= 0 && joint < _player2JointsPos.Length ? ( Vector3 ) _skeletonFrame.SkeletonData[Player2Index].SkeletonPositions[joint] : Vector3.zero;
        }

        return Vector3.zero;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userId Id of the user
    /// @brief Returns the user position which is based on the hip location
    ////////////////////////////////////////////////
    public Vector3 GetUserPosition( uint UserId )
    {
        if ( UserId == Player1ID )
        {
            return _player1Pos;
        }
        else if ( UserId == Player2ID )
        {
            return _player2Pos;
        }
        return Vector3.zero;

    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userId Id of the user
    /// @param join Index position of the joint
    /// @param flip Flag that determines if the X Z coordinates need to be flipped on convention
    /// @brief  Returns the joint rotation of the particular join for that user
    /////////////////////////////////////////////////
    public Quaternion GetJointOrientation( uint UserId, int joint, bool flip )
    {
        if (Ready)
        {
            if (UserId == Player1ID)
            {
                if (joint >= 0 && joint < _player1JointsOri.Length && _player1JointsTracked[joint])
                {
                    return Maths.ConvertMatrixToQuat(_player1JointsOri[joint], joint, flip);
                }
            }
            else if (UserId == Player2ID)
            {
                if (joint >= 0 && joint < _player2JointsOri.Length && _player2JointsTracked[joint])
                {
                    return Maths.ConvertMatrixToQuat(_player2JointsOri[joint], joint, flip);
                }
            }
        }

        return Quaternion.identity;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Returns true if at least one user is currently detected by the sensor
    /////////////////////////////////////////////////
    public bool IsUserDetected()
    {
        return KinectInitialized && ( _trackedPlayersList.Count > 0 );
    }


    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Removes the currently detected Kinect users, allowing a new detection/calibration process to start
    /////////////////////////////////////////////////
    public void ClearKinectUsers()
    {
        if ( !KinectInitialized )
            return;

        // remove current users
        for ( int i = _trackedPlayersList.Count - 1; i >= 0; i-- )
        {
            uint userId = _trackedPlayersList[ i ];
            RemoveUser( userId );
        }

        ResetFilters();
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param angle The angle in which to tilt the Kinect
    /// @brief  Set Kinect tilt angle using Kinect SDK
    /////////////////////////////////////////////////
    public bool SetKinectAngle( int angle)
    {
        int initializationIdentifier = NO_ERROR;
        
        try
        {
            initializationIdentifier = KinectMSWrapper.NuiCameraElevationSetAngle( angle );// set Kinect elevation angle

            if( initializationIdentifier != NO_ERROR )
            {
                throw new Exception( "Using Dll wrapper failed" );
            }
        }
        catch ( DllNotFoundException e )
        {
            string message = "Please check the Kinect SDK installation.";
            Debug.LogError( message );
            Debug.LogError( e.ToString() );

            return false;
        }
        catch ( Exception e )
        {
            string message = e.Message + " - " + KinectMSWrapper.GetNuiErrorString( initializationIdentifier );
            Debug.LogError( message );

            return false;
        }

        return true;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Initializes the Kinect SDK using the KinectMSWrpper class, default Kinect SKD flags advice in MSDN set
    /////////////////////////////////////////////////
    protected bool EnableKinect()
    {
        int initializationIdentifier = NO_ERROR;

        try
        {
            initializationIdentifier = KinectMSWrapper.NuiInitialize( KinectMSWrapper.NuiInitializeFlags.UsesSkeleton | KinectMSWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex | 0 );

            if ( initializationIdentifier != NO_ERROR )
            {
                throw new Exception( "NuiInitialize Failed" );
            }

            initializationIdentifier = KinectMSWrapper.NuiSkeletonTrackingEnable( IntPtr.Zero, 8 );  //tracking precision value, move these to const later 0, 12,8
            if ( initializationIdentifier != NO_ERROR )
            {
                throw new Exception( "Cannot initialize Skeleton Data" );
            }
        }
        catch ( DllNotFoundException e )
        {
            string message = "Please check the Kinect SDK installation.";
            Debug.Log( message );
            Debug.Log( e.ToString() );

            return false;
        }
        catch ( Exception e )
        {
            string message = e.Message + " - " + KinectMSWrapper.GetNuiErrorString( initializationIdentifier );
            Debug.Log( message );

            return false;
        }

        return true;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Creates an array of points based on the MSWraper skeleton joint count
    /// returns false if KinectMSWrapper returns no joints to track
    /////////////////////////////////////////////////
    protected bool SetupSkeleton()
    {
        if ( KinectMSWrapper.Constants.NuiSkeletonCount == 0 )
        {
            return false;
        }

        //Init skeleton structures
        _skeletonFrame = new KinectMSWrapper.NuiSkeletonFrame()
        {
            SkeletonData = new KinectMSWrapper.NuiSkeletonData[ KinectMSWrapper.Constants.NuiSkeletonCount ]
        };

        return true;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Set the KinectSDK joint smoothing parameters based on Level
    /////////////////////////////////////////////////
    protected void SetSmoothingParams()
    {
        //Values used to pass to smoothing function
        _smoothParameters = new KinectMSWrapper.NuiTransformSmoothParameters();

        switch ( smoothing )
        {
            case Smoothing.Default:
                _smoothParameters.fSmoothing = 0.5f;
                _smoothParameters.fCorrection = 0.5f;
                _smoothParameters.fPrediction = 0.5f;
                _smoothParameters.fJitterRadius = 0.05f;
                _smoothParameters.fMaxDeviationRadius = 0.04f;
                break;
            case Smoothing.Medium:
                _smoothParameters.fSmoothing = 0.5f;
                _smoothParameters.fCorrection = 0.1f;
                _smoothParameters.fPrediction = 0.5f;
                _smoothParameters.fJitterRadius = 0.1f;
                _smoothParameters.fMaxDeviationRadius = 0.1f;
                break;
            case Smoothing.Aggressive:
                _smoothParameters.fSmoothing = 0.7f;
                _smoothParameters.fCorrection = 0.3f;
                _smoothParameters.fPrediction = 1.0f;
                _smoothParameters.fJitterRadius = 1.0f;
                _smoothParameters.fMaxDeviationRadius = 1.0f;
                break;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Sets up the tracking, orientations and clipping filters
    /////////////////////////////////////////////////
    protected void SetupConstraintsAndFilters()
    {
        //Init the tracking state filter
        _trackingStateFilter = new TrackingStateFilter[ KinectMSWrapper.Constants.NuiSkeletonMaxTracked ];
        for ( int i = 0; i < _trackingStateFilter.Length; i++ )
        {
            _trackingStateFilter[ i ] = new TrackingStateFilter();
            _trackingStateFilter[ i ].Init();
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Initializes the Kinect SDK DLL using KinectMSWrapper class and sets up player matrix's
    /////////////////////////////////////////////////
    protected void BuildPlayers()
    {
        // create arrays for joint positions and joint orientations
        _player1JointsTracked = new bool[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];
        _player2JointsTracked = new bool[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];
        _player1PrevTracked = new bool[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];
        _player2PrevTracked = new bool[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];

        _player1JointsPos = new Vector3[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];
        _player2JointsPos = new Vector3[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];

        _player1JointsOri = new Matrix4x4[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];
        _player2JointsOri = new Matrix4x4[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count ];

        _trackedPlayersList = new List<uint>();
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Using the sensor angle create transformation matrix to straighten join positions and align join orientations 
    ////////////////////////////////////////////////
    protected void BuildKinectToWorldTransformationMatrixes()
    {
        //Create the transform matrix that converts from Kinect space to world-space
        Quaternion quatTiltAngle = new Quaternion();
        quatTiltAngle.eulerAngles = new Vector3( -SensorAngle, 0.0f, 0.0f );

        //Transform matrix Kinect to world
        _kinectToWorld.SetTRS( new Vector3( 0.0f, SensorHeight, 0.0f ), quatTiltAngle, Vector3.one );
        _flipMatrix = Matrix4x4.identity;
        _flipMatrix[ 2, 2 ] = -1;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Calls required setup functions
    /////////////////////////////////////////////////
    public void Setup()
    {
        if ( !KinectInitialized  )
        {
            if ( !EnableKinect() )
            {
                return;
            }

            if ( !SetKinectAngle( SensorAngle ) )
            {
                return;
            }

            if ( !SetupSkeleton() )
            {
                return;
            }

            SetSmoothingParams();

            SetupConstraintsAndFilters();

            BuildPlayers();

            BuildKinectToWorldTransformationMatrixes();

            Debug.Log( "Waiting for users" );
            Ready = false;

            KinectInitialized = true;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// Description: Retrieves raw skeleton data from Kinect SDK and calls ProcessSkeleton function
    /////////////////////////////////////////////////
    public void Update()
    {
        if ( KinectInitialized )
        {
            if ( KinectMSWrapper.PollSkeleton( ref _smoothParameters, ref _skeletonFrame ) )
            {
                ProcessSkeleton();
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Returns false if no skeletons can be tracked, 
    /// If closest user check is true then the skeleton with the closest z position is used
    /// CalibrateUser is called with the chosen ID and skeleton data
    /////////////////////////////////////////////////
    protected void ProcessUserCalibration( uint userID, ref KinectMSWrapper.NuiSkeletonData skeletonData, int skelitonID, ref Vector3 skeletonPos )
    {
        //Get the skeleton position
        if ( !_allPlayersCalibrated )
        {
            // check if this is the closest user
            bool bClosestUser = true;

            if ( DetectClosestUser )
            {
                for ( int j = 0; j < KinectMSWrapper.Constants.NuiSkeletonCount; j++ )
                {
                    if ( j != skelitonID )
                    {
                        KinectMSWrapper.NuiSkeletonData skeletonDataOther = _skeletonFrame.SkeletonData[ j ];

                        if ( ( skeletonDataOther.eTrackingState == KinectMSWrapper.NuiSkeletonTrackingState.SkeletonTracked ) &&
                            ( Mathf.Abs( _kinectToWorld.MultiplyPoint3x4( skeletonDataOther.Position ).z ) < Mathf.Abs( skeletonPos.z ) ) )
                        {
                            bClosestUser = false;
                            break;
                        }
                    }
                }
            }

            if ( bClosestUser )
            {
                CalibrateUser( userID, skelitonID + 1, ref skeletonData );
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userID ID of the user
    /// @param skeletonPos A reference to the skeleton position (Hip)
    /// @param skeletonIndex The index of the passed skeleton within the Kinect Skeleton list
    /// @param skeletonData A reference to the skeleton data
    /// @param skeletonData A reference to the skeleton data 
    /// @param deltaNuiTime Real world time since last update
    /// @brief Calls ProcessPlayerOneJointOrientations or ProcessPlayerTwoJointOrientations depending on the user id
    /////////////////////////////////////////////////
    protected void ProcessJointOrientations( uint userID, ref Vector3 skeletonPos, int skeletonIndex, ref KinectMSWrapper.NuiSkeletonData skeletonData, float deltaNuiTime )
    {
        if ( userID == Player1ID )
        {
            ProcessPlayerOneJointOrientations( userID, ref skeletonPos, skeletonIndex, ref skeletonData, deltaNuiTime );
        }
        else
        {
            ProcessPlayerTwoJointOrientations( userID, ref skeletonPos, skeletonIndex, ref skeletonData, deltaNuiTime );
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userID ID of the user
    /// @param skeletonPos A reference to the skeleton position (Hip)
    /// @param skeletonIndex The index of the passed skeleton within the Kinect Skeleton list
    /// @param skeletonData A reference to the skeleton data
    /// @param skeletonData A reference to the skeleton data 
    /// @param deltaNuiTime Real world time since last update
    /// @brief Applies filters to Joint orientations and positions, then stores the data in individual position and orientation arrays
    //////////////////////////////////////////////////
    protected void ProcessPlayerOneJointOrientations( uint userID, ref Vector3 skeletonPos, int skeletonIndex, ref KinectMSWrapper.NuiSkeletonData skeletonData, float deltaNuiTime )
    {
        if ( Mathf.Abs( skeletonPos.z ) >= MinUserDistance && ( MaxUserDistance <= 0f || Mathf.Abs( skeletonPos.z ) <= MaxUserDistance ) )
        {
            Player1Index = skeletonIndex;

            //Get player position
            _player1Pos = skeletonPos;

            //Apply tracking state filter first
            _trackingStateFilter[ 0 ].UpdateFilter( ref skeletonData );

            //Get joints position and rotation
            for ( int j = 0; j < ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count; j++ )
            {
                bool playerTracked = IgnoreInferredJoints ? ( int ) skeletonData.eSkeletonPositionTrackingState[ j ] == STATE_TRACKED :
                    ( Array.BinarySearch( _mustBeTrackedJoints, j ) >= 0 ? ( int ) skeletonData.eSkeletonPositionTrackingState[ j ] == STATE_TRACKED :
                    ( int ) skeletonData.eSkeletonPositionTrackingState[ j ] != STATE_NOT_TRACKED );

                _player1JointsTracked[ j ] = ( _player1PrevTracked[ j ] && playerTracked );
                _player1PrevTracked[ j ] = playerTracked;

                if ( _player1JointsTracked[ j ] )
                {
                    _player1JointsPos[ j ] = _kinectToWorld.MultiplyPoint3x4( skeletonData.SkeletonPositions[ j ] );
                }
            }

            //Calculate joint orientations using the Kinect SDK
            KinectMSWrapper.GetSkeletonJointOrientation( ref _player1JointsPos, ref _player1JointsTracked, ref _player1JointsOri );

            //Filter orientation constraints
            if ( UseBoneOrientationsConstraint && _boneConstraintsFilter != null )
            {
                _boneConstraintsFilter.Constrain( ref _player1JointsOri, ref _player1JointsTracked );
            }

            //Get player rotation
            Player1Ori = _player1JointsOri[ ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter ];
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param userID ID of the user
    /// @param skeletonPos A reference to the skeleton position (Hip)
    /// @param skeletonIndex The index of the passed skeleton within the Kinect Skeleton list
    /// @param skeletonData A reference to the skeleton data
    /// @param skeletonData A reference to the skeleton data 
    /// @param deltaNuiTime Real world time since last update
    /// @brief Applies filters to Joint orientations and positions, then stores the data in individual position and orientation arrays
    //////////////////////////////////////////////////
    protected void ProcessPlayerTwoJointOrientations( uint userID, ref Vector3 skeletonPos, int skeletonIndex, ref KinectMSWrapper.NuiSkeletonData skeletonData, float deltaNuiTime )
    {
        if ( Mathf.Abs( skeletonPos.z ) >= MinUserDistance && ( MaxUserDistance <= 0f || Mathf.Abs( skeletonPos.z ) <= MaxUserDistance ) )
	    {
            Player2Index = skeletonIndex;
            _player2Pos = skeletonPos;

            //Apply tracking state filter first
            _trackingStateFilter[1].UpdateFilter( ref skeletonData );

            //Get joints' position and rotation
            for ( int j = 0; j < ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count; j++ )
            {
                bool playerTracked = IgnoreInferredJoints ? ( int ) skeletonData.eSkeletonPositionTrackingState[j] == STATE_TRACKED :
                    ( Array.BinarySearch( _mustBeTrackedJoints, j ) >= 0 ? ( int ) skeletonData.eSkeletonPositionTrackingState[j] == STATE_TRACKED :
                    ( int ) skeletonData.eSkeletonPositionTrackingState[j] != STATE_NOT_TRACKED );
                _player2JointsTracked[j] = _player2PrevTracked[j] && playerTracked;
                _player2PrevTracked[j] = playerTracked;

                if ( _player2JointsTracked[j] )
                {
                    _player2JointsPos[j] = _kinectToWorld.MultiplyPoint3x4( skeletonData.SkeletonPositions[j] );
                }
            }

            //Calculate joint orientations
            KinectMSWrapper.GetSkeletonJointOrientation( ref _player2JointsPos, ref _player2JointsTracked, ref _player2JointsOri );

            //Filter orientation constraints
            if ( UseBoneOrientationsConstraint && _boneConstraintsFilter != null )
            {
                _boneConstraintsFilter.Constrain( ref _player2JointsOri, ref _player2JointsTracked );
            }

            //Get player rotation
            Player2Ori = _player2JointsOri[( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter];
	    }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  For both player 1 and 2 
    /////////////////////////////////////////////////
    protected void ProcessSkeleton()
    {
        List<uint> lostUsers = new List<uint>();
        lostUsers.AddRange( _trackedPlayersList );

        // calculate the time since last update
        float currentNuiTime = Time.realtimeSinceStartup;
        float deltaNuiTime = currentNuiTime - _lastNuiTime;

        for ( int i = 0; i < KinectMSWrapper.Constants.NuiSkeletonCount; i++ )
        {
            KinectMSWrapper.NuiSkeletonData skeletonData = _skeletonFrame.SkeletonData[ i ];
            uint userId = skeletonData.dwTrackingID;

            Vector3 skeletonPos = _kinectToWorld.MultiplyPoint3x4( skeletonData.Position );

            if ( skeletonData.eTrackingState == KinectMSWrapper.NuiSkeletonTrackingState.SkeletonTracked )
            {
                ProcessUserCalibration( userId, ref skeletonData, i, ref skeletonPos );
                ProcessJointOrientations( userId, ref skeletonPos , i, ref skeletonData, deltaNuiTime );

                lostUsers.Remove( userId );
            }
        }

        _lastNuiTime = currentNuiTime;

        // remove the lost users if any
        if ( lostUsers.Count > 0 )
        {
            foreach ( uint userId in lostUsers )
            {
                Debug.Log( "Removed User" );
                RemoveUser( userId );
            }

            lostUsers.Clear();
        }
    }
    

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param ID of the user to remove
    /// @brief  Remove user from user list and reset Player Attributes
    /////////////////////////////////////////////////
    public void RemoveUser( uint userId )
    {
        if ( userId == Player1ID )
        {
            Player1ID = 0;
            Player1Index = 0;
            Player1Calibrated = false;
        }

        if ( userId == Player2ID )
        {
            Player2ID = 0;
            Player2Index = 0;
            Player2Calibrated = false;
        }

        //Remove from user list
        _trackedPlayersList.Remove( userId );
        _allPlayersCalibrated = !TwoUsers ? _trackedPlayersList.Count >= 1 : _trackedPlayersList.Count >= 2; // false;

        Debug.Log( "Waiting for users......" );

        //If no users we are not ready
        if ( Player1ID == 0 && Player2ID == 0 )
        {
            Ready = false;
        }
        else
        {
            Ready = true;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Remove user from user list and reset Player Attributes
    /////////////////////////////////////////////////
    public void ResetFilters()
    {
        if ( !KinectInitialized )
        {
            return;
        }

        //Clear Kinect data
        _player1Pos = Vector3.zero; 
        _player2Pos = Vector3.zero;
        Player1Ori = Matrix4x4.identity; 
        Player2Ori = Matrix4x4.identity;

        //Reset filters
        int skeletonJointsCount = ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count;
        for ( int i = 0; i < skeletonJointsCount; i++ )
        {
            _player1JointsTracked[i] = false; _player2JointsTracked[i] = false;
            _player1PrevTracked[i] = false; _player2PrevTracked[i] = false;
            _player1JointsPos[i] = Vector3.zero; _player2JointsPos[i] = Vector3.zero;
            _player1JointsOri[i] = Matrix4x4.identity; _player2JointsOri[i] = Matrix4x4.identity;
        }

        if ( _trackingStateFilter != null )
        {
            for ( int i = 0; i < _trackingStateFilter.Length; i++ )
            {
                if ( _trackingStateFilter[ i ] != null )
                {
                    _trackingStateFilter[ i ].Reset();
                }
            }
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  For one or Two players check if user is in selected calibration pose, if they are add player to user list and set up player attributes
    ////////////////////////////////////////////////
    public void CalibrateUser( uint UserId, int UserIndex, ref KinectMSWrapper.NuiSkeletonData skeletonData )
    {
        //If player 1 hasn't been calibrated, assign that UserID to it.
        if ( !Player1Calibrated )
        {
            //Check to make sure we don't accidentally assign player 2 to player 1.
            if ( !_trackedPlayersList.Contains( UserId ) )
            {
                //if ( CheckForCalibrationPose( UserId, ref Player1CalibrationPose, ref _player1CalibrationData, ref skeletonData ) )
                if ( CheckForCalibrationPose( UserId ) )
                {
                    Player1Calibrated = true;
                    Player1ID = UserId;
                    Player1Index = UserIndex;

                    _trackedPlayersList.Add( UserId );

                    //Reset skeleton filters
                    ResetFilters();

                    //If we're not using 2 users, we're all calibrated.
                    _allPlayersCalibrated = !TwoUsers ? _trackedPlayersList.Count >= 1 : _trackedPlayersList.Count >= 2; // true;
                }
            }
        }
        // Otherwise, assign to player 2.
        else if ( TwoUsers && !Player2Calibrated )
        {
            if ( !_trackedPlayersList.Contains( UserId ) )
            {
                //if ( CheckForCalibrationPose( UserId, ref Player2CalibrationPose, ref _player2CalibrationData, ref skeletonData ) )
                if ( CheckForCalibrationPose( UserId ) )
                {
                    Player2Calibrated = true;
                    Player2ID = UserId;
                    Player2Index = UserIndex;

                    _trackedPlayersList.Add( UserId );

                    //Reset skeleton filters
                    ResetFilters();

                    //All users are calibrated!
                    _allPlayersCalibrated = !TwoUsers ? _trackedPlayersList.Count >= 1 : _trackedPlayersList.Count >= 2; // true;
                }
            }
        }

        //If all users are calibrated, stop trying to find them.
        if ( _allPlayersCalibrated )
        {
            Debug.Log( "User Detected" );
            Ready = true;
        }
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Returns the direction between baseJoint and nextJoint, for the specified user
    ////////////////////////////////////////////////
    public Vector3 GetDirectionBetweenJoints( uint UserId, int baseJoint, int nextJoint, bool flipX, bool flipZ )
    {
        Vector3 jointDir = Vector3.zero;

        if ( UserId == Player1ID )
        {
            if ( baseJoint >= 0 && baseJoint < _player1JointsPos.Length && _player1JointsTracked[ baseJoint ] &&
                nextJoint >= 0 && nextJoint < _player1JointsPos.Length && _player1JointsTracked[ nextJoint ] )
            {
                jointDir = _player1JointsPos[ nextJoint ] - _player1JointsPos[ baseJoint ];
            }
        }
        else if ( UserId == Player2ID )
        {
            if ( baseJoint >= 0 && baseJoint < _player2JointsPos.Length && _player2JointsTracked[ baseJoint ] &&
                nextJoint >= 0 && nextJoint < _player2JointsPos.Length && _player2JointsTracked[ nextJoint ] )
            {
                jointDir = _player2JointsPos[ nextJoint ] - _player2JointsPos[ baseJoint ];
            }
        }

        if ( jointDir != Vector3.zero )
        {
            if ( flipX )
                jointDir.x = -jointDir.x;

            if ( flipZ )
                jointDir.z = -jointDir.z;
        }

        return jointDir;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief  Check for the assigned calibration pose
    ///////////////////////////////////////////////
    protected bool CheckForCalibrationPose( uint userId )
    {
        return true;
    }


    //private bool CheckForCalibrationPose( uint userId, ref KinectGestures.Gestures calibrationGesture, ref KinectGestures.GestureData gestureData, ref KinectMSWrapper.NuiSkeletonData skeletonData )
    //{
    //    if ( calibrationGesture == KinectGestures.Gestures.None )
    //        return true;

    //    // init gesture data if needed
    //    if ( gestureData.userId != userId )
    //    {
    //        gestureData.userId = userId;
    //        gestureData.gesture = calibrationGesture;
    //        gestureData.state = 0;
    //        gestureData.joint = 0;
    //        gestureData.progress = 0f;
    //        gestureData.complete = false;
    //        gestureData.cancelled = false;
    //    }

    //    // get temporary joints' position
    //    int skeletonJointsCount = ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.Count;
    //    bool[] jointsTracked = new bool[skeletonJointsCount];
    //    Vector3[] jointsPos = new Vector3[skeletonJointsCount];

    //    int STATE_TRACKED = ( int ) KinectMSWrapper.NuiSkeletonPositionTrackingState.Tracked;
    //    int STATE_NOT_TRACKED = ( int ) KinectMSWrapper.NuiSkeletonPositionTrackingState.NotTracked;

    //    int[] _mustBeTrackedJoints = { 
    //        ( int )KinectMSWrapper.NuiSkeletonPositionIndex.AnkleLeft,
    //        ( int )KinectMSWrapper.NuiSkeletonPositionIndex.FootLeft,
    //        ( int )KinectMSWrapper.NuiSkeletonPositionIndex.AnkleRight,
    //        ( int )KinectMSWrapper.NuiSkeletonPositionIndex.FootRight,
    //    };

    //    for ( int j = 0; j < skeletonJointsCount; j++ )
    //    {
    //        jointsTracked[j] = Array.BinarySearch( _mustBeTrackedJoints, j ) >= 0 ? ( int ) skeletonData.eSkeletonPositionTrackingState[j] == STATE_TRACKED :
    //            ( int ) skeletonData.eSkeletonPositionTrackingState[j] != STATE_NOT_TRACKED;

    //        if ( jointsTracked[j] )
    //        {
    //            jointsPos[j] = _kinectToWorld.MultiplyPoint3x4( skeletonData.SkeletonPositions[j] );
    //        }
    //    }

    //    // estimate the gesture progress
    //    KinectGestures.CheckForGesture( userId, ref gestureData, Time.realtimeSinceStartup,
    //        ref jointsPos, ref jointsTracked );

    //    // check if gesture is complete
    //    if ( gestureData.complete )
    //    {
    //        gestureData.userId = 0;
    //        return true;
    //    }

    //    return false;
    //}
}