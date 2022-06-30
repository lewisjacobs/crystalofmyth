using UnityEngine;
using System.Collections;

public class LeftHandMiddle : KinectGesture
{
    public LeftHandMiddle()
    {
        this.ID = "Left Hand Middle";
        
        KinectAction rightArmMiddle = new KinectAction();
        rightArmMiddle.TimeOut = 1.0f;
        rightArmMiddle.PrimaryJoint = KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft;
        rightArmMiddle.RelationalJoint = KinectMSWrapper.NuiSkeletonPositionIndex.Spine;

        ActionRelationContraint rightArmOutContraint = new ActionRelationContraint();
        rightArmOutContraint.Relation = ActionRelations.INFRONT;
        rightArmOutContraint.ByAtLeastPersentage = 0.35f;
        rightArmOutContraint.ButNotFurtherThanPersentage = 0.65f;

        rightArmMiddle.AddRelationship( rightArmOutContraint );

        this.AddAction( rightArmMiddle );

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
