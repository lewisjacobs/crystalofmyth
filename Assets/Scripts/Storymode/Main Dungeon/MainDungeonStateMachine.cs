using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent( typeof( NetworkView ) )]
//[RequireComponent( typeof( NetworkSync ) )]
public class MainDungeonStateMachine : MonoBehaviour 
{
    public GameObject Door;
    public GameObject NPCPrefab;

    public GameObject EnemiesNPCPrefab;
    public GameObject BossNPCPrefab;

    public AudioClip NPC1AudioClip;
    public AudioClip NPC2AudioClip;
    public AudioClip NPC3AudioClip;
    public AudioClip NPC4AudioClip;

    public GameObject _NPObject;
    public  GameObject _NPObject2;
    public  GameObject _NPObject3;
    public GameObject _NPObject4;

    protected Character _boss;


    protected List<Character> _AICharacters = new List<Character>();

    protected bool _healthSet = false;

    public enum AdventureState
    {
        Cinematic,
		End_Cinematic,
        NPC1,
        NPC1_Running,
        NPC1_Finished,
        NPC2,
        NPC2_Running,
        NPC2_Finished,
        NPC3,
        NPC3_Running,
        NPC3_Finished,
        NPC4,
        NPC4_Running,
        NPC4_Finished,
        Boss
    }

    protected NetworkSync _networkSync;
    public AdventureState CurrentState { get; protected set; }
    protected bool _eventHandled = false;

    protected int _updateCount = 0;

	public void Start () 
    {
        CurrentState = AdventureState.Cinematic;
        
        if( StaticProperties.Instance.MultiPlayer )
        {
            _networkSync = this.GetComponent<NetworkSync>();
            _networkSync.NetworkSyncType = SyncType.Other;
        }

        _NPObject.SetActive( true );
        _NPObject2.SetActive( false );
        _NPObject3.SetActive( false );
        _NPObject4.SetActive( false );

        LoadAI1();
        LoadAI2();
        LoadAI3();
        LoadAI4();

        SetHealths();
    }

	public void Update () 
    {
        HandleAdventureState();
        CheckForEndGame();
        SetHealths();
	}

    protected void SetHealths()
    {
        if ( !_healthSet )
        {
            if ( StaticProperties.Instance.IsHost )
            {
                int pnum = PlayerController.Instance.NumOfRealPlayers();
                pnum++;

                foreach ( Character ch in _AICharacters )
                {
                    ch.statBaseHealth *= pnum;
                    ch.StatCurrentHealth = ch.statBaseHealth;
                }

                if ( pnum != 1 )
                {
                    _healthSet = true;
                }
            }
        }

        if( _updateCount == 1 )
        {
            foreach( Character ch in _AICharacters )
            {
                ch.statBaseHealth *= PlayerController.Instance.NumOfRealPlayers();
                ch.StatCurrentHealth = ch.statBaseHealth;
            }
            _updateCount++;
        }
        else if( _updateCount == 0 )
        {
            _updateCount++;
        }        
    }

    protected void HandleAdventureState()
    {
        switch( CurrentState )
        {
            case AdventureState.Cinematic: Cinematic(); break;
            case AdventureState.NPC1: NPC1Event(); break;
            case AdventureState.NPC1_Running: NPC1_RunningEvent(); break;
            case AdventureState.NPC1_Finished: NPC1_FinishedEvent(); break;
            case AdventureState.NPC2: NPC2Event(); break;
            case AdventureState.NPC2_Running: NPC2_RunningEvent(); break;
            case AdventureState.NPC2_Finished: NPC2_FinishedEvent(); break;
            case AdventureState.NPC3: NPC3Event(); break;
            case AdventureState.NPC3_Running: NPC3_RunningEvent(); break;
            case AdventureState.NPC3_Finished: NPC3_FinishedEvent(); break;
            case AdventureState.NPC4: NPC4Event(); break;
            case AdventureState.NPC4_Running: NPC4_RunningEvent(); break;
            case AdventureState.NPC4_Finished: NPC4_FinishedEvent(); break;
        }
    }

