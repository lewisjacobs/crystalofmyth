using UnityEngine;
using System.Collections;

public class LeftHandForward : KinectGesture
{
    public LeftHandForward()
    {
        this.ID = "Left Hand Forward";
        
        KinectAction rightArmForward = new KinectAction();
        rightArmForward.TimeOut = 1.0f;
        rightArmForward.PrimaryJoint = KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft;
        rightArmForward.RelationalJoint = KinectMSWrapper.NuiSkeletonPositionIndex.Spine;

        ActionRelationContraint rightArmOutContraint = new ActionRelationContraint();
        rightArmOutContraint.Relation = ActionRelations.INFRONT;
        rightArmOutContraint.ByAtLeastPersentage = 0.80f;
        rightArmOutContraint.ButNotFurtherThanPersentage = 0.0f;

        rightArmForward.AddRelationship( rightArmOutContraint );

        this.AddAction( rightArmForward );

        this.HoldTime = 0.0f;
    }

    /////////////////////////////////////////////////
    /// @author Max Ashton
    /// @param calibrationStructure structure holding calibration data
    /// @brief Calibrates the structure using the arm length
    ////////////////////////////////////////////////
    public override void CalibrateGesture( CalibrationStructure calibrationStructure )
    {
        ActionRelationContraint rightArmOutContraint = this.Actions[ 0 ].GetRelationship( 0 );
        rightArmOutContraint.ByAtLeast = calibrationStructure.ArmLength * this.Actions[ 0 ].Relationships[ 0 ].ByAtLeastPersentage;
        rightArmOutContraint.ButNotFurtherThan = calibrationStructure.ArmLength * this.Actions[ 0 ].Relationships[ 0 ].ButNotFurtherThanPersentage;

        this.Actions[ 0 ].UpdateRelationship( 0, rightArmOutContraint );
    }
}
