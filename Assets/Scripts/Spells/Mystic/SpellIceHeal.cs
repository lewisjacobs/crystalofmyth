using UnityEngine;
using System.Collections;

public class SpellIceHeal : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.HealCharacter(this, character, basePower);
            NetworkCombatHandler.Instance.HealManaCharacter(this, character, basePower);
            NetworkCombatHandler.Instance.CrippleCharacter(this, character, new StatusCripple(33, 4));
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
