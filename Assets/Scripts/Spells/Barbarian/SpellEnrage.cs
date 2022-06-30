using UnityEngine;
using System.Collections;

public class SpellEnrage : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.BuffCharacterAttack(this, character, new StatusAttack(character.StatCurrentHealth / 2, 20));
            NetworkCombatHandler.Instance.DamageCharacter(this, character, character.StatCurrentHealth / 3, sourcePlayerID);
            PlayAudio(false);
        }
    }
    
    public override void Update()
    {
        base.Update();
        if (!StaticProperties.Instance.MultiPlayer || (view != null && view.isMine))
        {
            this.transform.position = character.transform.position;
        }
    }
}
