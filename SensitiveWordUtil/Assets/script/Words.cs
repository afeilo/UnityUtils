﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class Words : MonoBehaviour {
	public Text text;
	void Start () {
		DateTime start = System.DateTime.Now;
		Hashtable hashtable = SenesitiveWord.addSensitiveWordToHashMap (SenestiveLib.regex);
		DateTime end = System.DateTime.Now;
		Debug.Log ("耗时1:"+(end-start).TotalMilliseconds);
		string s = SenesitiveWord.fiterWords("中国中国猪Fuck你就是一个傻 逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻 逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻 逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻 逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊傻 ",hashtable,
			SenesitiveWord.RegexOptions.IgnoreCase|SenesitiveWord.RegexOptions.IgnorePatternWhitespace);
		text.text = s;
		Debug.Log ("耗时2:"+(System.DateTime.Now-end).TotalMilliseconds);
		end = System.DateTime.Now;
		SenesitiveWord.fiterWords("你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊" +
			"你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊你就是一个傻逼傻吊啊"
			,hashtable,SenesitiveWord.RegexOptions.IgnorePatternWhitespace);
		Debug.Log ("耗时3:"+(System.DateTime.Now-end).TotalMilliseconds);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
