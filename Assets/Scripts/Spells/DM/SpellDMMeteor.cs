using UnityEngine;
using System.Collections;

public class SpellDMMeteor : Spell
{
    public ParticleSystem Ball;
    public ParticleSystem Explostion;
    public BoxCollider ExpolosionRange;

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
                Explostion.Emit( 1000 );
                Ball.gameObject.SetActive( false );

                if( RemoteFix )
                {
                    this.transform.rotation = new Quaternion();
                    Explostion.transform.rotation = new Quaternion( 0.0f, 0.0f, 90.0f, 1.0f );
                }
            }

            _destroyTime += Time.deltaTime;

            if ( _destroyTime > 2.0f )
            {
                GameController.Instance.DestroyObject( this.gameObject );
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        Character c = ( other.gameObject ).GetComponent<Character>();

        if ( CheckIfSpellShouldCollide(other) )
        {
            if ( c != null )
            {
                NetworkCombatHandler.Instance.DamageCharacter( this, c, currentDamage, sourcePlayerID );
            }
        }
    }

    public void OnCollisionEnter( Collision collision )
    {
        Exploding = true;
    }
}
