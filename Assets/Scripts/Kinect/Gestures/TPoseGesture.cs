using UnityEngine;
using System.Collections;

public class TPoseGesture : KinectGesture 
{
    public TPoseGesture()
    {
        this.ID = "TPose";
        KinectAction leftArmOut = new KinectAction();
        leftArmOut.TimeOut = 1.0f;
        leftArmOut.PrimaryJoint =  KinectMSWrapper.NuiSkeletonPositionIndex.HandLeft;
        leftArmOut.RelationalJoint = KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderLeft;
        leftArmOut.Activate();

        ActionRelationContraint leftArmOutContraint = new ActionRelationContraint();
        leftArmOutContraint.Relation = ActionRelations.TO_THE_LEFT_OF;
        leftArmOutContraint.ByAtLeast = 0.1f;

        leftArmOut.AddRelationship( leftArmOutContraint );

        this.AddAction( leftArmOut );

        KinectAction rightArmOut = new KinectAction();
        rightArmOut.TimeOut = 1.0f;
        rightArmOut.PrimaryJoint = KinectMSWrapper.NuiSkeletonPositionIndex.HandRight;
        rightArmOut.RelationalJoint = KinectMSWrapper.NuiSkeletonPositionIndex.ShoulderRight;

        ActionRelationContraint rightArmOutContraint = new ActionRelationContraint();
        rightArmOutContraint.Relation = ActionRelations.TO_THE_RIGHT_OF;
        rightArmOutContraint.ByAtLeast = 0.4f;

        rightArmOut.AddRelationship( rightArmOutContraint );

        this.AddAction( rightArmOut );

        this.HoldTime = 2.0f;
    }
}
