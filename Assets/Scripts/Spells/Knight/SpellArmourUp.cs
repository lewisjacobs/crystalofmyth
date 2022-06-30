using UnityEngine;
using System.Collections;

public class SpellArmourUp : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.BuffCharacterAttack(this, character, new StatusAttack(basePower, 20));
            NetworkCombatHandler.Instance.BuffCharacterDefence(this, character, new StatusDefence(basePower, 20));
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
