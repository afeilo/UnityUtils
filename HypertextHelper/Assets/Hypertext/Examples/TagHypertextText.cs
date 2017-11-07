using UnityEngine;
using System.Collections;
using HypertextHelper;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Text;

public class TagHypertextText : HypertextText{
	const string RegexURL = "http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?";
	const string RegexURL2 = @"\(\d*[.]\d\,\d*[.]\d\)";
	private static readonly Regex s_HrefRegex =
		new Regex(@"<url=([^>\n\s]+)>(.*?)(</url>)", RegexOptions.Singleline);
	//const string RegexURL2 = "\\(\\,\\)";
	//public string[] RegexUrls;
	//public Color color;
	//public Action<string> OnClick;
	//private Text text;
	public List<ClickableTextEntry> clickableEntries = new List<ClickableTextEntry>();
	void Start(){
		//		text = gameObject.GetComponent<Text> ();
	}
	protected override void OnPopulateMesh (VertexHelper vh){
		var origin = m_Text;
		MatchCollection hrefRegex = s_HrefRegex.Matches (m_Text);
		StringBuilder s_TextBuilder = new StringBuilder ();
		Debug.Log ("hrefRegex.Count ="+hrefRegex.Count);
		if (hrefRegex.Count > 0) {
			var indexText = 0;
			foreach (Match match in hrefRegex) {
				s_TextBuilder.Append (m_Text.Substring (indexText, match.Index - indexText));
				s_TextBuilder.Append (match.Groups [2].Value);
				clickableEntries.Add (new ClickableTextEntry (match.Groups [2].Value,match.Groups [1].Value,match.Index,color,(url) => Debug.Log(url)));
				indexText = match.Index + match.Length;
			}
			m_Text = s_TextBuilder.ToString();
		}
		base.OnPopulateMesh (vh);
		m_Text = origin;
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

		//foreach (Match match in Regex.Matches(text.text, RegexURL))
		//{
		//	RegisterClickable(match.Value,match.Index,color,(url) => Debug.Log(url));
		//}
		//foreach (Match match in Regex.Matches(text.text, RegexURL2))
		//{
		//	RegisterClickable(match.Value,match.Index,color,(url) => Debug.Log(url));
		//}
		foreach(ClickableTextEntry clickableEntry in clickableEntries){
			RegisterClickable(clickableEntry);
		}
	}
}
