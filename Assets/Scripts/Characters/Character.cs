using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class CharacterAnimation
{
    public const int Idle = 0;
    public const int Run = 1;
	public const int StrafeLeft=2;
	public const int StrafeRight=3;
    public const int LeftAttack = 4;
    public const int RightAttack = 5;
    public const int Jump = 6;
	public const int BackwardRun=7;
	public const int Damage=8;
    public const int Death = 9;
}

[Serializable]
[RequireComponent(typeof(AudioSource))]
public class Character : MonoBehaviour
{
    //public bool AI = false;
	public enum CharacterEventType 
	{
		DEATH,
        KILLED
	}

    public int CurrentAnimation { get; set; }

    public delegate void RightCollisionEvent(Collider c);
    public delegate void LeftCollisionEvent(Collider c);
	public delegate void MiddleCollisionEvent(Collider c);
    public delegate void CharacterEvent(CharacterEventType t, string sourcePlayerID);

    public event RightCollisionEvent rightCollision;
    public event LeftCollisionEvent leftCollision;
    public event MiddleCollisionEvent middleCollision;
	public event CharacterEvent characterEvent;

    private float[] leftSpellCooldowns;
    private float[] rightSpellCooldowns;
    private float[] leftSpellNextCasts;
    private float [] rightSpellNextCasts;
    private float middleSpellNextCast;
    private int[] leftSpellManaCosts;
    private int[] rightSpellManaCosts;
    private int middleSpellManaCost;
    public bool frozen { get; private set; }
    public bool PushedBack { get; private set; }
    private float frozenUntil = 0;
    private float nextHealthRegen = 0;
    private float nextManaRegen = 0;
    private float healthRegenCooldown = 0;
    private float manaRegenCooldown = 0;
    private List<Status> currentStatuses;
    private List<Status> statusToRemove;
    private float statCurrentAttack;
    private float statCurrentDefence;
    private float statCurrentCripple;
    private float statCurrentChargeSpeed;
    private float statCurrentWalkSpeed;
    private float statCurrentJumpHeight;
    private bool falling;
    private bool sprinting;
    private bool charging;
    private bool hasted;
    private Type currentSpellAudio;
    private float audioStops;
    private Text nameBar;
	private Image healthBar;

    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    private bool grounded = false;

    public int Team { get; set; }
    public int LeftSpellID { get; private set; }
    public int RightSpellID { get; private set; }
    public float CooldownTimerLeft { get; private set; }
    public float CooldownTimerRight { get; private set; }
    public float CooldownTimerMiddle { get; private set; }
    public int StatCurrentHealth { get; set; }
    public int StatCurrentMana { get; private set; }
    public int checkWait = 0;
    public int statBaseHealth;
    public int statBaseMana;
    public float statBaseAttack;
    public float statBaseDefence;
    public float statBaseWalkSpeed;
    public float statBaseJumpHeight;
    public Spell[] leftSpellPrefabs;
    public Spell[] rightSpellPrefabs;
    public Spell middleSpellPrefab;
    public Transform SpellSpawnLeft;
    public Transform SpellSpawnRight;
    public Transform SpellSpawnMiddle;
    public GameObject Body;
    public GameObject Hands;
    public Hand leftHand;
    public Hand rightHand;
    public Hand middleHand;
    public GameObject Head;
    public bool AddAttack { get; set; }
    public bool AddCripple { get; set; }
    public bool AddDefence { get; set; }
    public bool AddFreeze { get; set; }
    public bool AddCharge { get; set; }
    public bool AddHaste { get; set; }
    public float AttackDuration { get; set; }
    public float CrippleDuration { get; set; }
    public float DefenceDuration { get; set; }
    public float FreezeDuration { get; set; }
    public float ChargeDuration { get; set; }
    public float HasteDuration { get; set; }
	public Animator anim;
	
	public bool Active { get; set; }

    protected Rigidbody rigidbody;

    protected bool Dying = false;
    protected float _DeathTime = 0.0f;

    public string ID { get; set; }

