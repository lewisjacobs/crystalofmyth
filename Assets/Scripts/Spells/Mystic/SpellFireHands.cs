using UnityEngine;
using System.Collections;

public class SpellFireHands : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            GetComponent<AudioSource>().volume = 0.2f;
            hand.EnableCollider();
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
                if (handIndex == 1)
                    character.leftCollision -= SpellCollision;
                else
                    character.rightCollision -= SpellCollision;
                hand.DisableCollider();
                GameController.Instance.DestroyObject(this.gameObject);
            }
        }

        if (!StaticProperties.Instance.MultiPlayer || (view != null && view.isMine))
        {
            this.transform.position = hand.transform.position;
            this.transform.rotation = hand.transform.rotation;
        }
    }

    public override void SetHand(Hand h, int handIndex)
    {
        base.SetHand(h, handIndex);
        if (handIndex == 1) character.leftCollision += SpellCollision;
        else character.rightCollision += SpellCollision;
    }

    public override void OnTriggerEnter(Collider other)
    {

    }
}
