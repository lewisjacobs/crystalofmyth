using UnityEngine;
using System.Collections;

public class LavaCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            Character c = (other.gameObject).GetComponent<Character>();

            if (c != null)
            {
                c.TakeDamage(1000, "-1");
            }
        }
    }
}