	public void Start()
    {
        this.rigidbody = GetComponent<Rigidbody>();
        Active = true;

		healthBar = this.transform.Find("Name Canvas/Health Bar").gameObject.GetComponent<Image>();
		nameBar = this.transform.Find ("Name Canvas/Name Bar").gameObject.GetComponent<Text> ();
        nameBar.text = name;
        switch(Team)
        {
            case TeamColours.BLUE: nameBar.color = Color.cyan; healthBar.color = Color.cyan; break;
            case TeamColours.RED: nameBar.color = Color.red; healthBar.color = Color.red; break;
            case TeamColours.YELLOW: nameBar.color = Color.yellow; healthBar.color = Color.yellow; break;
            case TeamColours.GREEN: nameBar.color = Color.green; healthBar.color = Color.green; break;
        }

        audioStops = 0;

        LeftSpellID = 0;
        RightSpellID = 0;

        StatCurrentHealth = statBaseHealth;
        StatCurrentMana = statBaseMana;
        statCurrentAttack = statBaseAttack;
        statCurrentDefence = statBaseDefence;
        statCurrentWalkSpeed = statBaseWalkSpeed;
        statCurrentJumpHeight = statBaseJumpHeight;
        statCurrentCripple = 0;
        statCurrentChargeSpeed = 0;

        frozen = false;
        charging = false;
        hasted = false;
        AddAttack = false;
        AddCripple = false;
        AddDefence = false;
        AddFreeze = false;

        leftSpellCooldowns = new float[leftSpellPrefabs.Length];
        leftSpellNextCasts = new float[leftSpellPrefabs.Length];
        leftSpellManaCosts = new int[leftSpellPrefabs.Length];

        rightSpellCooldowns = new float[rightSpellPrefabs.Length];
        rightSpellNextCasts = new float[rightSpellPrefabs.Length];
        rightSpellManaCosts = new int[rightSpellPrefabs.Length];

        for (int i = 0; i < leftSpellPrefabs.Length; i++)
        {
            leftSpellCooldowns[i] = leftSpellPrefabs[i].cooldown;
            leftSpellNextCasts[i] = 0.0f;
            leftSpellManaCosts[i] = leftSpellPrefabs[i].manaCost;
        }

        for (int i = 0; i < rightSpellPrefabs.Length; i++)
        {
            rightSpellCooldowns[i] = rightSpellPrefabs[i].cooldown;
            rightSpellNextCasts[i] = 0.0f;
            rightSpellManaCosts[i] = rightSpellPrefabs[i].manaCost;
        }

        middleSpellNextCast = 0.0f;
        middleSpellManaCost = middleSpellPrefab.manaCost;

        currentStatuses = new List<Status>();
        statusToRemove = new List<Status>();

        //if( !AI )
        //{
            anim = this.transform.Find("Body/Model").gameObject.GetComponent<Animator>();
        //}
		
    }

    void Update()
    {
        DeathTimer();

        if (PushedBack && grounded) PushedBack = false;

        rigidbody.AddForce( new Vector3( 0, -gravity * rigidbody.mass, 0 ) );

        if (healthRegenCooldown > 0) healthRegenCooldown -= Time.deltaTime;
        if (manaRegenCooldown > 0) manaRegenCooldown -= Time.deltaTime;
        if (charging) ChargeForward();

        if ( checkWait > 10 )
        {
            checkWait = 0;
            falling = !Physics.Raycast( transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.4f );
        }
        else
        {
            checkWait++;
        }

        statusToRemove.Clear();
        foreach (Status s in currentStatuses)
        {
            if (GameController.Instance.ElapsedTime > s.removeTime)
            {
                switch(s.type)
                {
                    case Status.statusType.ATTACK:
                        statCurrentAttack -= s.power;
                        break;
                    case Status.statusType.DEFENCE:
                        statCurrentDefence -= s.power;
                        break;
                    case Status.statusType.CRIPPLE:
                        statCurrentCripple -= s.power;
                        break;
                    case Status.statusType.CHARGE:
                        statCurrentChargeSpeed -= s.power;
                        charging = false;
                        break;
                    case Status.statusType.HASTE:
                        hasted = false;
                        break;
                }
                statusToRemove.Add(s);
            }
        }

        foreach (Status s in statusToRemove)
        {
            currentStatuses.Remove(s);
        }

        if(GameController.Instance.ElapsedTime > nextHealthRegen && StatCurrentHealth < statBaseHealth && healthRegenCooldown <= 0)
        {
            Heal(1);
            nextHealthRegen = GameController.Instance.ElapsedTime + 5.0f;
        }

        if (GameController.Instance.ElapsedTime > nextManaRegen && StatCurrentMana < statBaseMana && manaRegenCooldown <= 0)
        {
            HealMana(1);
            nextManaRegen = GameController.Instance.ElapsedTime + 0.1f;
        }

        UpdateStats();
        UpdateCharacterAnimation();
    }

