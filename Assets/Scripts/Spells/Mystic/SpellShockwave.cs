using UnityEngine;
using System.Collections;

public class SpellShockwave : Spell
{
    public override void Start()
    {
        base.Start();
        if (character != null)
        {
            PlayAudio(false);
            hand.EnableCollider();
            character.middleCollision += SpellCollision;
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
                NetworkCombatHandler.Instance.PushbackCharacter(this, c, basePower, this.transform.forward);
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
