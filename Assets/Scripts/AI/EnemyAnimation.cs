using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour
{
	public float deadZone = 5f;             
	
	private Transform player;               
	private AI_Sight enemySight;          
	private NavMeshAgent nav;              
	private Animator anim;           

	
	void Awake ()
	{
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        //enemySight = GetComponent<AI_Sight>();
        //nav = GetComponent<NavMeshAgent>();
        //anim = GetComponent<Animator>();
		
        //nav.updateRotation = false;
        //deadZone *= Mathf.Deg2Rad;
	}
	

	
	void OnAnimatorMove ()
	{
        //nav.velocity = anim.deltaPosition / Time.deltaTime;
		
        //transform.rotation = anim.rootRotation;
	}
	
	
	
	



}