    public float GetCurrentSpeed()
    {
        return statCurrentWalkSpeed;
    }

    public void UpdateCharacterAnimation()
    {
		if(anim != null && Active)
		{
            if (CurrentAnimation == CharacterAnimation.Death || StatCurrentHealth <= 0 )
			{
				anim.SetBool("Death",true);                
			}
            else
            {
                anim.SetBool("Death", false);  
            }
            
			if(CurrentAnimation == CharacterAnimation.Run)
			{
				anim.SetBool("Forward_Run",true);
			}
			else
			{
				anim.SetBool("Forward_Run",false);
			}
			
			if(CurrentAnimation == CharacterAnimation.StrafeLeft)
			{
				anim.SetBool("Left_Strafe",true);
			}
			else
			{
				anim.SetBool("Left_Strafe",false);
			}
			
			if( CurrentAnimation == CharacterAnimation.StrafeRight)
			{
				anim.SetBool("Right_Strafe",true);
			}
			else
			{
				anim.SetBool("Right_Strafe",false);
			}
			
			if ( CurrentAnimation==CharacterAnimation.LeftAttack )
			{
				anim.SetBool("Left_Att",true);
			}
			else
			{
				anim.SetBool("Left_Att",false);
			}
			
			if ( CurrentAnimation==CharacterAnimation.RightAttack )
			{
				anim.SetBool("Right_Att",true);
			}
			else
			{
				anim.SetBool("Right_Att",false);
			}

            if (CurrentAnimation == CharacterAnimation.BackwardRun)
            {
                anim.SetBool("Backward_Run", true);
            }
            else
            {
                anim.SetBool("Backward_Run", false);
            }

			if ( CurrentAnimation==CharacterAnimation.Damage )
			{
				anim.SetBool("Damage",true);
			}
            else
            {
                anim.SetBool( "Damage", false );
            }

			if ( CurrentAnimation==CharacterAnimation.Jump )
			{
				anim.SetBool("Jump",true);
			}
			else
			{
				anim.SetBool("Jump",false);
			}
		}
    }

    public virtual void ApplyCameraXRotation( float fRotation )
    {


    }

    public void UpdateStats()
    {
        if (GameController.Instance.ElapsedTime >= frozenUntil && frozen)
        {
            frozen = false;
        }
        CooldownTimerLeft = leftSpellNextCasts[LeftSpellID] - GameController.Instance.ElapsedTime;
        if (CooldownTimerLeft < 0) CooldownTimerLeft = 0;
        CooldownTimerRight = rightSpellNextCasts[RightSpellID] - GameController.Instance.ElapsedTime;
        if (CooldownTimerRight < 0) CooldownTimerRight = 0;
        CooldownTimerMiddle = middleSpellNextCast - GameController.Instance.ElapsedTime;
        if (CooldownTimerMiddle < 0) CooldownTimerMiddle = 0;
    }

