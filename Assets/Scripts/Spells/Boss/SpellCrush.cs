using UnityEngine;
using System.Collections;

public class SpellCrush : Spell
{    
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            hand = character.leftHand;
            hand.EnableCollider();
            character.leftCollision += SpellCollision;
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

                character.leftCollision -= SpellCollision;

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
                character.leftCollision -= SpellCollision;
                hand.DisableCollider();
                GameController.Instance.DestroyObject(this.gameObject);
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }
}
