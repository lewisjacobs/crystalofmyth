using UnityEngine;
using System.Collections;

public class SpiralProjectile : MonoBehaviour 
{
    public float SpiralArc = 0.2f;
    public float DownwardsSpeed = 3;
    public float TimeScaler = 10.0f;

    protected float t=0.0f, x = 0.0f, y = 0.0f, z = 0.0f;

	void Start () 
    {
	
	}
	
	void Update() 
    {
        t +=  TimeScaler * Time.deltaTime;
        x = SpiralArc * t * Mathf.Cos( t );
        z = SpiralArc * t * Mathf.Sin( t );
        y = -9.81f * t * t - DownwardsSpeed * t;

        y = ( float ) y / 5000;

        Vector3 V = new Vector3( x, y, z );

        this.transform.position = this.transform.position + V;
	}
}
