using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class MessageData
{
    protected List<object> Data { get; set; }

    public MessageData()
    {
        Data = new List<object>();
    }

    public void Add<T>( T data )
    {
        Data.Add( data );
    }

    public T Get<T>( int index )
    {
        return ( T ) Data[ index ];
    }

    public void Add( UnityEngine.Vector3 data )
    {
        NetworkStructureWrapper wrapper = new NetworkStructureWrapper();
        wrapper.Create( data );
        Data.Add( wrapper );
    }

    public Vector3 Get( int index )
    {
        return ( Vector3 ) ( ( ( NetworkStructureWrapper ) Data[ index ] ).Convert() );
    }
}