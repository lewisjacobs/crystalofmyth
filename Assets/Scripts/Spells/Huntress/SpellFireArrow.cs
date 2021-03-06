using UnityEngine;
using System.Collections;

public class SpellFireArrow : Spell
{
    public override void Start()
    {
        base.Start();
        PlayAudio(true);
    }

    public override void OnTriggerEnter(Collider other)
    {
        Character c = (other.gameObject).GetComponent<Character>();
        
        if (CheckIfSpellShouldCollide(other))
        {
            if (c != null)
            {
                NetworkCombatHandler.Instance.DamageCharacter(this, c, currentDamage, sourcePlayerID);
            }
            else
            {
                if ( !_destroyed )
                {
                    _destroyed = true;
                    GameController.Instance.DestroyObject( this.gameObject );
                }
            }
        }
    }
}
