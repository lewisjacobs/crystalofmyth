using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ActionRelations
{
    ABOVE,
    BELlOW,
    INFRONT,
    BEHIND,
    TO_THE_RIGHT_OF,
    TO_THE_LEFT_OF
}

public struct ActionRelationContraint
{
    public ActionRelations Relation;
    public float ByAtLeast;
    public float ByAtLeastPersentage;
    public float ButNotFurtherThan;
    public float ButNotFurtherThanPersentage;
}

/////////////////////////////////////////////////
/// @class KinectAction
/// @brief A KinectAction is a single part of a gesture. An action is defined as a comparison between two different Body parts
///////////////////////////////////////////////
public class KinectAction 
{
    protected KinectMSWrapper.NuiSkeletonPositionIndex _PrimaryJoint; //! Primary body part 
    public KinectMSWrapper.NuiSkeletonPositionIndex PrimaryJoint //! Primary body part Property
    {
        get 
        {
            return _PrimaryJoint; 
        }
        set
        {
            _PrimaryJoint = value;
            PrimaryJointIndex = ( int ) value;
        }
    }

    public KinectMSWrapper.NuiSkeletonPositionIndex _RelationalJoint; //! Body part to compare against
    public KinectMSWrapper.NuiSkeletonPositionIndex RelationalJoint
    {
        get
        {
            return _RelationalJoint;
        }
        set
        {
            _RelationalJoint = value;
            RelationalJointIndex = ( int ) value;
        }
    }

    public float TimeOut { get; set; } //! Length of time the action can stay active
    protected float ActivationTime { get; set; } //! Time in which the action was activated
    protected int PrimaryJointIndex { get; set; } //! Index of the body part in the Kinect array
    protected int RelationalJointIndex { get; set; } //! Index of the body part in the Kinect array}

    public List<ActionRelationContraint> Relationships { get; private set; } // List of relationship/constraints to compare the body parts against 

    protected bool _IsActive = false; //! Flag to say if this action is the active action

    public KinectAction()
    {
        Relationships = new List<ActionRelationContraint>();
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param contraint Pre built contraint to be added to the action
    /// @brief Adds a relationship to the Relationships list
    ////////////////////////////////////////////////
    public void AddRelationship( ActionRelationContraint contraint )
    {
        Relationships.Add( contraint );
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param index Index of relationship in the Relationship list
    /// @brief Returns a relationship from the Relationship list
    ////////////////////////////////////////////////
    public ActionRelationContraint GetRelationship( int index )
    {
        return Relationships[ index ];
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param index Index of relationship to replace in the Relationships list
    /// @param contraint New relationship to replace to old one with
    /// @brief Returns a relationship from the Relationship list
    ////////////////////////////////////////////////
    public void UpdateRelationship( int index, ActionRelationContraint contraint )
    {
        Relationships[ index ] = contraint;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets the Activation time to current game time, and sets the IsActive flag to true
    ////////////////////////////////////////////////
    public void Activate()
    {
        ActivationTime = GameController.Instance.ElapsedTime;
        _IsActive = true;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Sets the IsActive flag to false
    ////////////////////////////////////////////////
    public void Deactivate()
    {
        _IsActive = false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @brief Compares the Elapsed time against the time the gesture has been active, returns true if greater and detractive the gesture
    ////////////////////////////////////////////////
    public bool TimedOut()
    {
        if( GameController.Instance.ElapsedTime > ( ActivationTime + TimeOut ) )
        {
            _IsActive = false;
            return true;
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @brief Returns true if all relationship have been met
    ////////////////////////////////////////////////
    public bool CheckCompletion( ref Vector3[] jointPositions )
    {
        bool succes = true;

        if ( _IsActive )
        {
            foreach ( ActionRelationContraint relation in Relationships )
            {
                if ( succes )
                {
                    switch ( relation.Relation )
                    {
                        case ActionRelations.ABOVE: succes = Above( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        case ActionRelations.BELlOW: succes = Bellow( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        case ActionRelations.TO_THE_LEFT_OF: succes = Left( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        case ActionRelations.TO_THE_RIGHT_OF: succes = Right( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        case ActionRelations.INFRONT: succes = Infront( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        case ActionRelations.BEHIND: succes = Behind( ref jointPositions, relation.ByAtLeast, relation.ButNotFurtherThan ); break;
                        default: throw new Exception( "Invalid Action Relation" );
                    }
                }
                else
                {
                    break;
                }
            }

            if ( succes )
            {
                _IsActive = false;
            }
        }
        else
        {
            succes = false;
        }

        return succes;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is above the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Above( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].y > jointPositions[RelationalJointIndex].y )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].y - jointPositions[ RelationalJointIndex ].y );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is below the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Bellow( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].y < jointPositions[RelationalJointIndex].y )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].y - jointPositions[ RelationalJointIndex ].y );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is Left of the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Left( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].x < jointPositions[RelationalJointIndex].x )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].x - jointPositions[ RelationalJointIndex ].x );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is Right of the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Right( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].x > jointPositions[RelationalJointIndex].x )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].x - jointPositions[ RelationalJointIndex ].x );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is Infront of the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Infront( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].z < jointPositions[RelationalJointIndex].z )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].z - jointPositions[ RelationalJointIndex ].z );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param jointPositions Reference to the JoinPosition data
    /// @param byAtLeast Distance between the two joints
    /// @brief Returns true if the Secondary Body part is Behind of the Primary body part by the correct amount 
    ////////////////////////////////////////////////
    protected bool Behind( ref Vector3[] jointPositions, float byAtLeast, float butNotFurtherThan )
    {
        if ( jointPositions[PrimaryJointIndex].z > jointPositions[RelationalJointIndex].z )
        {
            float difference = Mathf.Abs( jointPositions[ PrimaryJointIndex ].z - jointPositions[ RelationalJointIndex ].z );

            if ( DifferenceCheck( difference, byAtLeast, butNotFurtherThan ) )
            {
                return true;
            }
        }

        return false;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param difference The value to compare
    /// @param near Minimum value distance can be
    /// @param far Maximum value difference can be
    /// @brief Returns true if difference lies within near and far range 
    ////////////////////////////////////////////////
    protected bool DifferenceCheck( float difference, float near, float far )
    {
        if( far == 0.0f )
        {
            if ( difference >= near )
            {
                return true;
            }
        }
        else
        { 
            if( difference >= near && difference <= far )
            {
                return true;
            }
        }

        return false;
    }
}