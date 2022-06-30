using UnityEngine;
using System.Collections;

public class SpellDMPolarity : Spell
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
                    Explostion.transform.eulerAngles = new Vector3( 90.0f, 0.0f, 0.0f );
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
                Vector3 cPos = c.transform.position;

                Vector3 direction = (cPos - this.transform.position).normalized;

                NetworkCombatHandler.Instance.PushbackCharacter(this, c, basePower, -direction);
                NetworkCombatHandler.Instance.DamageCharacter( this, c, currentDamage, sourcePlayerID );
            }
        }
    }

    public void OnCollisionEnter( Collision collision )
    {
        Exploding = true;
    }
}
