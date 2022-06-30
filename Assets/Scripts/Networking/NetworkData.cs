using UnityEngine;
using System.Collections;
using System;

[Serializable()]
public class NetworkData
{
    protected Vector3 _v3Position;
    public Vector3 Position { get { return _v3Position; } set { _v3Position = value; } }

    protected Vector3 _v3Velocity;
    public Vector3 Velocity { get { return _v3Velocity; } set { _v3Velocity = value; } }

    protected Quaternion _qRotation;
    public Quaternion Rotation { get { return _qRotation; } set { _qRotation = value; } }

    protected bool _bActive;
    public bool Active { get { return _bActive; } set { _bActive = value; } }

    protected int _iHealth;
    public int Health{ get { return _iHealth; } set { _iHealth = value; } }

    protected string _sName;
    public string Name { get { return _sName; } set { _sName = value; } }

    protected int _aCurrentAnimation;
    public int CurrentAnimation { get { return _aCurrentAnimation; } set { _aCurrentAnimation = value; } }

    protected int team;
    public int Team { get { return team; } set { team = value; } }

    protected bool _exploding;
    public bool Exploding { get { return _exploding; } set { _exploding = value; } }


    public void SerializeToBitStream( BitStream stream )
    {
        stream.Serialize( ref _v3Position );
        stream.Serialize( ref _v3Velocity );
        stream.Serialize( ref _qRotation );
        stream.Serialize( ref _bActive );
        stream.Serialize( ref _iHealth );
        stream.Serialize(ref _aCurrentAnimation);
        stream.Serialize(ref team);
        stream.Serialize( ref _exploding );
    }

    public void DeserializeFromBitStream( BitStream stream )
    {
        stream.Serialize( ref _v3Position );
        stream.Serialize( ref _v3Velocity );
        stream.Serialize( ref _qRotation );
        stream.Serialize( ref _bActive );
        stream.Serialize( ref _iHealth );
        stream.Serialize(ref _aCurrentAnimation);
        stream.Serialize(ref team);
        stream.Serialize( ref _exploding );
    }
}
