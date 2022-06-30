using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkCombatHandler : Singleton<NetworkCombatHandler>
{
    public void DamageCharacter(Spell spell, Character character, int iDamage, string sourcePlayerID)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.TakeDamage(iDamage, sourcePlayerID);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<int>(iDamage);
                data.Add<string>(sourcePlayerID);

                networkSync.SendMessageToCharacterAsSender("TakeDamage", data);
            }
        }
    }

    public void HealCharacter(Spell spell, Character character, int iHealth)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.Heal(iHealth);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if ( nvSpell != null && networkSync != null && nvSpell.isMine )
            {
                MessageData data = new MessageData();
                data.Add<int>( iHealth );

                networkSync.SendMessageToCharacterAsSender( "Heal", data );
            }
        }
    }

    public void HealManaCharacter(Spell spell, Character character, int iMana)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.HealMana(iMana);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if ( nvSpell != null && networkSync != null && nvSpell.isMine )
            {
                MessageData data = new MessageData();
                data.Add<int>( iMana );

                networkSync.SendMessageToCharacterAsSender( "HealMana", data );
            }
        }
    }

    public void CrippleCharacter(Spell spell, Character character, StatusCripple status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.Cripple(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if ( nvSpell != null && networkSync != null && nvSpell.isMine )
            {
                MessageData data = new MessageData();
                data.Add<StatusCripple>( status );

                networkSync.SendMessageToCharacterAsSender( "Cripple", data );
           }
        }
    }

    public void FreezeCharacter(Spell spell, Character character, StatusFreeze status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.Freeze(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<StatusFreeze>(status);

                networkSync.SendMessageToCharacterAsSender("Freeze", data);
            }
        }
    }

    public void BuffCharacterAttack(Spell spell, Character character, StatusAttack status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.BuffAttack(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<StatusAttack>(status);

                networkSync.SendMessageToCharacterAsSender("BuffAttack", data);
            }
        }
    }
    public void BuffCharacterDefence(Spell spell, Character character, StatusDefence status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.BuffDefence(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<StatusDefence>(status);

                networkSync.SendMessageToCharacterAsSender("BuffDefence", data);
            }
        }
    }

    public void ChargeCharacter(Spell spell, Character character, StatusCharge status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.Charge(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<StatusCharge>(status);

                networkSync.SendMessageToCharacterAsSender("Charge", data);
            }
        }
    }

    public void HasteCharacter(Spell spell, Character character, StatusHaste status)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.Haste(status);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<StatusHaste>(status);

                networkSync.SendMessageToCharacterAsSender("Haste", data);
            }
        }
    }

    public void PushbackCharacter(Spell spell, Character character, int iPush, Vector3 direction)
    {
        if (!StaticProperties.Instance.MultiPlayer)
        {
            character.PushBack(iPush, direction);
        }
        else
        {
            NetworkView nvSpell = spell.GetComponent<NetworkView>();
            NetworkSync networkSync = character.gameObject.GetComponent<NetworkSync>();

            if (nvSpell != null && networkSync != null && nvSpell.isMine)
            {
                MessageData data = new MessageData();
                data.Add<int>(iPush);
                data.Add(direction);

                networkSync.SendMessageToCharacterAsSender("PushBack", data);
            }
        }
    }

}