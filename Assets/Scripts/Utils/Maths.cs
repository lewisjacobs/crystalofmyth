using UnityEngine;
using System.Collections;

/////////////////////////////////////////////////
/// Class for mathamatics not in Unity.Mathf
/////////////////////////////////////////////////
public class Maths 
{
    /////////////////////////////////////////////////
    /// Author: Max Ashton
    /// Method:ConvertMatrixToQuat
    /// Description: Converts a matrix to a quaternion, bool to flip the x y axies
    /////////////////////////////////////////////////
    public static Quaternion ConvertMatrixToQuat( Matrix4x4 mOrient, int joint, bool flip )
    {
        Vector4 vZ = mOrient.GetColumn( 2 );
        Vector4 vY = mOrient.GetColumn( 1 );

        if ( !flip )
        {
            vZ.y = -vZ.y;
            vY.x = -vY.x;
            vY.z = -vY.z;
        }
        else
        {
            vZ.x = -vZ.x;
            vZ.y = -vZ.y;
            vY.z = -vY.z;
        }

        if ( vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f )
            return Quaternion.LookRotation( vZ, vY );
        else
            return Quaternion.identity;
    }
}
