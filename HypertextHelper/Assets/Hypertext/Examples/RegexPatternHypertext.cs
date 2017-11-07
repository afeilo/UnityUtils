using UnityEngine;
using System.Collections;
using HypertextHelper;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class RegexPatternHypertext : Hypertext {
	const string RegexURL = "http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?";
	//public string[] RegexUrls;
	public Color color;
	public Action<string> OnClick;
	private Text text;
	void Start(){
		text = gameObject.GetComponent<Text> ();

	}
	protected override void RegisterClickable (){
	//	if (RegexUrls == null) {
	//		return;
	//	}
	//	foreach (string regex in RegexUrls) {
	//		foreach (Match match in Regex.Matches(text.text, regex))
	//		{
	//			RegisterClickable(match.Value,match.Index,color,url => Debug.Log(url));
	//		}
	//	}
		foreach (Match match in Regex.Matches(text.text, RegexURL))
		{
			RegisterClickable(match.Value,match.Index,color,url => Debug.Log(url));
		}
	}
}