	protected void Cinematic()
	{
		CurrentState= AdventureState.End_Cinematic;
	}

    protected void NPC1Event()
    {
        if ( !_eventHandled )
        {
            _eventHandled =  false;
            CurrentState = AdventureState.NPC1_Running;
            _NPObject.GetComponent<AudioSource>().clip = NPC1AudioClip;
            _NPObject.GetComponent<AudioSource>().Play();
        }
    }

    protected void NPC1_RunningEvent()
    {
        if ( !_NPObject.GetComponent<AudioSource>().isPlaying )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC1_Finished;
        }
    }

    protected void NPC1_FinishedEvent()
    {
        if ( !_eventHandled )
        {
            _eventHandled = true;
            _NPObject.SetActive( false );
            _NPObject2.SetActive( true );
        }
    }

    protected void NPC2Event()
    {
        if ( !_eventHandled )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC2_Running;
            _NPObject2.GetComponent<AudioSource>().clip = NPC2AudioClip;
            _NPObject2.GetComponent<AudioSource>().Play();
        }
    }

    protected void NPC2_RunningEvent()
    {
        if ( !_NPObject2.GetComponent<AudioSource>().isPlaying )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC2_Finished;
        }
    }

    protected void NPC2_FinishedEvent()
    {
        if ( !_eventHandled )
        {
            _eventHandled = true;
            _NPObject2.SetActive( false );
            _NPObject3.SetActive( true );
        }
    }

    protected void NPC3Event()
    {
        if ( !_eventHandled )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC3_Running;
            _NPObject3.GetComponent<AudioSource>().clip = NPC3AudioClip;
            _NPObject3.GetComponent<AudioSource>().Play();
        }
    }

    protected void NPC3_RunningEvent()
    {
        if ( !_NPObject3.GetComponent<AudioSource>().isPlaying )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC3_Finished;
        }
    }

    protected void NPC3_FinishedEvent()
    {
        if ( !_eventHandled )
        {
            _eventHandled = true;
            _NPObject3.SetActive( false );
            Door.SetActive( false );
            _NPObject4.SetActive( true );
        }
    }

    protected void NPC4Event()
    {
        if ( !_eventHandled )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC3_Running;
            _NPObject4.GetComponent<AudioSource>().clip = NPC4AudioClip;
            _NPObject4.GetComponent<AudioSource>().Play();
        }
    }

    protected void NPC4_RunningEvent()
    {
        if ( !_NPObject4.GetComponent<AudioSource>().isPlaying )
        {
            _eventHandled = false;
            CurrentState = AdventureState.NPC4_Finished;
        }

    }

    protected void NPC4_FinishedEvent()
    {
        if ( !_eventHandled )
        {
            _eventHandled = true;
            _NPObject4.SetActive( false );
            CurrentState = AdventureState.Boss;
        }
    }

    public void SetAdventureState( AdventureState state )
    {
        if( StaticProperties.Instance.MultiPlayer )
        {
            MessageData data = new MessageData();
            data.Add<AdventureState>( state );

            _networkSync.SendMessageToCharacterAsSender( "UpdateAdventureStateMessage", data );
        }
        else
        {
            UpdateAdventureState( state );
        }
    }

    public void UpdateAdventureStateMessage( MessageData data )
    {
        UpdateAdventureState( data.Get<AdventureState>( 0 ) );
    }

    public void UpdateAdventureState( AdventureState state )
    {
        _eventHandled = false;
        CurrentState = state;
    }

    protected void LoadAI1()
    {
        Transform stubs = GameObject.Find( "Enemy Spawn Points 1" ).transform;
        
        foreach( Transform child in stubs.transform )
        {
            if( StaticProperties.Instance.IsHost )
            {
                Character nps = ( ( GameObject ) GameController.Instance.InstanciatePrefab( EnemiesNPCPrefab, child.transform.position, child.transform.rotation ) ).GetComponent<Character>();
                nps.Team = TeamColours.RED;

                AICharacter aiChar = new AICharacter( ChosenCharacter.BARBARIAN ) { PlayerTeam = TeamColours.RED };
                aiChar.PlayerTeam = TeamColours.RED;
                aiChar.SetCharacter( nps );
                aiChar.CreateWayPointSystem();
                aiChar.Lives = 1;

                PlayerController.Instance.AddPlayer( aiChar );
                _AICharacters.Add(nps);
            }

            Destroy( child.gameObject );
        }
    }

    protected void LoadAI2()
    {
        Transform stubs = GameObject.Find( "Enemy Spawn Points 2" ).transform;

        foreach ( Transform child in stubs.transform )
        {
            if ( StaticProperties.Instance.IsHost )
            {
                Character nps = ( ( GameObject ) GameController.Instance.InstanciatePrefab( EnemiesNPCPrefab, child.transform.position, child.transform.rotation ) ).GetComponent<Character>();
                nps.Team = TeamColours.GREEN;

                AICharacter aiChar = new AICharacter( ChosenCharacter.BARBARIAN ) { PlayerTeam = TeamColours.GREEN };
                aiChar.PlayerTeam = TeamColours.GREEN;
                aiChar.SetCharacter( nps );
                aiChar.CreateWayPointSystem();
                aiChar.Lives = 1;

                PlayerController.Instance.AddPlayer( aiChar );
                _AICharacters.Add(nps);
            }

            Destroy( child.gameObject );
        }
    }

    protected void LoadAI3()
    {
        Transform stubs = GameObject.Find( "Enemy Spawn Points 3" ).transform;

        foreach ( Transform child in stubs.transform )
        {
            if ( StaticProperties.Instance.IsHost )
            {
                Character nps = ( ( GameObject ) GameController.Instance.InstanciatePrefab( EnemiesNPCPrefab, child.transform.position, child.transform.rotation ) ).GetComponent<Character>();
                nps.Team = TeamColours.YELLOW;

                AICharacter aiChar = new AICharacter( ChosenCharacter.BARBARIAN ) { PlayerTeam = TeamColours.YELLOW };
                aiChar.PlayerTeam = TeamColours.YELLOW;
                aiChar.SetCharacter( nps );
                aiChar.CreateWayPointSystem();
                aiChar.Lives = 1;

                PlayerController.Instance.AddPlayer( aiChar );
                _AICharacters.Add(nps);
            }

            Destroy( child.gameObject );
        }
    }

    protected void LoadAI4()
    {
        Transform stubs = GameObject.Find( "Enemy Spawn Points 4" ).transform;

        foreach ( Transform child in stubs.transform )
        {
            if ( StaticProperties.Instance.IsHost )
            {
                _boss = ( ( GameObject ) GameController.Instance.InstanciatePrefab( BossNPCPrefab, child.transform.position, child.transform.rotation ) ).GetComponent<Character>();
                _boss.Team = TeamColours.NOTEAM;
                _boss.name = "Boss11111";

                AICharacter aiChar = new AICharacter( ChosenCharacter.BARBARIAN ) { PlayerTeam = TeamColours.NOTEAM };
                aiChar.PlayerTeam = TeamColours.NOTEAM;
                aiChar.SetCharacter( _boss );
                aiChar.CreateWayPointSystem();
                aiChar.Lives = 1;

                PlayerController.Instance.AddPlayer( aiChar );
                _AICharacters.Add(_boss);
            }

            Destroy( child.gameObject );
        }
    }

    protected void CheckForEndGame()
    {
        if (_boss == null)
        {
            _boss = GameObject.Find("Boss11111").GetComponent<Character>();
        }
        if (_boss.StatCurrentHealth < 1)
        {

            AdventureController.Instance.BossKilled();
            GameController.Instance.QuitToMenu();
        }
    }

}