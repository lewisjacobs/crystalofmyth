using UnityEngine;
using System.Collections;

public class SpellHasten : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            NetworkCombatHandler.Instance.HasteCharacter(this, character, new StatusHaste(0, 5f));
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
