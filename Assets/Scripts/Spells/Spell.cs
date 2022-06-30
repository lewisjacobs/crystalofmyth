using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Spell : MonoBehaviour
{
    public bool Exploding = false;
    public bool RemoteFix = false;

    public List<Collider> hitColliders = new List<Collider>();
    public int team;
    public float cooldown;
    public string spellName;
    public int basePower;
    public int manaCost;
    public AudioClip castSound;
    public float lifetime;

    protected int currentDamage;
    protected Character character;
    protected Hand hand;
    protected int handIndex;
    protected NetworkView view;
    protected string sourcePlayerID;
    protected bool _destroyed = false;

    public virtual void Start()
    {
        currentDamage = basePower;
        view = this.GetComponent<NetworkView>();
    }

    public void PlayAudio(bool local)
    {
        if (castSound != null)
        {
            if (local)
                GetComponent<AudioSource>().PlayOneShot(castSound);
            else character.PlaySound(castSound, this.GetType());
        }
    }

    public virtual void Update()
    {
        lifetime -= Time.deltaTime;

        if (lifetime <= 0)
        {
            // lewis
            //character.StopAudio();
            GameController.Instance.DestroyObject(this.gameObject);
        }
    }

    public void SetTeam(int colour)
    {
        team = colour;
    }

    public void SetForward(Vector3 forward)
    {
        this.transform.forward = forward;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (CheckIfSpellShouldCollide(other))
        {
            GameController.Instance.DestroyObject( this.gameObject );
        }
    }

    //Returns true if the spell should make contact with "other"
    public bool CheckIfSpellShouldCollide( Collider other )
    {
        if (!other.isTrigger)
        {
            Character c = (other.gameObject).GetComponent<Character>();

            if (c != null)
            {
                if ((team == TeamColours.NOTEAM || team != c.Team) && c != this.character)
                {
                    return true;
                }
            }
            else
            {
                if ((other.gameObject).GetComponent<Spell>() == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetCharacter(Character c, string id)
    {
        SetID( id );
        character = c;
    }

    public void SetID(string id)
    {
        sourcePlayerID = id;
    }

    public virtual void SetHand( Hand h, int handIndex )
    {
        this.handIndex = handIndex;
        hand = h;
    }
    
    public void SetDamageMultiplier(float amount)
    {
        currentDamage = (int)(basePower * amount);
    }
}
