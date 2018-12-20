using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffect
{
	Confirm,
	Cancel,
	HitOrDestroy,
	Walk
}

public class SoundEffectMaker : MonoBehaviour {

	List<AudioSource> sounds;

	void Start () 
	{
		sounds = new List<AudioSource>();

		var soundsObj = GetComponentsInChildren<AudioSource>();
		int kind = soundsObj.Length;
		for(int i=1;i<kind;i++)
		{
			sounds.Add(soundsObj[i]);
		}
	}
	
	public void play(SoundEffect kind)
	{
		var sound = sounds[(int)kind];

		//if(sound.isPlaying) return;
		sound.PlayOneShot(sound.clip);
	}
}
