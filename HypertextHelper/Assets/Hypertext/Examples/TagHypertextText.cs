using UnityEngine;
using System.Collections;
using HypertextHelper;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Tag hypertext text.
/// 字符串匹配规则
/// </summary>
public class TagHypertextText : HypertextText{
	private static readonly Regex s_HrefRegex =
		new Regex(@"<url=([^>\n\s]+)>(.*?)(</url>)", RegexOptions.Singleline);
	public List<ClickableTextEntry> clickableEntries = new List<ClickableTextEntry>();
	void Start(){
		//		text = gameObject.GetComponent<Text> ();
	}
	protected override void OnPopulateMesh (VertexHelper vh){
		var origin = m_Text;
		clickableEntries.Clear ();
		MatchCollection hrefRegex = s_HrefRegex.Matches (m_Text);
		StringBuilder s_TextBuilder = new StringBuilder ();
		if (hrefRegex.Count > 0) {
			var indexText = 0;
			foreach (Match match in hrefRegex) {
				s_TextBuilder.Append (m_Text.Substring (indexText, match.Index - indexText));
				s_TextBuilder.Append (match.Groups [2].Value);
				clickableEntries.Add (new ClickableTextEntry (match.Groups [2].Value,match.Groups [1].Value,match.Index,(url) => Debug.Log(url)));
				indexText = match.Index + match.Length;
			}
			s_TextBuilder.Append (m_Text.Substring (indexText));
			m_Text = s_TextBuilder.ToString();
		}

		base.OnPopulateMesh (vh);
		m_Text = origin;
	}
	protected override void RegisterClickable (){

		foreach(ClickableTextEntry clickableEntry in clickableEntries){
			RegisterClickable(clickableEntry);
		}
	}
}