    public void ShootSpellOne(Vector3 camForward, string sourcePlayerID)
    {
        if (GameController.Instance.ElapsedTime > leftSpellNextCasts[LeftSpellID] && StatCurrentMana >= leftSpellManaCosts[LeftSpellID] && !frozen && !charging)
        {
            float cdFactor = 1;
            if (hasted) cdFactor = 0.5f;
			CurrentAnimation=CharacterAnimation.LeftAttack;
            Spell spell = (Spell)GameController.Instance.InstanciatePrefab(leftSpellPrefabs[LeftSpellID], SpellSpawnLeft.position, Quaternion.identity);
            spell.SetCharacter(this, sourcePlayerID);
            spell.SetTeam(Team);
            spell.SetHand(leftHand, 1);
            spell.SetDamageMultiplier(statCurrentAttack);
            spell.SetForward(camForward);
            leftSpellNextCasts[LeftSpellID] = GameController.Instance.ElapsedTime + (spell.cooldown * cdFactor);
            StatCurrentMana -= leftSpellManaCosts[LeftSpellID];
            manaRegenCooldown = 4.0f;
        }
    }

    public void ShootSpellTwo(Vector3 camForward, string sourcePlayerID)
    {
        if (GameController.Instance.ElapsedTime > rightSpellNextCasts[RightSpellID] && StatCurrentMana >= rightSpellManaCosts[RightSpellID] && !frozen && !charging)
        {
            float cdFactor = 1;
            if (hasted) cdFactor = 0.5f;
			CurrentAnimation=CharacterAnimation.RightAttack;
            Spell spell = (Spell)GameController.Instance.InstanciatePrefab(rightSpellPrefabs[RightSpellID], SpellSpawnRight.position, Quaternion.identity);
            spell.SetCharacter(this, sourcePlayerID);
            spell.SetTeam(Team);
            spell.SetHand(rightHand, 2);
            spell.SetDamageMultiplier(statCurrentAttack);
            spell.SetForward(camForward);
            rightSpellNextCasts[RightSpellID] = GameController.Instance.ElapsedTime + (spell.cooldown * cdFactor);
            StatCurrentMana -= rightSpellManaCosts[RightSpellID];
            manaRegenCooldown = 4.0f;
        }
    }

    public void ShootSpellThree(Vector3 camForward, string sourcePlayerID)
    {
        if (GameController.Instance.ElapsedTime > middleSpellNextCast && StatCurrentMana >= middleSpellManaCost && !frozen && !charging)
        {
            float cdFactor = 1;
            if (hasted) cdFactor = 0.5f;
            Spell spell = (Spell)GameController.Instance.InstanciatePrefab(middleSpellPrefab, SpellSpawnMiddle.position, Quaternion.identity);
            spell.SetCharacter(this, sourcePlayerID);
            spell.SetTeam(Team);
            spell.SetHand(middleHand, 3);
            spell.SetDamageMultiplier(statCurrentAttack);
            spell.SetForward(camForward);
            middleSpellNextCast = GameController.Instance.ElapsedTime + (spell.cooldown * cdFactor);
            StatCurrentMana -= middleSpellManaCost;
            manaRegenCooldown = 4.0f;
        }
    }

    public void WalkForward( float horizontal, float vertical, bool jump )
    {
        if ( grounded && !charging && !frozen)
        {
            if ( ( horizontal > 0 || horizontal < 0 ) && ( vertical < 0 || vertical > 0 ) )
            {
                horizontal *= 0.8f;
                vertical *= 0.8f;
            }

            float movement = statCurrentWalkSpeed;
            if (statCurrentCripple == 0)
            {
                if ((sprinting && horizontal < 0.15f && horizontal > -0.15f && vertical > 0) || hasted)
                    movement *= 1.4f;
            }
            else
            {
                movement *= (1 - statCurrentCripple);
            }
            if (vertical < -0.1f) movement *= 0.75f;

            if (movement < 2f) movement = 2f;


            Vector3 targetVelocity = new Vector3( horizontal, 0, vertical );
            targetVelocity = transform.TransformDirection( targetVelocity );
            targetVelocity *= movement;

            Vector3 velocity = rigidbody.velocity;
            Vector3 velocityChange = ( targetVelocity - velocity );

            velocityChange.x = Mathf.Clamp( velocityChange.x, -maxVelocityChange, maxVelocityChange );
            velocityChange.z = Mathf.Clamp( velocityChange.z, -maxVelocityChange, maxVelocityChange );
            velocityChange.y = 0;
            rigidbody.AddForce( velocityChange, ForceMode.VelocityChange );

            if ( jump )
            {
                rigidbody.velocity = new Vector3( velocity.x, CalculateJumpVerticalSpeed(), velocity.z );
            }
        }

        grounded = false;

        WalkAnimations( horizontal, vertical, jump );
    }

