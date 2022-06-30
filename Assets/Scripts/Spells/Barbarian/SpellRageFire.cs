using UnityEngine;
using System.Collections;

public class SpellRageFire : Spell
{    
    public override void Start()
    {
        base.Start();
        PlayAudio(true);
        if (character != null)
        {
            currentDamage += (character.statBaseHealth - character.StatCurrentHealth) / 4;
        }
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

            if ( !_destroyed )
            {
                _destroyed = true;
                GameController.Instance.DestroyObject( this.gameObject );
            }
        }
    }
}
