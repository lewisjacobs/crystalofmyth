using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkController : Singleton<NetworkController> 
{
    private bool isRefreshingHostList = false;
    protected List<HostData> HostList { get; private set; }
    protected List<string> _HostNames = new List<string>();

    public delegate void NetworkCallback();
    public delegate void NetworkPlayerCallback( NetworkPlayer player );
    public delegate void NetworkDisconnectionCallback( NetworkDisconnection player );
    public delegate void NetworkConnectionErrorCallback( NetworkConnectionError error );
    public event NetworkCallback ServerInitializedCallback;
    public event NetworkCallback ConnectedToServerCallback;
    public event NetworkPlayerCallback OnPlayerConnectedCallback;
    public event NetworkPlayerCallback OnPlayerDisconnectedCallback;
    public event NetworkDisconnectionCallback OnNetworkDisconnectedCallback;
    public event NetworkConnectionErrorCallback OnFailedToConnectCallback;

    public delegate void NetworkHostListRefreshCallback( ref List<string> hostList );
    public event NetworkHostListRefreshCallback HostListRefresh;

    protected bool _bConnected = false;

    public bool ShowGUI { get; set; }

	protected NetworkController()
    {
        HostList = new List<HostData>();
    }

    public void Awake()
    {
        MasterServer.ipAddress = "10.8.0.1";
        MasterServer.port = 23466;
        Network.natFacilitatorIP = "10.8.0.1";
        Network.natFacilitatorPort = 50005;

        DontDestroyOnLoad( this.gameObject );
    }

	public UnityEngine.Object Instanciate( UnityEngine.Object prefab, Vector3 v3Position, Quaternion qRotation )
    {
        return Network.Instantiate( prefab, v3Position, qRotation, 0 );
    }

    public void DestroyObject( GameObject obj )
    {
        NetworkView view = obj.GetComponent<NetworkView>();

        if( view != null )
        {
            if( view.isMine )
            {
                Network.Destroy( obj );
            }
        }
    }

    public void StartServer( string serverTypeName, string roomName )
    {
        if ( !_bConnected )
        {
			try
            {
                Network.InitializeServer( 10, 25000, !Network.HavePublicAddress() );
                MasterServer.RegisterHost( serverTypeName, roomName );
                _bConnected = true;
            }
			catch( Exception )
            {
                _bConnected = false;
            }
        }
    }

    public void Update()
    {
        if ( isRefreshingHostList && MasterServer.PollHostList().Length > 0 )
        {
            isRefreshingHostList = false;
            HostList.Clear();
            HostList.AddRange( MasterServer.PollHostList() );

            BuildHostNames();

            if (HostListRefresh != null)
            {
                HostListRefresh( ref _HostNames );
            }
        }
    }

    public void DisconnectServer()
    {
        if( _bConnected )
        {
            Network.DestroyPlayerObjects(Network.player);
            Network.Disconnect();
            MasterServer.UnregisterHost();
            _bConnected = false;
        }
    }

    public void Disconnect()
    {
        if ( _bConnected )
        {
            Network.Disconnect();
            _bConnected = false;
        }
    }

    public void RefreshHostList( string serverTypeName )
    {
        if ( !isRefreshingHostList )
        {
            isRefreshingHostList = true;
            MasterServer.RequestHostList( serverTypeName );
        }
    }

    public void StopRefreshingHostList()
    {
        isRefreshingHostList = false;
        MasterServer.ClearHostList();
    }

    public void BuildHostNames()
    {
        _HostNames.Clear();

        foreach (HostData data in HostList)
        {
            _HostNames.Add( data.gameName );
        }
    }

    public void JoinServerIndex(int index)
    {
        JoinServer(HostList[index]);
    }

    protected void JoinServer( HostData hostData )
    {
        Network.Connect( hostData );
    }

    public void OnConnectedToServer()
    {
        _bConnected = true;
        if ( ConnectedToServerCallback != null )
        {
            ConnectedToServerCallback();
        }
    }

    public void OnServerInitialized()
    {
        _bConnected = true;
        if ( ServerInitializedCallback != null )
        {
            ServerInitializedCallback();
        }
    }

    public void OnPlayerConnected( NetworkPlayer player )
    {
        if( OnPlayerConnectedCallback != null )
        {
            OnPlayerConnectedCallback( player );
        }
    }

    public void OnPlayerDisconnected( NetworkPlayer player )
    {
        Network.RemoveRPCs( player );
        Network.DestroyPlayerObjects( player );

        if( OnPlayerDisconnectedCallback != null )
        {
            OnPlayerDisconnectedCallback( player );
        }
    }

    public void OnDisconnectedFromServer( NetworkDisconnection info )
    {
        _bConnected = false;

        if ( Network.isServer )
        {
            Debug.Log( "Local server connection disconnected" );
        }
        else
        {
            if ( info == NetworkDisconnection.LostConnection )
            {
                Debug.Log( "Lost connection to the server" );
            }
            else
            {
                Debug.Log( "Successfully disconnected from the server" );
            }
        }

        if ( OnNetworkDisconnectedCallback != null )
        {
            OnNetworkDisconnectedCallback( info );
        }
    }

    public void OnFailedToConnect( NetworkConnectionError error )
    {
        _bConnected = false;

        if ( OnFailedToConnectCallback != null )
        {
            OnFailedToConnectCallback( error );
        }
    }

    public void ClearHostData()
    {
        HostList.Clear();
        _HostNames.Clear();
    }
}
