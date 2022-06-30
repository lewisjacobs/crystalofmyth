using UnityEngine;
using System.Collections;

public class Play_Audio : MonoBehaviour {

	public AudioClip[] aud;





	public void PlayArchress()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[0]);
	}

	public void PlayKnight()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[1]);
	}

	public void PlayBarbarian()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[2]);
	}

	public void PlayMystic()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[3]);
	}

	public void PlayMusic()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[4]);
	}

	public void PlayOpeningMusic()
	{
		GetComponent<AudioSource>().PlayOneShot(aud[5]);

	}



}
