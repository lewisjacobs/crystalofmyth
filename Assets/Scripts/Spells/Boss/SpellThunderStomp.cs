using UnityEngine;
using System.Collections;

public class SpellThunderStomp : Spell
{    
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            hand = character.middleHand;
            hand.EnableCollider();
            character.middleCollision += SpellCollision;
            GetComponent<AudioSource>().volume = 0.5f;
            PlayAudio(false);
        }
    }

    public void SpellCollision(Collider other)
    {
        Character c = (other.gameObject).GetComponent<Character>();

        if (CheckIfSpellShouldCollide(other))
        {
            if (c != null && !hitColliders.Contains(other))
            {
                hitColliders.Add(other);
                NetworkCombatHandler.Instance.CrippleCharacter(this, c, new StatusCripple(50, 5.0f));
                NetworkCombatHandler.Instance.FreezeCharacter(this, c, new StatusFreeze(0.0f, 2.0f));
                NetworkCombatHandler.Instance.DamageCharacter(this, c, currentDamage, sourcePlayerID);
            }
        }
    }

    public override void Update()
    {
        lifetime -= Time.deltaTime;

        if (lifetime <= 0)
        {
            if (!StaticProperties.Instance.MultiPlayer || (view != null && view.isMine))
            {
                character.middleCollision -= SpellCollision;
                hand.DisableCollider();
                GameController.Instance.DestroyObject(this.gameObject);
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }
}
