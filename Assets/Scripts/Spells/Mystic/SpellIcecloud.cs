using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellIcecloud : Spell
{
    private List<float> timeSinceStay = new List<float>();
    public override void Start()
    {
        base.Start();
        PlayAudio(true);
    }

    public override void OnTriggerEnter(Collider other)
    {

    }

    public void OnTriggerStay(Collider other)
    {
        int index;
        if(!hitColliders.Contains(other))
        {
            index = hitColliders.Count;
            hitColliders.Add(other);
            timeSinceStay.Add(0.0f);
        }
        else
        {
            index = hitColliders.IndexOf(other);
        }

        Character c = (other.gameObject).GetComponent<Character>();

        if (CheckIfSpellShouldCollide(other))
        {
            if (c != null && timeSinceStay[index] < GameController.Instance.ElapsedTime)
            {
                timeSinceStay.Add(GameController.Instance.ElapsedTime + 0.5f);
                NetworkCombatHandler.Instance.CrippleCharacter(this, c, new StatusCripple(50, 0.5f));
                NetworkCombatHandler.Instance.DamageCharacter(this, c, currentDamage, sourcePlayerID);
                timeSinceStay[index] = GameController.Instance.ElapsedTime + 0.5f;
            }
        }
    }
}
