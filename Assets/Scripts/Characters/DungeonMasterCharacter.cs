using UnityEngine;
using System.Collections;

public class DungeonMasterCharacter : MonoBehaviour 
{
	public Spell FireballPrefab;
    public Spell IceRainPrefab;
    public Spell MeteorPrefab;
    public Spell PolarityPrefab;

    public float[] SpellCooldowns { get; private set; }

    void Start()
    {
        SpellCooldowns = new float[4] {0, 0, 0, 0};
    }

    public void Update()
    {
        for (int i = 0; i < SpellCooldowns.Length; i++)
        {
            if (SpellCooldowns[i] > 0)
                SpellCooldowns[i] -= Time.deltaTime;
        }
    }

	public void FireMeteor(string playerID)
	{
        if (SpellCooldowns[0] <= 0.0f)
        {
            SpellDMMeteor fireball = (SpellDMMeteor)GameController.Instance.InstanciatePrefab(MeteorPrefab, this.transform.position, new Quaternion());
            fireball.SetForward(this.transform.forward);
            fireball.SetID( playerID );

            SpellCooldowns[0] = 7.5f;
        }
	}

    public void FireIceRain( string playerID )
    {
        if (SpellCooldowns[1] <= 0.0f)
        {
            SpellDMIceRain fireball = ( SpellDMIceRain ) GameController.Instance.InstanciatePrefab( IceRainPrefab, this.transform.position, new Quaternion() );
            fireball.SetForward( this.transform.forward );
            fireball.SetID( playerID );
            SpellCooldowns[1] = 10.0f;
        }
    }

    public void FireFireball( string playerID )
    {
        if (SpellCooldowns[2] <= 0.0f)
        {
            SpellFireball fireball = (SpellFireball)GameController.Instance.InstanciatePrefab(FireballPrefab, this.transform.position, new Quaternion());
            fireball.SetForward( this.transform.forward );
            fireball.SetID( playerID );
            SpellCooldowns[2] = 0.5f;
        }
    }
    public void FirePolarity( string playerID )
    {
        if (SpellCooldowns[3] <= 0.0f)
        {
            SpellDMPolarity fireball = (SpellDMPolarity)GameController.Instance.InstanciatePrefab(PolarityPrefab, this.transform.position, new Quaternion());
            fireball.SetForward( this.transform.forward );
            fireball.SetID( playerID );
            SpellCooldowns[3] = 10.0f;
        }
    }
    
    public void SetForward( Vector3 forward )
    {
        this.transform.forward = forward;
    }
}
