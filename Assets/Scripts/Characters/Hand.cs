using UnityEngine;
using System.Collections;

public enum HandSide
{
    RIGHT,
    LEFT,
    MIDDLE
}

public class Hand : MonoBehaviour
{
    public Character character;
    public HandSide handSide;

    void Start()
    {
        DisableCollider();
    }

    public void OnTriggerEnter(Collider c)
    {

        if (handSide == HandSide.LEFT && c.gameObject.activeSelf)
        {
            character.FireLeftCollision(c);
        }
        else if (handSide == HandSide.RIGHT && c.gameObject.activeSelf)
        {
            character.FireRightCollision(c);
        }
        else if (handSide == HandSide.MIDDLE && c.gameObject.activeSelf)
        {
            character.FireMiddleCollision(c);
        }
    }

    public void EnableCollider()
    {
        GetComponent<Collider>().enabled = true;
    }
    public void DisableCollider()
    {
        GetComponent<Collider>().enabled = false;
    }
}
