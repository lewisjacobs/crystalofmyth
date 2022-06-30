using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct CalibrationStructure
{
    public float ArmLength;
    public float LegLength;
}

public class KinectGesture
{
    public delegate void ActionCompleteCallback( Type type );
    public event ActionCompleteCallback CompletedCallback;
    protected List<KinectAction> Actions { get; set; }
    public int CurrentAction { get; protected set; }
    public string ID { get; set; }

    public float HoldTime { get; set; }
    protected float _gestureStartTime;

    public KinectGesture()
    {
        Actions = new List<KinectAction>();
        CurrentAction = 0;
        HoldTime = 0;
        _gestureStartTime = 0.0f;
    }

    public void AddAction( KinectAction action )
    {
        Actions.Add( action );
    }

    public void CheckCompletion( ref Vector3[] jointPosition )
    {
        if( !Actions[CurrentAction].TimedOut() )
        {
            if ( Actions[ CurrentAction ].CheckCompletion( ref jointPosition ) )
            {
                if ( CurrentAction >= Actions.Count - 1 )
                {
                    // if current held time is greater than HoldTime, fire callback
                    if ( _gestureStartTime != 0.0f && ( GameController.Instance.ElapsedTime - _gestureStartTime ) >= HoldTime )
                    {
                        if ( CompletedCallback != null )
                        {
                            CompletedCallback( this.GetType() );
                        }

                        ResetGesture();
                    }
                    // else gesture has been completed but not held long enough
                    else
                    {
                        // if first completion since reset, set time
                        if ( _gestureStartTime == 0.0f )
                        {
                            _gestureStartTime = GameController.Instance.ElapsedTime;
                        }

                        ResetGesture( true );
                    }
                }
                else
                {
                    //more actions in gesture, deactivate current and continue to check next action
                    Actions[ CurrentAction ].Deactivate();
                    CurrentAction++;
                    Actions[ CurrentAction ].Activate();
                    CheckCompletion( ref jointPosition );
                }
            }
        }
        else
        {
            ResetGesture();
        }
    }

    public void ResetGesture( bool held = false )
    {
        Actions[ CurrentAction ].Deactivate();
        Actions[ 0 ].Activate();

        CurrentAction = 0;

        if( !held )
        {
            _gestureStartTime = 0.0f;
        }
    }

    public virtual void  CalibrateGesture( CalibrationStructure calibrationStructure )
    {
    }
}
