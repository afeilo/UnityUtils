using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Text;

public class EmojiText : Text {
	//private static Dictionary<string,EmojiTextInfo> EmojiIndex = null;

	readonly UIVertex[] m_TempVerts = new UIVertex[4];
	private const char REGEX_PRE = '≙';
	private const char REGEX_SUF = '≙';
	public override string text
	{
		get
		{
			return base.text;
		}
		set
		{
			base.text = EmojiDecoder.instance.EncodeEmoji (value);
		}
	}
	public struct EmojiIndexInfo
	{
		public int index;
		public int len;
		public EmojiInfo emojiInfo;
	}
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (font == null)
			return;

		Dictionary<int,EmojiInfo> emojiDic = null;
		//if (supportRichText) {
		emojiDic = new Dictionary<int,EmojiInfo> ();
		string emojitext = text;
		MatchCollection matches = Regex.Matches (text, "\\≙[a-z0-9A-Z]+\\≙");
		int len = 0;
        Dictionary<string, EmojiInfo> emojiInfos = EmojiFileManager.getInstance ().emojiInfos;
		for (int i = 0; i < matches.Count; i++) {
			EmojiInfo emojiInfo;
			string match = matches [i].Value;
			if (emojiInfos.TryGetValue (match.Substring(1,match.Length-2), out emojiInfo)) {
				int index = matches [i].Index - len;
				len += matches [i].Length - 1;
				Debug.Log (index);
				emojiDic.Add (index, emojiInfo);
				emojitext = emojitext.Replace (match,"M");
			}
		}
		//}
		Debug.Log (emojitext);
		// We don't care if we the font Texture changes while we are doing our Update.
		// The end result of cachedTextGenerator will be valid for this instance.
		// Otherwise we can get issues like Case 619238.
		m_DisableFontTextureRebuiltCallback = true;
		Vector2 extents = rectTransform.rect.size;
		var settings = GetGenerationSettings(extents);
		cachedTextGenerator.Populate(emojitext, settings);

		// Apply the offset to the vertices
		IList<UIVertex> verts = cachedTextGenerator.verts;
		float unitsPerPixel = 1 / pixelsPerUnit;
		//Last 4 verts are always a new line... (\n)
		int vertCount = verts.Count - 4;

		Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
		roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
		toFill.Clear();
		if (roundingOffset != Vector2.zero)
		{
			for (int i = 0; i < vertCount; ++i)
			{
				int tempVertsIndex = i & 3;
				m_TempVerts[tempVertsIndex] = verts[i];
				m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
				m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
				m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
				if (tempVertsIndex == 3)
					toFill.AddUIVertexQuad(m_TempVerts);
			}
		}
		else
		{
			for (int i = 0; i < vertCount; ++i)
			{			
				EmojiInfo emojiInfo;
				int index = i / 4;
				if (emojiDic.TryGetValue (index, out emojiInfo)) {
					m_TempVerts [3] = verts [i];//1
					m_TempVerts [2] = verts [i + 1];//2
					m_TempVerts [1] = verts [i + 2];//3
					m_TempVerts [0] = verts [i + 3];//4

					//方形
					float fixValue = (m_TempVerts [2].position.x - m_TempVerts [3].position.x - (m_TempVerts [2].position.y - m_TempVerts [1].position.y))/2;
					if (fixValue > 0) {
						m_TempVerts [2].position -= new Vector3 (fixValue, 0, 0);
						m_TempVerts [1].position -= new Vector3 (fixValue, 0, 0);
						m_TempVerts [0].position += new Vector3 (fixValue, 0, 0);
						m_TempVerts [3].position += new Vector3 (fixValue, 0, 0);
					} else {
						m_TempVerts [2].position += new Vector3 (0, fixValue, 0)*2;
						m_TempVerts [3].position += new Vector3 (0, fixValue, 0)*2;
						//m_TempVerts [0].position -= new Vector3 (0, fixValue, 0);
						//m_TempVerts [1].position -= new Vector3 (0, fixValue, 0);
					}


					m_TempVerts [0].position *= unitsPerPixel;
					m_TempVerts [1].position *= unitsPerPixel;
					m_TempVerts [2].position *= unitsPerPixel;
					m_TempVerts [3].position *= unitsPerPixel;

                    m_TempVerts[0].uv1 = new Vector2(emojiInfo.x, emojiInfo.y);
                    m_TempVerts[1].uv1 = new Vector2(emojiInfo.x + emojiInfo.size, emojiInfo.y);
                    m_TempVerts[2].uv1 = new Vector2(emojiInfo.x + emojiInfo.size, emojiInfo.y + emojiInfo.size);
                    m_TempVerts[3].uv1 = new Vector2(emojiInfo.x, emojiInfo.y + emojiInfo.size);
					toFill.AddUIVertexQuad (m_TempVerts);
					i += 3;
				} else {
					int tempVertsIndex = i & 3;
					m_TempVerts [tempVertsIndex] = verts [i];
					m_TempVerts [tempVertsIndex].position *= unitsPerPixel;
					if (tempVertsIndex == 3)
						toFill.AddUIVertexQuad (m_TempVerts);
				}
			}
		}

