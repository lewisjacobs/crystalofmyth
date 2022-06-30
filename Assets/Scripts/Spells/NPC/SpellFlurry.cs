using UnityEngine;
using System.Collections;

public class SpellFlurry : Spell
{
    private int attacksLeft;
    private float impactTimer;
    private Character other;

    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            hand.EnableCollider();
            character.rightCollision += SpellCollision;
            GetComponent<AudioSource>().volume = 0.5f;
        }
    }

    public void SpellCollision(Collider other)
    {
        Character c = (other.gameObject).GetComponent<Character>();

        if (CheckIfSpellShouldCollide(other))
        {
            if (c != null)
            {
                impactTimer = 0.0f;
                attacksLeft = 4;
                this.other = c;
                character.rightCollision -= SpellCollision;
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
                character.rightCollision -= SpellCollision;
                hand.DisableCollider();
            }
        }

        if (impactTimer > 0.0f)
        {
            impactTimer -= Time.deltaTime;
        }
        else
        {
            impactTimer = 0.10f;
            attacksLeft--;
            PlayAudio(false);
            if (other != null)
                NetworkCombatHandler.Instance.DamageCharacter(this, other, currentDamage, sourcePlayerID);
            
            if (attacksLeft <= 0)
            {
                if (this != null)
                    GameController.Instance.DestroyObject(this.gameObject);
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        
    }
}
