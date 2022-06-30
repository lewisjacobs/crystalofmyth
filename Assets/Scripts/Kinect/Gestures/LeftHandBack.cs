using UnityEngine;
using System.Collections;

public class LeftHandBack : KinectGesture 
{
    public LeftHandBack()
    {
        this.ID = "Left Hand Back";
        
        KinectAction rightArmBack = new KinectAction();
        rightArmBack.TimeOut = 1.0f;
        rightArmBack.PrimaryJoint = KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft;
        rightArmBack.RelationalJoint = KinectMSWrapper.NuiSkeletonPositionIndex.Spine;

        ActionRelationContraint rightArmOutContraint = new ActionRelationContraint();
        rightArmOutContraint.Relation = ActionRelations.INFRONT;
        rightArmOutContraint.ByAtLeastPersentage = 0.0f;
        rightArmOutContraint.ButNotFurtherThanPersentage = 0.3f;

        rightArmBack.AddRelationship( rightArmOutContraint );

        this.AddAction( rightArmBack );

        this.HoldTime = 0.5f;
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
