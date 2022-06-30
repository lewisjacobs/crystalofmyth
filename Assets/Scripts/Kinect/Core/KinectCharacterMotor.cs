using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinectCharacterMotor : MonoBehaviour
{
    public int User = 1; //! User of Kinect skeleton to animate from
    public bool mirroredMovement = false; //! Bool that has the characters (facing the player) actions become mirrored. Default false.
    public bool verticalMovement = false; //! Bool that determines whether the avatar is allowed to move in vertical direction.
    public float smoothFactor = 5f;       //! Slerp smooth factor
    public bool offsetRelativeToSensor = false; //! Whether the offset node must be repositioned to the user's coordinates, as reported by the sensor or not.

    protected Transform bodyRoot;   //! The body root node
    protected GameObject offsetNode;//! A required variable if you want to rotate the model in space.
    protected int moveRate = 1;     //! Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
    protected Transform[] bones;             //! Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected Quaternion[] initialRotations; //! Rotations of the bones when the Kinect tracking starts.
    protected Quaternion[] initialLocalRotations; //! Cash for initial local rotations of model
    protected Vector3 initialPosition;    //! Initial position and rotation of the transform
    protected Quaternion initialRotation; //! Cash for initial rotations from model
    protected bool offsetCalibrated = false;     //! Calibration Offset Variables for Character Position.
    protected float xOffset; 
    protected float yOffset;
    protected float zOffset;
    Transform _transformCache; //! Reference to local transform 

    public new Transform transform
    {
        get
        {
            if ( !_transformCache )
                _transformCache = base.transform;

            return _transformCache;
        }
    }

    public void Awake()
    {
        // check for double start
        if ( bones != null )
            return;

        // inits the bones array
        bones = new Transform[ 22 ];

        // Initial rotations and directions of the bones.
        initialRotations = new Quaternion[ bones.Length ];
        initialLocalRotations = new Quaternion[ bones.Length ];

        // Map bones to the points the Kinect tracks
        MapBones();

        // Get initial bone rotations
        GetInitialRotations();
    }

    public void Update()
    {
        if ( User == 1 )
        {
            UpdateAvatar( KinectController.Instance.Player1ID );
        }
        else
        {
            UpdateAvatar( KinectController.Instance.Player2ID );
        }
    }

    // Update the avatar each frame.
    public void UpdateAvatar( uint UserID )
    {
        if ( !transform.gameObject.activeInHierarchy )
            return;

        // move the avatar to its Kinect position
        MoveAvatar( UserID );

        for ( var boneIndex = 0; boneIndex < bones.Length; boneIndex++ )
        {
            if ( !bones[ boneIndex ] )
                continue;

            if ( boneIndex2JointMap.ContainsKey( boneIndex ) )
            {
                KinectMSWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[ boneIndex ] : boneIndex2MirrorJointMap[ boneIndex ];
                TransformBone( UserID, joint, boneIndex, !mirroredMovement );
            }
            else if ( specIndex2JointMap.ContainsKey( boneIndex ) )
            {
                // special bones (clavicles)
                List<KinectMSWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[ boneIndex ] : specIndex2MirrorJointMap[ boneIndex ];

                if ( alJoints.Count >= 2 )
                {
                    //Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    //TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }
    }

    // Set bones to their initial positions and rotations
    public void ResetToInitialPosition()
    {
        if ( bones == null )
            return;

        if ( offsetNode != null )
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // For each bone that was defined, reset to initial position.
        for ( int i = 0; i < bones.Length; i++ )
        {
            if ( bones[ i ] != null )
            {
                bones[ i ].rotation = initialRotations[ i ];
            }
        }

        if ( bodyRoot != null )
        {
            bodyRoot.localPosition = Vector3.zero;
            bodyRoot.localRotation = Quaternion.identity;
        }

        // Restore the offset's position and rotation
        if ( offsetNode != null )
        {
            offsetNode.transform.position = initialPosition;
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    // Invoked on the successful calibration of a player.
    public void SuccessfulCalibration( uint userId )
    {
        // reset the models position
        if ( offsetNode != null )
        {
            offsetNode.transform.rotation = initialRotation;
        }

        // re-calibrate the position offset
        offsetCalibrated = false;
    }

    // Apply the rotations tracked by kinect to the joints.
    protected void TransformBone( uint userId, KinectMSWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip )
    {
        Transform boneTransform = bones[ boneIndex ];
        if ( boneTransform == null || KinectController.Instance == null )
            return;

        int iJoint = ( int ) joint;
        if ( iJoint < 0 )
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = KinectController.Instance.GetJointOrientation( userId, iJoint, flip );
        if ( jointRotation == Quaternion.identity )
            return;

        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot( jointRotation, boneIndex );

        if ( smoothFactor != 0f )
            boneTransform.rotation = Quaternion.Slerp( boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime );
        else
            boneTransform.rotation = newRotation;
    }

    // Apply the rotations tracked by kinect to a special joint
    protected void TransformSpecialBone( uint userId, KinectMSWrapper.NuiSkeletonPositionIndex joint, KinectMSWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip )
    {
        Transform boneTransform = bones[ boneIndex ];
        if ( boneTransform == null || KinectController.Instance == null )
            return;

        //if ( !KinectController.Instance.IsJointTracked( userId, ( int ) joint ) ||
        //   !KinectController.Instance.IsJointTracked( userId, ( int ) jointParent ) )
        //{
        //    return;
        //}

        Vector3 jointDir = KinectController.Instance.GetDirectionBetweenJoints( userId, ( int ) jointParent, ( int ) joint, false, true );
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation( baseDir, jointDir ) : Quaternion.identity;

        //		if(!flip)
        //		{
        //			Vector3 mirroredAngles = jointRotation.eulerAngles;
        //			mirroredAngles.y = -mirroredAngles.y;
        //			mirroredAngles.z = -mirroredAngles.z;
        //			
        //			jointRotation = Quaternion.Euler(mirroredAngles);
        //		}

        if ( jointRotation != Quaternion.identity )
        {
            // Smoothly transition to the new rotation
            Quaternion newRotation = Kinect2AvatarRot( jointRotation, boneIndex );

            if ( smoothFactor != 0f )
                boneTransform.rotation = Quaternion.Slerp( boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime );
            else
                boneTransform.rotation = newRotation;
        }

    }

    // Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
    // Only pulls positional, not rotational.
    protected void MoveAvatar( uint UserID )
    {
        if ( bodyRoot == null || KinectController.Instance == null )
            return;
        //if ( !KinectController.Instance.IsJointTracked( UserID, ( int ) KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter ) )
        //    return;

        // Get the position of the body and store it.
        Vector3 trans = this.gameObject.transform.position; //KinectController.Instance.GetUserPosition( UserID );

        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if ( !offsetCalibrated )
        {
            offsetCalibrated = true;

            xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
            yOffset = trans.y * moveRate;
            zOffset = -trans.z * moveRate;

            if ( offsetRelativeToSensor )
            {
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = ( offsetNode != null ? offsetNode.transform.position.y : transform.position.y ) - cameraPos.y;
                Vector3 relativePos = new Vector3( trans.x * moveRate, yRelToAvatar, trans.z * moveRate );
                Vector3 offsetPos = cameraPos + relativePos;

                if ( offsetNode != null )
                {
                    offsetNode.transform.position = offsetPos;
                }
                else
                {
                    transform.position = offsetPos;
                }
            }
        }

        // Smoothly transition to the new position
        Vector3 targetPos = Kinect2AvatarPos( trans, verticalMovement );

        if ( smoothFactor != 0f )
            bodyRoot.localPosition = Vector3.Lerp( bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime );
        else
            bodyRoot.localPosition = targetPos;
    }

    // If the bones to be mapped have been declared, map that bone to the model.
    protected virtual void MapBones()
    {
        // make OffsetNode as a parent of model transform.
        offsetNode = new GameObject( name + "Ctrl" ) { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;

        transform.parent = offsetNode.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // take model transform as body root
        bodyRoot = transform;

        // get bone transforms from the animator component
        var animatorComponent = GetComponent<Animator>();

        for ( int boneIndex = 0; boneIndex < bones.Length; boneIndex++ )
        {
            if ( !boneIndex2MecanimMap.ContainsKey( boneIndex ) )
                continue;

            bones[ boneIndex ] = animatorComponent.GetBoneTransform( boneIndex2MecanimMap[ boneIndex ] );
        }
    }

    // Capture the initial rotations of the bones
    protected void GetInitialRotations()
    {
        // save the initial rotation
        if ( offsetNode != null )
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            transform.rotation = Quaternion.identity;
        }

        for ( int i = 0; i < bones.Length; i++ )
        {
            if ( bones[ i ] != null )
            {
                initialRotations[ i ] = bones[ i ].rotation; // * Quaternion.Inverse(initialRotation);
                initialLocalRotations[ i ] = bones[ i ].localRotation;
            }
        }

        // Restore the initial rotation
        if ( offsetNode != null )
        {
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }

    // Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
    protected Quaternion Kinect2AvatarRot( Quaternion jointRotation, int boneIndex )
    {
        // Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[ boneIndex ];

        //If an offset node is specified, combine the transform with its
        //orientation to essentially make the skeleton relative to the node
        if ( offsetNode != null )
        {
            // Grab the total rotation by adding the Euler and offset's Euler.
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            // Grab our new rotation.
            newRotation = Quaternion.Euler( totalRotation );
        }

        return newRotation;
    }

    // Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
    protected Vector3 Kinect2AvatarPos( Vector3 jointPosition, bool bMoveVertically )
    {
        float xPos;
        float yPos;
        float zPos;

        // If movement is mirrored, reverse it.
        if ( !mirroredMovement )
            xPos = jointPosition.x * moveRate - xOffset;
        else
            xPos = -jointPosition.x * moveRate - xOffset;

        yPos = jointPosition.y * moveRate - yOffset;
        zPos = -jointPosition.z * moveRate - zOffset;

        // If we are tracking vertical movement, update the y. Otherwise leave it alone.
        Vector3 avatarJointPos = new Vector3( xPos, bMoveVertically ? yPos : 0f, zPos );

        return avatarJointPos;
    }

    // dictionaries to speed up bones' processing
    // the author of the terrific idea for kinect-joints to mecanim-bones mapping
    // along with its initial implementation, including following dictionary is
    // Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
		{2, HumanBodyBones.Neck},
		{3, HumanBodyBones.Head},
		
		{4, HumanBodyBones.LeftShoulder},
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		{8, HumanBodyBones.LeftIndexProximal},

		{9, HumanBodyBones.RightShoulder},
		{10, HumanBodyBones.RightUpperArm},
		{11, HumanBodyBones.RightLowerArm},
		{12, HumanBodyBones.RightHand},
		{13, HumanBodyBones.RightIndexProximal},

		{14, HumanBodyBones.LeftUpperLeg},
		{15, HumanBodyBones.LeftLowerLeg},
		{16, HumanBodyBones.LeftFoot},
		{17, HumanBodyBones.LeftToes},
		
		{18, HumanBodyBones.RightUpperLeg},
		{19, HumanBodyBones.RightLowerLeg},
		{20, HumanBodyBones.RightFoot},
		{21, HumanBodyBones.RightToes},
	};

    protected readonly Dictionary<int, KinectMSWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectMSWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectMSWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectMSWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{6, KinectMSWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{7, KinectMSWrapper.NuiSkeletonPositionIndex.WristLeft},
		{8, KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{10, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{11, KinectMSWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{12, KinectMSWrapper.NuiSkeletonPositionIndex.WristRight},
		{13, KinectMSWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{14, KinectMSWrapper.NuiSkeletonPositionIndex.HipLeft},
		{15, KinectMSWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{16, KinectMSWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{17, KinectMSWrapper.NuiSkeletonPositionIndex.FootLeft},
		
		{18, KinectMSWrapper.NuiSkeletonPositionIndex.HipRight},
		{19, KinectMSWrapper.NuiSkeletonPositionIndex.KneeRight},
		{20, KinectMSWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{21, KinectMSWrapper.NuiSkeletonPositionIndex.FootRight},
	};

    protected readonly Dictionary<int, List<KinectMSWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectMSWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectMSWrapper.NuiSkeletonPositionIndex> {KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectMSWrapper.NuiSkeletonPositionIndex> {KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};

    protected readonly Dictionary<int, KinectMSWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectMSWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectMSWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectMSWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectMSWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{6, KinectMSWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{7, KinectMSWrapper.NuiSkeletonPositionIndex.WristRight},
		{8, KinectMSWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{10, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{11, KinectMSWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{12, KinectMSWrapper.NuiSkeletonPositionIndex.WristLeft},
		{13, KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{14, KinectMSWrapper.NuiSkeletonPositionIndex.HipRight},
		{15, KinectMSWrapper.NuiSkeletonPositionIndex.KneeRight},
		{16, KinectMSWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{17, KinectMSWrapper.NuiSkeletonPositionIndex.FootRight},
		
		{18, KinectMSWrapper.NuiSkeletonPositionIndex.HipLeft},
		{19, KinectMSWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{20, KinectMSWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{21, KinectMSWrapper.NuiSkeletonPositionIndex.FootLeft},
	};

    protected readonly Dictionary<int, List<KinectMSWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectMSWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectMSWrapper.NuiSkeletonPositionIndex> {KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectMSWrapper.NuiSkeletonPositionIndex> {KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
}
