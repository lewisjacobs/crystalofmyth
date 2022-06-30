using UnityEngine;
using System.Collections;

public class SpellShieldBash : Spell
{    
    public override void Start()
    {
        if (character != null)
        {
            hand.EnableCollider();
            character.leftCollision += SpellCollision;
            base.Start();
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
                NetworkCombatHandler.Instance.FreezeCharacter(this, c, new StatusFreeze(0, 2.0f));
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
                this.transform.position = character.transform.position;

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