    protected void WalkAnimations( float horizontal, float vertical, bool jump )
    {
        if (jump)
        {
            CurrentAnimation = CharacterAnimation.Jump;
        }
        else if( horizontal > 0 )
        {
            CurrentAnimation = CharacterAnimation.StrafeRight;
        }
        else if( horizontal < 0 )
        {
            CurrentAnimation = CharacterAnimation.StrafeLeft;
        }
        else if( vertical > 0 )
        {
            CurrentAnimation = CharacterAnimation.Run;
        }   
        else if( vertical < 0 )
        {
            CurrentAnimation = CharacterAnimation.BackwardRun;
        }
    }

    public void OnCollisionStay()
    {
        grounded = true;
    }

    public float CalculateJumpVerticalSpeed()
    {
        return Mathf.Sqrt( 2 * statCurrentJumpHeight * gravity );
    }

    public void ChargeForward()
    {
        float movement = statCurrentWalkSpeed + statCurrentChargeSpeed; 
		CurrentAnimation=CharacterAnimation.Run;
        this.GetComponent<Rigidbody>().velocity += this.Body.transform.forward * movement;
        
        if (this.GetComponent<Rigidbody>().velocity.magnitude > movement)
        {
            this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity.normalized * movement; 
        }
    }

    public void Sprint(bool b)
    {
        sprinting = b;
    }

    public void SwitchLeftSpell()
    {
        if (LeftSpellID < leftSpellPrefabs.Length - 1)
            LeftSpellID++;
        else LeftSpellID = 0;
    }

    public void SwitchRightSpell()
    {
        if (RightSpellID < rightSpellPrefabs.Length - 1)
            RightSpellID++;
        else RightSpellID = 0;
    }

	public void Respawn()
	{
		CooldownTimerLeft = 0;
		CooldownTimerRight = 0;
		CooldownTimerMiddle = 0;
		for (int index = 0; index < rightSpellNextCasts.Length; index++)
			rightSpellNextCasts [index] = 0;
		
		for (int index = 0; index < leftSpellNextCasts.Length; index++)
			leftSpellNextCasts [index] = 0;

		middleSpellNextCast = 0;

		frozen = false;
		frozenUntil = 0;
		nextHealthRegen = 0;
		nextManaRegen = 0;
		healthRegenCooldown = 0;
		manaRegenCooldown = 0;
		currentStatuses.Clear ();
		statusToRemove.Clear();
		statCurrentAttack = statBaseAttack;
		statCurrentDefence = statBaseDefence;
		statCurrentCripple = 0;
		statCurrentChargeSpeed = 0;
		statCurrentWalkSpeed = statBaseWalkSpeed;
		statCurrentJumpHeight = statBaseJumpHeight;
		falling = false;
		sprinting = false;
		charging = false;
		hasted = false;
		currentSpellAudio = null;
		audioStops = 0;
		
		LeftSpellID = 0;
		RightSpellID = 0;
		CooldownTimerLeft = 0;
		CooldownTimerRight = 0;
		CooldownTimerMiddle = 0;
		StatCurrentHealth = 0;
		StatCurrentMana = 0;
		checkWait = 0;
		AddAttack = false;
		AddCripple = false;
		AddDefence = false;
		AddFreeze = false;
		AddCharge = false;
		AddHaste = false;
		AttackDuration = 0;
		CrippleDuration = 0;
		DefenceDuration = 0;
		FreezeDuration = 0;
		ChargeDuration = 0;
		HasteDuration = 0;
		
		Heal (statBaseHealth);
		HealMana (statBaseMana);

        Vector3 position = GameController.Instance.GetSpawnPoint(true);
		this.transform.position = position;

        SetActive();
    }

