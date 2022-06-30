using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
    public float Speed;

    protected bool _bAccelerated = false;

	void Update () 
    {
        if ( !_bAccelerated )
        {
            _bAccelerated = true;
            this.GetComponent<Rigidbody>().velocity = this.transform.forward * Speed;
        }
	}
}
