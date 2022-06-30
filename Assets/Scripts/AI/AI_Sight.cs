using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Sight : MonoBehaviour
{
    protected List<Character> _CharacterList = new List<Character>();

    public List<Character> GetCharactersInRange()
    {
        return _CharacterList;
    }

	public void OnTriggerEnter(Collider other)
	{
        Character character = other.gameObject.GetComponent<Character>();

        if( character != null )
        {
            _CharacterList.Add(character);
        }
	}

    public void OnTriggerExit( Collider other )
    {
        Character character = other.gameObject.GetComponent<Character>();

        if (character != null)
        {
            _CharacterList.Remove(character);
        }
    }
}



	