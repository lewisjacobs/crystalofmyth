using UnityEngine;
using System.Collections;

public class SpellBlock : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.BuffCharacterDefence(this, character, new StatusDefence(basePower, 3));
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
