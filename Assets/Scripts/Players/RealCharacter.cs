using UnityEngine;
using System;
using System.Collections;
using PigeonCoopToolkit.TIM;
using PigeonCoopToolkit;

[Serializable]
public class RealCharacter : CharacterPlayer
{
	public Camera PlayerCamera { get; set; }

	protected float _fRotationSensitivity = 5.0f;
    private float openMenuCooldown = 0.2f;
    private bool deathCameraActive = false;

    public RealCharacter()
    {
        IsAI = false;
    }

	public void SetCharacter(Character c)
	{
        InitialiseStats();
        Character = c;
        Character.ID = PlayerID;
        Character.characterEvent += CharacterEventCallback;
	}

	public override void Update() 
	{
        base.Update();
        if (openMenuCooldown > 0) openMenuCooldown -= Time.deltaTime;
        if (switchLeftCooldown > 0) switchLeftCooldown -= Time.deltaTime;
        if (switchRightCooldown > 0) switchRightCooldown -= Time.deltaTime;
		
        if (RespawnTimer <= 0 && !Alive && Lives > 0)
		{
			Alive = true;
			Character.Respawn();
		}
        else if( RespawnTimer >= 1.0 && RespawnTimer <= 1.5f && !Alive )
        {
            Vector3 position = GameController.Instance.GetSpawnPoint(true);
            Character.transform.position = position;
        }
        else if (RespawnTimer <= 0 && !Alive && Lives <= 0 && !deathCameraActive)
        {
            deathCameraActive = true;
            if (StaticProperties.Instance.GameType == GameTypes.ADVENTURE)
            {
                PlayerCamera.transform.position = new Vector3(335, 20, 365);
                PlayerCamera.transform.eulerAngles = new Vector3(17, 165, 0);
            }
            else
            {
                PlayerCamera.transform.position = new Vector3(0, 100, -10);
                PlayerCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            }
            UIController.Instance.FadeInScreen();
            Transform hud = GameObject.Find("Character HUD").transform;
            foreach (Transform child in hud)
            {
                if (child.name != "Lives Text")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
		idle = true;
        if( Character != null)
        {
            UIController.Instance.UpdateUI( Character.StatCurrentHealth, Character.statBaseHealth, Character.StatCurrentMana, Character.statBaseMana, Character.CooldownTimerLeft,
                Character.CooldownTimerRight, Character.CooldownTimerMiddle, Character.LeftSpellID, Character.RightSpellID, Character.AddAttack, Character.AddCripple, Character.AddDefence, 
                Character.AddFreeze, Character.AddCharge, Character.AddHaste, Character.AttackDuration, Character.CrippleDuration, Character.DefenceDuration, Character.FreezeDuration,
                Character.ChargeDuration, Character.HasteDuration, Alive, deathCameraActive);
            Character.AddAttack = false;
            Character.AddCripple = false;
            Character.AddDefence = false;
            Character.AddFreeze = false;
            Character.AddCharge = false;
            Character.AddHaste = false;
            //input stuff goes here to change character;
            if (Alive && !BattleController.Instance.MenuActive && !AdventureController.Instance.MenuActive)
            {
                if (!GameController.Instance.UseTouchControls)
                {
                    if ( Input.GetAxis( "Horizontal" ) != 0.0f || Input.GetAxis( "Vertical" ) != 0.0f || Input.GetButton( "Jump" ) )
                    {
                        Character.WalkForward( Input.GetAxis( "Horizontal" ), Input.GetAxis( "Vertical" ), Input.GetButton( "Jump" ) );
                        idle = false;
                    }

                    if (Input.GetButton("FireLeft"))
                    {
                        Character.ShootSpellOne(PlayerCamera.transform.forward, PlayerID);
                        idle = false;
                    }

                    if (Input.GetButton("FireRight"))
                    {
                        Character.ShootSpellTwo(PlayerCamera.transform.forward, PlayerID);
                        idle = false;
                    }
                    if (Input.GetButton("FireMiddle"))
                    {
                        Character.ShootSpellThree(PlayerCamera.transform.forward, PlayerID);
                    }
                    if ((Input.GetButton("SwitchLeft") || Input.GetAxis("SwitchSpellAxis") == 1) && switchLeftCooldown <= 0)
                    {
                        Character.SwitchLeftSpell();
                        switchLeftCooldown = 0.25f;
                    }
                    if ((Input.GetButton("SwitchRight") || Input.GetAxis("SwitchSpellAxis") == -1) && switchRightCooldown <= 0)
                    {
                        Character.SwitchRightSpell();
                        switchRightCooldown = 0.25f;
                    }
                    if (Input.GetButton("Suicide"))
                    {
                        Character.TakeDamage(1000, "-1");
                    }
                    if (Input.GetButtonDown("Sprint"))
                    {
                        Character.Sprint(true);
                    }
                    if (Input.GetButtonUp("Sprint"))
                    {
                        Character.Sprint(false);
                    }
                    if ( ( Input.GetButton("Cancel") || Input.GetKey( KeyCode.Escape ) ) && openMenuCooldown <= 0)
                    {
#if  UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
#else
                        TouchInput.SetInactive( "Main" );
#endif
                        openMenuCooldown = 0.2f;
                        if( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
                        {
                            AdventureController.Instance.OpenMenu();
                        }
                        else
                        {
                            BattleController.Instance.OpenMenu();
                        }
                    }
                    if (idle)
                    {
                        Character.Idle();
                    }

                    //Handle Mouse Input
                    RotateCamera(Input.GetAxis("Mouse X") * _fRotationSensitivity, Input.GetAxis("Mouse Y") * _fRotationSensitivity);
                }
                else
                {
                    Vector2 movement = TouchInput.JoystickValue("Main", "Left");

                    Character.WalkForward( movement.x, movement.y, ( TouchInput.GetButton( "Main", "Jump" ) || TouchInput.GetButton( "Main", "Jump2" ) ) );

                    if ( TouchInput.GetButton( "Main", "FireLeft" ) && TouchInput.GetButton( "Main", "FireRight" ) )
                    {
                        Character.ShootSpellThree(PlayerCamera.transform.forward, PlayerID);
                    }
                    else if ( TouchInput.GetButton( "Main", "FireLeft" ) )
                    {
                        Character.ShootSpellOne(PlayerCamera.transform.forward, PlayerID);
                        idle = false;
                    }
                    else if ( TouchInput.GetButton( "Main", "FireRight" ) )
                    {
                        Character.ShootSpellTwo(PlayerCamera.transform.forward, PlayerID);
                        idle = false;
                    }

                    if ( TouchInput.GetButton( "Main", "SwitchLeft" ) && switchLeftCooldown <= 0 )
                    {
                        Character.SwitchLeftSpell();
                        switchLeftCooldown = 0.25f;
                    }
                    if ( TouchInput.GetButton( "Main", "SwitchRight" ) && switchRightCooldown <= 0 )
                    {
                        Character.SwitchRightSpell();
                        switchRightCooldown = 0.25f;
                    }


                    Vector2 axis = TouchInput.JoystickValue("Main", "Right");
                    RotateCamera(axis.x * _fRotationSensitivity, axis.y * _fRotationSensitivity);
                }
            }
            else
            {
                if (Input.GetButton("Cancel") && openMenuCooldown <= 0)
                {
                    openMenuCooldown = 0.2f;
                    if ( StaticProperties.Instance.GameType == GameTypes.ADVENTURE )
                    {
                        if ( AdventureController.Instance.MenuActive )
                            AdventureController.Instance.CloseMenu();
                        else
                            AdventureController.Instance.OpenMenu();
                    }
                    else
                    {
                        if ( BattleController.Instance.MenuActive )
                            BattleController.Instance.CloseMenu();
                        else
                            BattleController.Instance.OpenMenu();
                    }
                }
            }
        }
	}
	
	public void RotateCamera(float amount, float amount2)
	{
        float xVal = PlayerCamera.transform.localEulerAngles.x - amount2;

        if (xVal > 180.0f && xVal < 320.0f)
        {
            xVal = 320.0f;
        }
        else if (xVal > 40.0f && xVal < 180.0f)
        {
            xVal = 40.0f;
        }

        float yVal = PlayerCamera.transform.localEulerAngles.y + amount;
        if (Character.CameraCanRotate())
            PlayerCamera.transform.rotation = Quaternion.Euler(xVal, yVal, 0.0f);

        if (Character != null)
        {
            PlayerCamera.transform.position = Character.Head.transform.position;
            if (Character.CameraCanRotate())
            {
                Character.ApplyCameraXRotation(xVal);
                Character.transform.rotation = Quaternion.Euler(0.0f, yVal, 0.0f);
                Character.Hands.transform.rotation = Quaternion.Euler(xVal, yVal, 0.0f);
            }
        }
    }
}