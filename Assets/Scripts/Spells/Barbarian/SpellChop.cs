using UnityEngine;
using System.Collections;

public class SpellChop : Spell
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
            if (c != null)
            {
                NetworkCombatHandler.Instance.DamageCharacter(this, c, currentDamage, sourcePlayerID);

                character.middleCollision -= SpellCollision;

                if (this != null)
                    GameController.Instance.DestroyObject(this.gameObject);

                hand.DisableCollider();
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
