﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log ("test");
		EmojiFile emojiFile = EmojiFileMgr.getInstance ().emojiFile;
		Debug.Log(emojiFile.count);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
