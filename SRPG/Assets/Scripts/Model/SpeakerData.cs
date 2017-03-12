using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpeakerData 
{
	public List<string> messages = new List<string> ();
	public Sprite speaker;
	public TextAnchor anchor;
}
