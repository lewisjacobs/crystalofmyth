using UnityEngine;
using System.Collections;
using System;

[Serializable]
public enum NetworkStructureType
{
    Vector3
};

[Serializable]
public class NetworkStructureWrapper 
{
    public NetworkStructureType DataType { get; set; }
    public object[] Data { get; set; }


    public NetworkStructureWrapper()
    {
    }

    public void Create( Vector3 vector )
    {
        this.DataType = NetworkStructureType.Vector3;
        this.Data = new object[ 3 ] { vector.x, vector.y, vector.z };
    }

    public object Convert()
    {
        if( DataType == NetworkStructureType.Vector3 )
        {
            return CreateForVector( Data );
        }

        return null;
    }

    protected Vector3 CreateForVector( object[] data )
    {
        return new Vector3( ( float ) data[ 0 ], ( float ) data[ 1 ], ( float ) data[ 2 ] );
    }
}
