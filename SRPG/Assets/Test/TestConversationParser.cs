using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;


public static class TestConversationParser 
{
	[MenuItem("Pre Production/Parse Convos")]
	public static void Parse()
	{
		CreateDirectories ();
		ParseConvs ();

	//	AssetDatabase.SaveAssets();
	//	AssetDatabase.Refresh();
	}

	static void CreateDirectories ()
	{
		if (!AssetDatabase.IsValidFolder("Assets/Resources/Conversations"))
			AssetDatabase.CreateFolder("Assets/Resources", "Conversations");
	}

	static void ParseConvs ()
	{
		ConversationData ThisConversation = ScriptableObject.CreateInstance<ConversationData> ();// new ConversationData () ;
		int conversationCounter = 0;
		string readPath = string.Format("{0}/Settings/testConv.txt", Application.dataPath);
		string[] readText = File.ReadAllLines(readPath);
		Sprite lastSprite = null;
		TextAnchor lastAnchor = TextAnchor.MiddleCenter;
		for (int i = 1; i < readText.Length; ++i) { //skip first line
			string[] elements = readText[i].Split('/');	
			//PartsNiceConvs (readText [i]);
			//Debug.Log ("ASDF"+elements[2]);

			Sprite curSprite = GetSprite(elements[0]);
			TextAnchor curAnchor = GetAnchor (elements [1]);
			string thisMessage = elements [2];
			Debug.Log (thisMessage);
			if (curAnchor == lastAnchor && curSprite == lastSprite) {
				ThisConversation.list [conversationCounter-1].messages.Add (thisMessage);
			} else {
				SpeakerData spkrData = new SpeakerData();
				spkrData.speaker=curSprite;
				spkrData.anchor=curAnchor;
				Debug.Log ("Message to send: "+thisMessage);
				Debug.Log ("Speaker Data: "+spkrData);
				Debug.Log ("string list: " +spkrData.messages);
				//spkrData.messages=new List <string>();
				spkrData.messages.Add(thisMessage);



				Debug.Log (ThisConversation);
						ThisConversation.list.Add(spkrData);
				conversationCounter++;
			}
			lastAnchor=curAnchor;
			lastSprite = curSprite;
		}

		string filePath =  "Assets/Resources/Conversations/";
		string fileName = string.Format("{0}{1}.asset", filePath, "zzzz");
		AssetDatabase.CreateAsset(ThisConversation, fileName);

	}

	static void PartsNiceConvs (string line)
	{
		string[] elements = line.Split('/');
		Debug.Log (elements [0] + " " + elements [1]+ " " + elements [2]);



	}



	static StatModifierFeature GetFeature (GameObject obj, StatTypes type)
	{
		StatModifierFeature[] smf = obj.GetComponents<StatModifierFeature>();
		for (int i = 0; i < smf.Length; ++i)
		{
			if (smf[i].type == type)
				return smf[i];
		}

		StatModifierFeature feature = obj.AddComponent<StatModifierFeature>();
		feature.type = type;
		return feature;
	}

	static Sprite GetSprite (string spriteName)
	{
		string fullPath = string.Format("Assets/Textures/UI/ConvSprites/{0}.png", spriteName,typeof(Sprite));
		Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
		if (spr == null)
			Debug.LogError ("You wrote a wrong sprite name: " + spriteName);
		else
			Debug.Log ("DONE RIGHT: " + spr.name);
		return spr;
	}

	static TextAnchor GetAnchor (string anchorName){
		
		TextAnchor theAnchor = TextAnchor.MiddleCenter;
		if (anchorName=="UR")
			theAnchor=  TextAnchor.UpperRight;	
		else if (anchorName=="LR")
			theAnchor=  TextAnchor.LowerRight;	
		else if (anchorName=="UL")
			theAnchor=  TextAnchor.UpperLeft;	
		else if (anchorName=="LL")
		theAnchor=  TextAnchor.LowerLeft;
			return theAnchor;
	}
	/*
	static GameObject Create (string fullPath)
	{
		ConversationData instance = ScriptableObject.CreateInstance<ConversationData> ();
		instance.AddComponent<Job>();
		GameObject prefab = PrefabUtility.CreatePrefab( fullPath, instance );
		GameObject.DestroyImmediate(instance);
		return prefab;
	}*/
}