    public void TakeDamage(MessageData data)
    {
        TakeDamage(data.Get<int>(0), data.Get<string>(1));
    }

    public void TakeDamage(int amount, string sourcePlayerID)
    {
        healthRegenCooldown = 4.0f;
        amount = (int)(Mathf.Round(amount * (1 - statCurrentDefence)));
        StatCurrentHealth -= amount;
		CurrentAnimation= CharacterAnimation.Damage;
        if (StatCurrentHealth < 0) StatCurrentHealth = 0;

        float healthPercentage = (100f / statBaseHealth) * StatCurrentHealth;
        if (healthPercentage >= 0)
            healthBar.fillAmount = healthPercentage / 100f;

        if (StatCurrentHealth <= 0)
        {
            CurrentAnimation = CharacterAnimation.Death;

            if (characterEvent != null)
            {   
                characterEvent(CharacterEventType.DEATH, sourcePlayerID);
            }

            StartDeathTimer();
        }
    }

    protected void StartDeathTimer()
    {
        Dying = true;
        _DeathTime = 0.0f;

        if ( healthBar != null ) healthBar.gameObject.SetActive( false );
        if ( nameBar != null ) nameBar.gameObject.SetActive( false );
    }

    protected void DeathTimer()
    {
        if( Dying )
        {
            _DeathTime += Time.deltaTime;

            if( _DeathTime > 2.0f )
            {
                Dying = false;
                SetInactive();
            }
        }

    }

    public void Heal(MessageData data)
    {
        Heal(data.Get<int>(0));
    }

    public void Heal(int amount)
    {
        StatCurrentHealth += amount;
        if (StatCurrentHealth > statBaseHealth) StatCurrentHealth = statBaseHealth;
        float healthPercentage = (100f / statBaseHealth) * StatCurrentHealth;
        if (healthPercentage >= 0)
            healthBar.fillAmount = healthPercentage / 100f;
    }

    public void HealMana(MessageData data)
    {
        HealMana(data.Get<int>(0));
    }

    public void HealMana(int amount)
    {
        StatCurrentMana += amount;
        if (StatCurrentMana > statBaseMana) StatCurrentMana = statBaseMana;
    }

    public void Cripple( MessageData data )
    {
        Cripple( data.Get<StatusCripple>( 0 ) );
    }

    public void Cripple(StatusCripple status)
    {
        AddCripple = true;
        CrippleDuration = status.duration;
        currentStatuses.Add(status);
        statCurrentCripple = statCurrentCripple + status.power;
    }

    public void Freeze( MessageData data )
    {
        Freeze( data.Get<StatusFreeze>( 0 ) );
    }

    public void Freeze(StatusFreeze status)
    {
        AddFreeze = true;
        float newFrozen = status.duration + GameController.Instance.ElapsedTime;
        FreezeDuration = status.duration;
        if (newFrozen > frozenUntil) frozenUntil = newFrozen;
        frozen = true;
    }

    public void BuffAttack(MessageData data)
    {
        BuffAttack(data.Get<StatusAttack>(0));
    }

    public void BuffAttack(StatusAttack status)
    {
        AddAttack = true;
        AttackDuration = status.duration;
        currentStatuses.Add(status);
        statCurrentAttack = statCurrentAttack + status.power;
    }

    public void BuffDefence(MessageData data)
    {
        BuffDefence(data.Get<StatusDefence>(0));
    }

    public void BuffDefence(StatusDefence status)
    {
        AddDefence = true;
        DefenceDuration = status.duration;
        currentStatuses.Add(status);
        statCurrentDefence = statCurrentDefence + status.power;
    }
    
    public void Charge(MessageData data)
    {
        Charge(data.Get<StatusCharge>(0));
    }
    
