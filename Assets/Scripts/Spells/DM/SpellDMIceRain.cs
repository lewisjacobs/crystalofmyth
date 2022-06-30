using UnityEngine;
using System.Collections;

public class SpellDMIceRain : Spell
{
    public ParticleSystem Ball;
    public ParticleSystem Explostion;
    public Collider ExpolosionRange;

    protected float _destroyTime = 0.0f;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        if ( Exploding )
        {
            if( _destroyTime == 0.0f )
            {
                Explostion.gameObject.SetActive( true );
                Explostion.Play();
                Ball.gameObject.SetActive( false );
            }

            _destroyTime += Time.deltaTime;

            if ( _destroyTime > 5.0f )
            {
                GameController.Instance.DestroyObject( this.gameObject );
            }
        }

        this.transform.rotation = new Quaternion();
    }

    public override void OnTriggerEnter(Collider other)
    {
        Character c = ( other.gameObject ).GetComponent<Character>();

        if ( CheckIfSpellShouldCollide( other ) )
        {
            if ( c != null )
            {                
                NetworkCombatHandler.Instance.CrippleCharacter( this, c, new StatusCripple( 30, 2.0f ) );
                NetworkCombatHandler.Instance.DamageCharacter( this, c, currentDamage, sourcePlayerID );                
            }
        }
    }

    public void OnCollisionEnter( Collision collision )
    {
        Exploding = true;
    }
}
