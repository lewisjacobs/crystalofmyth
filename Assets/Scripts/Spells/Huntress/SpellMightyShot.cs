using UnityEngine;
using System.Collections;

public class SpellMightyShot : Spell
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
            if (c != null && !hitColliders.Contains(other))
            {
                hitColliders.Add(other);
                NetworkCombatHandler.Instance.PushbackCharacter(this, c, currentDamage, this.transform.forward);
            }
        }
    }
}
