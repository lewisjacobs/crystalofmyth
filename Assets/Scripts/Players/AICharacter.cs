using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PigeonCoopToolkit.TIM;

public class AICharacter : CharacterPlayer
{
    public enum AIState
    {
        Dead,
        Idel,
        Patroling,
        Attacking,
        LookToFlee,
        Fleeing
    }

	public Character Character { get; set; }
	private bool idle;

    private Character target;

    public float fieldOfView = 110f;
    public SphereCollider scol;
    public Animator anim;
    public NavMeshAgent nav;
    public List<Transform> patrolWayPoints = new List<Transform>();

    public float _attackDistance = 10.0f;
    public float _chaseDistance = 20.0f;
    public float _minDistance = 5.0f;
    public int _patrolIndex = -1;

    protected AI_Sight _aiSight;
    protected AIState _currentState = AIState.Patroling;

    //protected float deadZone = 5f * Mathf.Deg2Rad;  
    //public float speedDampTime = 0.1f;              
    //public float angularSpeedDampTime = 0.7f;       
    //public float angleResponseTime = 0.6f; 

    public AICharacter( ChosenCharacter aiType )
    {
        IsAI = true;

        BuildAI( aiType );
    }

    public override void CreateWayPointSystem()
    {
        if ( StaticProperties.Instance.GameType != GameTypes.ADVENTURE )
        {
            for ( int i = 0; i <= 18; i++ )
            {
                patrolWayPoints.Add( GameObject.Find( "Way Point " + i ).transform );
            }
		}
        else
        {
            if ( PlayerTeam == TeamColours.RED )
            {
                Transform holder = GameObject.Find( "Waypoints 1" ).transform;

                foreach( Transform child in holder )
                {
                    patrolWayPoints.Add( child );
                }
            }
            else if ( PlayerTeam == TeamColours.GREEN )
            {
                Transform holder = GameObject.Find( "Waypoints 2" ).transform;

                foreach ( Transform child in holder )
                {
                    patrolWayPoints.Add( child );
                }
            }
            else if( PlayerTeam == TeamColours.YELLOW )
            {
                Transform holder = GameObject.Find( "Waypoints 3" ).transform;

                foreach ( Transform child in holder )
                {
                    patrolWayPoints.Add( child );
                }
            }
            else if ( PlayerTeam == TeamColours.NOTEAM )
            {
                Transform holder = GameObject.Find( "Waypoints 4" ).transform;

                foreach ( Transform child in holder )
                {
                    patrolWayPoints.Add( child );
                }
            }
        }
    }

    protected void BuildAI( ChosenCharacter aiType )
    {
        if( aiType == ChosenCharacter.MYSTIC || aiType ==  ChosenCharacter.HUNTRESS )
        {
            _attackDistance = 10.0f;
            _chaseDistance = 20.0f;
            _minDistance = 5.0f;
        }
        else 
        {
            _attackDistance = 5.0f;
            _chaseDistance = 20.0f;
            _minDistance = 1.0f;
        }
    }

    public void SetCharacter(Character c)
    {
        InitialiseStats();
        Character = c;
        Character.ID = PlayerID;
        Character.characterEvent += AICharacterEventCallback;

        if (!StaticProperties.Instance.MultiPlayer)
            Character.name = Name;

        scol = Character.GetComponent<SphereCollider>();
        nav = Character.GetComponent<NavMeshAgent>();
        _aiSight = Character.GetComponent<AI_Sight>();
        anim = Character.GetComponent<Animator>();

        nav.stoppingDistance = 1.0f;
    }

	public override void Update() 
	{
        base.Update();
        CheckForStun();
        if ( switchLeftCooldown > 0 ) switchLeftCooldown -= Time.deltaTime;
        if ( switchRightCooldown > 0 ) switchRightCooldown -= Time.deltaTime;
        if ( RespawnTimer <= 0 && !Alive && Lives > 0 )
        {
            Alive = true;
            Character.Respawn();
        }
        else if (RespawnTimer >= 1.0 && RespawnTimer <= 1.5f && !Alive)
        {
            Vector3 position = GameController.Instance.GetSpawnPoint(true);
            Character.transform.position = position;
        }
        HandleState();
        HandleAnimations();
	}

    protected void CheckForStun()
    {
        if(Character.frozen)
        {
            nav.Stop();
        }
        else
        {
            nav.Resume();
        }

        if(Character.PushedBack)
        {
            nav.Stop();
            nav.ResetPath();
        }
    }

    protected void HandleState()
    {
        CheckToFlee();

        if( _currentState != AIState.Dead )
        {
            Character.SetActive();
        }

        if( _currentState != AIState.Fleeing )
        {
            SearchForTarget();
        }

        if ( _currentState == AIState.Dead )
        {
            nav.Stop();

            if( Alive )
            {
                nav.ResetPath();
                _currentState = AIState.Patroling;
            }
        }
        else if ( _currentState == AIState.LookToFlee || _currentState == AIState.Fleeing )
        {
            Flee();
            Patrol();
        }
        else if( target == null )
        {
            Patrol();
            _currentState = AIState.Patroling;
        }
        else
        {
            ChangeTarget();

            if (target != null)
            {
                float distance = (this.Character.transform.position - target.transform.position).magnitude;

                if (distance < _minDistance)
                {
                    _currentState = AIState.Idel;
                    FireRandomSpell();
                    nav.Stop();

                    Vector3 dirVec = target.transform.position - Character.transform.position;
                    dirVec.Normalize();

                    Character.transform.forward = dirVec;
                }
                else if (distance < _attackDistance)
                {
                    _currentState = AIState.Attacking;
                    nav.SetDestination(target.transform.position);

                    FireRandomSpell();
                    nav.Resume();
                }
                else if (distance < _chaseDistance)
                {
                    _currentState = AIState.Patroling;
                    nav.SetDestination(target.transform.position);
                }
                else
                {
                    target = null;
                }
            }
        }
    }