		m_DisableFontTextureRebuiltCallback = false;
	}
	public class EmojiDecoder
	{

		public static EmojiDecoder instance = new EmojiDecoder ();
		public string EncodeEmoji(string org)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < org.Length; i++)
			{
				int rst = IS_EMOJI(org, i);
				if (rst != 0)
				{
					sb.Append(REGEX_PRE+EncodeName(org,i,rst)+REGEX_SUF);
					i += rst - 1;
				}
				else
				{
					sb.Append(org[i]);
				}
			}

			return sb.ToString();
		}

		static bool IS_IN(int val, int min, int max)
		{
			return ((val) >= (min)) && ((val) <= (max));
		}

		static int IS_EMOJI(string s, int index)
		{
			char hs = s[index];
			char ls = s.Length > index + 1 ? s[index + 1] : (char)0;


			if (IS_IN(hs, 0xD800, 0xDBFF))
			{
				int uc = ((hs - 0xD800) * 0x400) + (ls - 0xDC00) + 0x10000;

				// Musical: [U+1D000, U+1D24F]
				// Enclosed Alphanumeric Supplement: [U+1F100, U+1F1FF]
				// Enclosed Ideographic Supplement: [U+1F200, U+1F2FF]
				// Miscellaneous Symbols and Pictographs: [U+1F300, U+1F5FF]
				// Supplemental Symbols and Pictographs: [U+1F900, U+1F9FF]
				// Emoticons: [U+1F600, U+1F64F]
				// Transport and Map Symbols: [U+1F680, U+1F6FF]
				if (IS_IN(uc, 0x1D000, 0x1F9FF))
					return 2;
			}
			else if (ls == 0x20E3)
			{
				// emojis for numbers: number + modifier ls = U+20E3
				return 2;
			}
			else
			{
				if (		// Latin-1 Supplement
					hs == 0x00A9 || hs == 0x00AE
					// General Punctuation
					|| hs == 0x203C || hs == 0x2049
					// Letterlike Symbols
					|| hs == 0x2122 || hs == 0x2139
					// Arrows
					|| IS_IN(hs, 0x2194, 0x2199) || IS_IN(hs, 0x21A9, 0x21AA)
					// Miscellaneous Technical
					|| IS_IN(hs, 0x231A, 0x231B) || IS_IN(hs, 0x23E9, 0x23F3) || IS_IN(hs, 0x23F8, 0x23FA) || hs == 0x2328 || hs == 0x23CF
					// Geometric Shapes
					|| IS_IN(hs, 0x25AA, 0x25AB) || IS_IN(hs, 0x25FB, 0x25FE) || hs == 0x25B6 || hs == 0x25C0
					// Miscellaneous Symbols
					|| IS_IN(hs, 0x2600, 0x2604) || IS_IN(hs, 0x2614, 0x2615) || IS_IN(hs, 0x2622, 0x2623) || IS_IN(hs, 0x262E, 0x262F)
					|| IS_IN(hs, 0x2638, 0x263A) || IS_IN(hs, 0x2648, 0x2653) || IS_IN(hs, 0x2665, 0x2666) || IS_IN(hs, 0x2692, 0x2694)
					|| IS_IN(hs, 0x2696, 0x2697) || IS_IN(hs, 0x269B, 0x269C) || IS_IN(hs, 0x26A0, 0x26A1) || IS_IN(hs, 0x26AA, 0x26AB)
					|| IS_IN(hs, 0x26B0, 0x26B1) || IS_IN(hs, 0x26BD, 0x26BE) || IS_IN(hs, 0x26C4, 0x26C5) || IS_IN(hs, 0x26CE, 0x26CF)
					|| IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26D3, 0x26D4) || IS_IN(hs, 0x26E9, 0x26EA) || IS_IN(hs, 0x26F0, 0x26F5)
					|| IS_IN(hs, 0x26F7, 0x26FA)
					|| hs == 0x260E || hs == 0x2611 || hs == 0x2618 || hs == 0x261D || hs == 0x2620 || hs == 0x2626 || hs == 0x262A
					|| hs == 0x2660 || hs == 0x2663 || hs == 0x2668 || hs == 0x267B || hs == 0x267F || hs == 0x2699 || hs == 0x26C8
					|| hs == 0x26D1 || hs == 0x26FD
					// Dingbats
					|| IS_IN(hs, 0x2708, 0x270D) || IS_IN(hs, 0x2733, 0x2734) || IS_IN(hs, 0x2753, 0x2755)
					|| IS_IN(hs, 0x2763, 0x2764) || IS_IN(hs, 0x2795, 0x2797)
					|| hs == 0x2702 || hs == 0x2705 || hs == 0x270F || hs == 0x2712 || hs == 0x2714 || hs == 0x2716 || hs == 0x271D
					|| hs == 0x2721 || hs == 0x2728 || hs == 0x2744 || hs == 0x2747 || hs == 0x274C || hs == 0x274E || hs == 0x2757
					|| hs == 0x27A1 || hs == 0x27B0 || hs == 0x27BF
					// CJK Symbols and Punctuation
					|| hs == 0x3030 || hs == 0x303D
					// Enclosed CJK Letters and Months
					|| hs == 0x3297 || hs == 0x3299
					// Supplemental Arrows-B
					|| IS_IN(hs, 0x2934, 0x2935)
					// Miscellaneous Symbols and Arrows
					|| IS_IN(hs, 0x2B05, 0x2B07) || IS_IN(hs, 0x2B1B, 0x2B1C) || hs == 0x2B50 || hs == 0x2B55
				)
				{
					return 1;
				}
			}
			return 0;
		}


		string EncodeName(string s, int startIndex, int length)
		{
			string r = "";
			for (int i = 0; i < length; i++)
			{
				char c = s[i + startIndex];
				byte[] bytes = BitConverter.GetBytes(c);

				for (int j = 1; j >= 0; j--)
				{
					string t = bytes[j].ToString("x");
					if (t.Length < 2)
						t = "0" + t;
					r += t;
				}
			}
			return r;
		}
	}
}
