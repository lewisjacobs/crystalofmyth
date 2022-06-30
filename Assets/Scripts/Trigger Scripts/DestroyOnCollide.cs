using UnityEngine;
using System.Collections;

public class DestroyOnCollide : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            GameController.Instance.DestroyObject(other.gameObject);
        }
    }
}
