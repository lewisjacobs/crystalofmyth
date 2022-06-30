using UnityEngine;
using System.Collections;

public class SpellVitality : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.HealCharacter(this, character, (character.statBaseHealth - character.StatCurrentHealth) / 2);
            PlayAudio(false);
        }
    }

    public override void Update()
    {
        base.Update();

        if( character != null )
        {
            this.transform.position = character.transform.position;
        }
    }
}
