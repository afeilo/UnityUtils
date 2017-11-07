using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System;

public class SenesitiveWord{
	
	private static string GetPattern(){
		return RegExpLib.regex;
	}

	private static Regex GetRegex(string pattern){
		Regex regex = null;
		if (pattern != null) {
			//regex = new Regex (pattern,RegexOptions.Multiline|RegexOptions.IgnoreCase|RegexOptions.IgnorePatternWhitespace);
		}
		return regex;
	}
	//正则表达式方法 不科学
	public static string ConvertToFiterWords(string raw){
		Regex regex = GetRegex (GetPattern());
		StringBuilder sb = new StringBuilder ();
		int startIndex = 0;
		foreach(Match match in regex.Matches (raw)){
			//Debug.Log(match.Index);
			sb.Append (raw.Substring (startIndex, match.Index-startIndex));
			//Debug.Log("length:"+match.Length);
			for (int i = 0, len = match.Length; i < len; i++) {
				sb.Append ("*");
			}
			startIndex = (match.Index+match.Length);
		}
		sb.Append (raw.Substring (startIndex, raw.Length-startIndex));
		//raw = regex.Replace (raw, "***");
		return sb.ToString();
	}

	//转换树结构
	public static Hashtable addSensitiveWordToHashMap(string[] words){
		Hashtable sensitiveWord = new Hashtable ();
		for (int i = 0, len = words.Length; i < len; i++) {
			string word = words [i];
			Hashtable now = sensitiveWord;//当前hashTable
			for (int j = 0; j < word.Length; j++) {
				var table = now [word [j]];
				if (table != null) {
					now = table as Hashtable;
				}else{
					Hashtable newtable = new Hashtable ();
					newtable.Add ("e","0");
					now.Add (word [j], newtable);
					now = newtable;
				}
				if (j == word.Length - 1) {
					now.Remove("e");
					now.Add ("e","1");
				}
			}
		}
		return sensitiveWord;
	}
	//根据树结构替换敏感词
	public static string fiterWords(string word,Hashtable hashtable,RegexOptions options){
		bool ignoreCase = (options & RegexOptions.IgnoreCase)==RegexOptions.IgnoreCase;
		bool ignorePatternWhiteSpace = (options & RegexOptions.IgnorePatternWhitespace)==RegexOptions.IgnorePatternWhitespace;
		StringBuilder sb = new StringBuilder ();
		Hashtable now = null;
		string fiterWord = word;
		if (ignoreCase) {
			fiterWord = word.ToLower ();
		}
		for (int i = 0, len = fiterWord.Length; i < len; i++) {

			int start = i,end = i;
			var table = hashtable [fiterWord [i]];
			while(table != null) {
				now = table as Hashtable;
				if (now ["e"] == "1") {
					end = i;
				}
				i++;
				while ( ignorePatternWhiteSpace && i<len && Char.IsWhiteSpace(fiterWord[i])) 
					i++;	
				if (i >= len)
					break;
				table = now [fiterWord [i]];
			}
			if (start == end) {
				i = start;
				sb.Append (word[i]);
			}
			else {
				for (; start <= end; start++) {
					sb.Append ("*");
				}
				i = end;
			}
		}
		return sb.ToString ();
	}
	public enum RegexOptions
	{
		None = 0,
		IgnoreCase = 1,
		IgnorePatternWhitespace = 2,
	}
}
