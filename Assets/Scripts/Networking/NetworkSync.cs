using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public enum SyncType
{
    Spell,
    Character,
    Other
}

[RequireComponent ( typeof( NetworkView ) )]
public class NetworkSync : MonoBehaviour
{
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Quaternion syncStartRotation = Quaternion.identity;
    private Quaternion syncEndRotaiton = Quaternion.identity;
    protected bool Sender = false;
    protected bool _Named = false;
    protected NetworkView _myView;

    public SyncType NetworkSyncType;

    public void Start()
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            Destroy(this);
        }
    }

    public void OnSerializeNetworkView( BitStream stream, NetworkMessageInfo info )
    {
        if ( NetworkSyncType == SyncType.Character )
        {
            CharacterOnSerializeNetworkView( stream, info );
        }
        else if( NetworkSyncType == SyncType.Spell )
        {
            SpellOnSerializedNetworkView( stream, info );
        }
    }

    public void CharacterOnSerializeNetworkView( BitStream stream, NetworkMessageInfo info )
    {
        NetworkData syncData = new NetworkData();

        if ( stream.isWriting )
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();

            if ( rigidBody != null )
            {
                syncData.Position = rigidBody.position;
                syncData.Velocity = rigidBody.velocity;
                syncData.Rotation = rigidBody.rotation;

            }
            
            Character character = this.gameObject.GetComponent<Character>();

            if ( character != null )
            {
                syncData.Active = character.Active;
                syncData.Health = character.StatCurrentHealth;
                syncData.CurrentAnimation = character.CurrentAnimation;

                syncData.Team = character.Team;
            }

            syncData.SerializeToBitStream( stream );
        }
        else
        {
            syncData.DeserializeFromBitStream( stream );

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncData.Position + syncData.Velocity * syncDelay;
            syncStartPosition = GetComponent<Rigidbody>().position;

            syncEndRotaiton = syncData.Rotation;
            syncStartRotation = GetComponent<Rigidbody>().rotation;

            Character character = this.gameObject.GetComponent<Character>();

            if ( character != null )
            {
                if ( syncData.Active )
                {
                    character.SetActive();
                }
                else
                {
                    character.SetInactive();
                }

                character.StatCurrentHealth = syncData.Health;
                character.CurrentAnimation = syncData.CurrentAnimation;
                character.UpdateCharacterAnimation();

                character.Team = syncData.Team;
                character.RefreshHeathBar();
            }
        }
    }

    public void SpellOnSerializedNetworkView( BitStream stream, NetworkMessageInfo info )
    {
        NetworkData syncData = new NetworkData();

        if ( stream.isWriting )
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if ( rigidbody != null )
            {

                syncData.Position = rigidbody.position;
                syncData.Velocity = rigidbody.velocity;
                syncData.Rotation = rigidbody.rotation;
            }

            syncData.Exploding = GetComponent<Spell>().Exploding;

            syncData.SerializeToBitStream( stream );
        }
        else
        {
            syncData.DeserializeFromBitStream( stream );

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncData.Position + syncData.Velocity * syncDelay;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            
            if( rigidbody != null )
            {
                syncStartPosition = GetComponent<Rigidbody>().position;

                syncEndRotaiton = syncData.Rotation;
                syncStartRotation = GetComponent<Rigidbody>().rotation;
            }

            GetComponent<Spell>().Exploding = syncData.Exploding;
            GetComponent<Spell>().RemoteFix = true;
        }
    }

    public void Awake()
    {
        if ( NetworkSyncType == SyncType.Character || NetworkSyncType == SyncType.Spell )
        {
            lastSynchronizationTime = Time.time;
        }

        if( StaticProperties.Instance.MultiPlayer )
        {
            _myView = GetComponent<NetworkView>();

            if ( !_myView.isMine )
            {
                Rigidbody rigidbody = this.GetComponent<Rigidbody>();

                if ( rigidbody != null )
                {
                    rigidbody.isKinematic = true;
                }
            }

            Sender = false;
        }
    }

    protected int _lateUpdate = 0;
    public void Update()
    {
        if ( NetworkSyncType == SyncType.Character || NetworkSyncType == SyncType.Spell )
        {
            if ( StaticProperties.Instance.MultiPlayer )
            {
                NetworkView view = GetComponent<NetworkView>();
                if ( !view.isMine )
                {
                    if( NetworkSyncType == SyncType.Character )
                    {
                        CharacterSyncedMovement();
                    }
                    else if( NetworkSyncType == SyncType.Spell )
                    {
                        SpellSyncedMovement();
                    }
                }

                if( _lateUpdate > 5 )
                {
                    if( !_Named && _myView.isMine )
                    {
                        Character character = this.gameObject.GetComponent<Character>();

                        if ( character != null )
                        {
                            Player player = PlayerController.Instance.GetPlayer(character.ID);

                            if( player != null )
                            {
                                MessageData data = new MessageData();
                                data.Add<string>( player.Name );
                                data.Add<int>(player.PlayerTeam);
                                SendMessageToCharacterAsSender( "SetNameMessage", data );
                                _Named = true;
                            }
                        }
                    }
                }
                else
                {
                    _lateUpdate++;
                }
            }
        }
    }

    protected void SpellSyncedMovement()
    {
        syncTime += Time.deltaTime;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.position = Vector3.Lerp( syncStartPosition, syncEndPosition, syncTime / syncDelay );
    }

    protected void CharacterSyncedMovement()
    {
        syncTime += Time.deltaTime;

        Character character = GetComponent<Character>();

        if( character != null )
        {
            if ( !character.Active )
            {
                GetComponent<Rigidbody>().position = syncEndPosition;
            }
            else
            {
                GetComponent<Rigidbody>().position = Vector3.Lerp( syncStartPosition, syncEndPosition, syncTime / syncDelay );
            }

            GetComponent<Rigidbody>().rotation = Quaternion.Lerp( syncStartRotation, syncEndRotaiton, syncTime / syncDelay );
        }
    }

    public void SendMessageToCharacterAsSender( string sFunction, MessageData val = null )
    {
        if( val == null )
        {
            val = new MessageData();
        }

        Sender = true;
        SendMessageToCharacter(sFunction, SerializeMessageParms( val ) );
        Sender = false;
    }

    [RPC]
    public void SendMessageToCharacter( string sFunction, string val )
    {
        MessageData deSerialized = DeSerializeMessageParms( val );

        gameObject.SendMessage( sFunction, Convert.ChangeType( deSerialized, deSerialized.GetType() ) );

        if ( Sender )
        {
            try
            {
                GetComponent<NetworkView>().RPC( "SendMessageToCharacter", RPCMode.Others, sFunction, val );
            }
            catch( Exception )
            {
                Debug.Log( "No clients to send message to" );
            }
        }
    }

    public void SendToSpecificClientAsSender( NetworkPlayer player, string sFunction, MessageData val )
    {
        Sender = true;
        GetComponent<NetworkView>().RPC( "SendToSpecificClient", player, sFunction, SerializeMessageParms( val ) );
        Sender = false;
    }

    [RPC]
    public void SendToSpecificClient( string sFunction, string val )
    {
        MessageData deSerialized = DeSerializeMessageParms( val );
        gameObject.SendMessage( sFunction, Convert.ChangeType( deSerialized, deSerialized.GetType() ) );
    }
    
    protected string SerializeMessageParms( MessageData val )
    {
        byte[] baSerialized;
        
        using ( MemoryStream stream = new MemoryStream() )
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(stream, val);
                baSerialized = stream.ToArray();
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize MESSAGE Reason: " + e.Message);
                throw;
            }
            finally
            {
                stream.Close();
            }
        }

        return Convert.ToBase64String( baSerialized );
    }

    protected MessageData DeSerializeMessageParms( string val )
    {
        byte[] byteVal = Convert.FromBase64String( val );
        MessageData baDeSerialized;

        using ( MemoryStream stream = new MemoryStream( byteVal ) )
        {
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                baDeSerialized = ( MessageData ) formatter.Deserialize( stream );
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to DeSerialize MESSAGE Reason: " + e.Message);
                throw;
            }
            finally
            {
                stream.Close();
            }
        }

        return baDeSerialized;
    }

    public  void OnDisconnectedFromServer( NetworkDisconnection info )
    {
        Destroy( this.gameObject );
    }
}