    public void Charge(StatusCharge status)
    {
        charging = true;
        AddCharge = true;
        ChargeDuration = status.duration;
        currentStatuses.Add(status);
        statCurrentChargeSpeed = status.power;
    }

    public void Haste(MessageData data)
    {
        Haste(data.Get<StatusHaste>(0));
    }

    public void Haste(StatusHaste status)
    {
        hasted = true;
        AddHaste = true;
        HasteDuration = status.duration;
        currentStatuses.Add(status);
    }

    public void PushBack( MessageData data )
    {
        PushBack( data.Get<int>( 0 ), data.Get( 1 ) );
    }

    public void PushBack(int amount, Vector3 direction)
    {
        PushedBack = true;
        this.GetComponent<Rigidbody>().AddForce(direction.x * amount, 2.5f, direction.z * amount, ForceMode.VelocityChange);
    }

    public void FireRightCollision(Collider c)
    {
        if (rightCollision != null)
        {
            rightCollision(c);
        }
    }

    public void FireLeftCollision(Collider c)
    {
        if (leftCollision != null)
        {
            leftCollision(c);
        }
    }

    public void FireMiddleCollision(Collider c)
    {
        if (middleCollision != null)
        {
            middleCollision(c);
        }
    }

    public bool CameraCanRotate()
    {
        if (!charging && !frozen) return true;
        return false;
    }

    public void PlaySound(AudioClip castSound, Type spell)
    {
        if (castSound != null)
        {
            if (spell != currentSpellAudio || GameController.Instance.ElapsedTime > audioStops)
            {
                currentSpellAudio = spell;
                GetComponent<AudioSource>().PlayOneShot(castSound);
                audioStops = GameController.Instance.ElapsedTime + castSound.length;
            }
        }
    }

    public void StopAudio()
    {
        currentSpellAudio = null;
        GetComponent<AudioSource>().Stop();
        audioStops = 0;
    }

	public void Idle()
	{
		CurrentAnimation=CharacterAnimation.Idle;
	}
		
	public void RefreshHeathBar()
    {
        if( healthBar != null )
        {
            float healthPercentage = ( 100f / statBaseHealth ) * StatCurrentHealth;
            if ( healthPercentage >= 0 )
                healthBar.fillAmount = healthPercentage / 100f; 
        }
    }

    public void SetNameMessage( MessageData data )
    {
        SetName( data.Get<string>( 0 ), data.Get<int>(1) );
    }

    public void SetName( string name, int team )
    {
        this.name = name;
        if( nameBar != null )
        {
            nameBar.text = name;
            switch (team)
            {
                case TeamColours.BLUE: nameBar.color = Color.cyan; healthBar.color = Color.cyan; break;
                case TeamColours.RED: nameBar.color = Color.red; healthBar.color = Color.red; break;
                case TeamColours.YELLOW: nameBar.color = Color.yellow; healthBar.color = Color.yellow; break;
                case TeamColours.GREEN: nameBar.color = Color.green; healthBar.color = Color.green; break;
            }
        }
    }
    public void SetTeam(int team)
    {
        this.Team = team;
    }
	
	public void SetActive()
    {
        Active = true;
        Body.SetActive( Active );
        Hands.SetActive( Active );
        leftHand.gameObject.SetActive( Active );
        rightHand.gameObject.SetActive( Active );
        middleHand.gameObject.SetActive( Active );
        Head.gameObject.SetActive( Active );

        if ( healthBar != null ) healthBar.gameObject.SetActive( Active );
        if ( nameBar != null ) nameBar.gameObject.SetActive( Active );
    }

    public void SetInactive()
    {
        Active = false;
        Body.SetActive( Active );
        Hands.SetActive( Active );
        leftHand.gameObject.SetActive( Active );
        rightHand.gameObject.SetActive( Active );
        middleHand.gameObject.SetActive( Active );
        Head.gameObject.SetActive( Active );

        if( healthBar != null ) healthBar.gameObject.SetActive( Active );
        if ( nameBar != null ) nameBar.gameObject.SetActive( Active );
    }
}