    protected void FireRandomSpell()
    {
        int rand = Random.Range( 0, 100 );

        if( rand == 1 )
        {
            Character.SwitchLeftSpell();
        }
        else if( rand == 2 )
        {
            Character.SwitchRightSpell();
        }

        rand = Random.Range( 0, 10 );

        if ( rand == 0 )
        {
            Character.ShootSpellOne( Character.transform.forward, this.PlayerID );
        }
        else
        {
            Character.ShootSpellTwo( Character.transform.forward, this.PlayerID );
        }
    }


    protected void HandleAnimations()
    {
        if ( _currentState == AIState.Patroling || _currentState == AIState.Attacking || _currentState == AIState.Fleeing || _currentState ==AIState.LookToFlee )
        {
            Character.CurrentAnimation = CharacterAnimation.Run;
        }
        else if( _currentState == AIState.Idel )
        {
            Character.CurrentAnimation = CharacterAnimation.Idle;
        }
    }

    public void Patrol()
    {
        nav.speed = Character.GetCurrentSpeed() / 2;

        if (nav.remainingDistance < nav.stoppingDistance)
        {
            _patrolIndex = UnityEngine.Random.Range(0, patrolWayPoints.Count - 1);
        }

        nav.destination = patrolWayPoints[_patrolIndex].position;

        nav.Resume();
    }

    protected void ChangeTarget()
    {
        int random = Random.Range( 0, 100 );

        if( random == 50 )
        {
            if( target != null )
            {
                target = null;

                SearchForTarget();
            }
        }
    }
    List<Character> _targetsInRange = new List<Character>();

    protected void SearchForTarget()
    {
        if ( target == null )
        {
            List<Character> charactersInRange = _aiSight.GetCharactersInRange();

            if ( charactersInRange.Count > 0 )
            {
                foreach ( Character otherCharacter in charactersInRange )
                {
                    if ( StaticProperties.Instance.GameType == GameTypes.ARENA || this.Character.Team != otherCharacter.Team )
                    {
                        float distance = ( this.Character.transform.position - otherCharacter.transform.position ).magnitude;

                        if ( distance < _chaseDistance )
                        {
                            _targetsInRange.Add( otherCharacter );
                        }
                    }

                    if( _targetsInRange.Count > 0 )
                    {
                        int randInd = Random.Range( 0, _targetsInRange.Count );
                        target = _targetsInRange[ randInd ];

                        _targetsInRange.Clear();
                    }
                }
            }
            else
            {
                if ( target == null)
                {
                    return;
                }

                if ( !target.Active )
                {
                    target = null;
                    return;
                }
                else if( target.transform != null )
                {
                    float distance = ( this.Character.transform.position - target.transform.position ).magnitude;

                    if ( distance > _chaseDistance )
                    {
                        target = null;
                    }
                }
                else
                {
                    target = null;
                }
            }
        }
    }

    public void AICharacterEventCallback( Character.CharacterEventType t, string sourcePlayerID )
    {
        if ( PlayerID == StaticProperties.Instance.PlayerID || ( this is AICharacter && StaticProperties.Instance.IsHost ) )
        {
            switch ( t )
            {
                case Character.CharacterEventType.DEATH:
                    if ( Alive )
                    {
                        Lives--;
                        Alive = false;
                        RespawnTimer = 5.0f;
                        GameController.Instance.PlayerDied( PlayerID, Lives, Kills );
                        GameController.Instance.SendKill( sourcePlayerID );
                        _currentState = AIState.Dead;
                    }
                    break;
            }
        }
    }

    protected void CheckToFlee()
    {
        if( _currentState != AIState.Dead && Character.StatCurrentHealth < 20  || Character.StatCurrentMana < 20 )
        {
            target = null;
            _currentState = AIState.LookToFlee;
        }
    }

    protected void Flee()
    {
        if( _currentState == AIState.LookToFlee )
        {
            _patrolIndex = UnityEngine.Random.Range(0, patrolWayPoints.Count - 1);
            nav.destination = patrolWayPoints[ _patrolIndex ].position;
            _currentState = AIState.Fleeing;
        }

        if( _currentState == AIState.Fleeing )
        {
            if ( nav.remainingDistance < nav.stoppingDistance )
            {
                _patrolIndex = UnityEngine.Random.Range(0, patrolWayPoints.Count - 1);
                if (Character.StatCurrentMana > 50)
                {
                    _currentState = AIState.Patroling;
                }
            }

            Patrol();
        }
    }
}