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
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (font == null)
			return;
		string emojitext = text;
		Dictionary<int,EmojiDecoder.EmojiIndexInfo> emojiDic = null;
		int len = 0;
		if (supportRichText) {
			emojiDic = new Dictionary<int, EmojiDecoder.EmojiIndexInfo> ();
			StringBuilder sb = new StringBuilder ();
			List<EmojiDecoder.EmojiIndexInfo> emojiMsgs = EmojiDecoder.instance.EncodeEmoji (emojitext);
			Debug.Log (emojiMsgs.Count);
			int lastIndex = 0;
			for(int i = 0,count = emojiMsgs.Count; i < count; i++){
				EmojiDecoder.EmojiIndexInfo emojiIndexInfo = emojiMsgs [i];
				Debug.Log ("index = " + emojiIndexInfo.index);
				sb.Append (emojitext.Substring(lastIndex,emojiIndexInfo.index-lastIndex)).Append('M');
				emojiDic.Add (emojiIndexInfo.index - len, emojiIndexInfo);
				len += emojiIndexInfo.len - 1;
				lastIndex = emojiIndexInfo.index + emojiIndexInfo.len;
				//Debug.Log ("index = "+emojiIndexInfo.index);
			}
			sb.Append (emojitext.Substring(lastIndex));
			emojitext = sb.ToString();
		}

		foreach (var item in emojiDic)
		{
			Debug.Log (item.Key);
		}

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
				EmojiDecoder.EmojiIndexInfo info;
				int index = i / 4;
				if (emojiDic.TryGetValue (index, out info)) {
					EmojiInfo emojiInfo = info.emojiInfo;
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

					//去除间隙
					float pixelOffset = 1/2048;
					float emojiSize = (float)emojiInfo.size / EmojiFileManager.getInstance ().size;
					Debug.Log (emojiSize);
					float uv_x = (float)emojiInfo.x / EmojiFileManager.getInstance ().size;
					Debug.Log (uv_x+pixelOffset);
					float uv_y = (float)emojiInfo.y / EmojiFileManager.getInstance ().size;
					m_TempVerts [0].uv1 = new Vector2 (uv_x + pixelOffset, uv_y + pixelOffset);
					m_TempVerts [1].uv1 = new Vector2 (uv_x - pixelOffset + emojiSize, uv_y + pixelOffset);
					m_TempVerts [2].uv1 = new Vector2 (uv_x- pixelOffset +emojiSize, uv_y - pixelOffset + emojiSize);
					m_TempVerts [3].uv1 = new Vector2 (uv_x+ pixelOffset, uv_y - pixelOffset + emojiSize);
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
		public struct EmojiIndexInfo
		{
			public int index;
			public int len;
			public EmojiInfo emojiInfo;
		}
		public static EmojiDecoder instance = new EmojiDecoder ();
		public List<EmojiIndexInfo> EncodeEmoji(string org)
		{
			List<EmojiIndexInfo> emojiMsgs = new List<EmojiIndexInfo>();
			for (int i = 0; i < org.Length; i++)
			{
				for (int j = 0, len = EmojiFileManager.getInstance ().count; j < len; j++) {
					char[] regex = EmojiFileManager.getInstance ().emojiInfos [j].regex;
					if (regex.Length == 1) {
						if (FindCode1(org, i, regex))
						{
							EmojiIndexInfo emojiIndex = new EmojiIndexInfo ();
							emojiIndex.emojiInfo = EmojiFileManager.getInstance ().emojiInfos [j];
							emojiIndex.index = i;
							emojiIndex.len = 1;
							emojiMsgs.Add (emojiIndex);
							Debug.Log (emojiIndex.emojiInfo.key);
						}
					} else if (regex.Length == 2) {
						if (FindCode2(org, i,regex))
						{
							EmojiIndexInfo emojiIndex = new EmojiIndexInfo ();
							emojiIndex.emojiInfo = EmojiFileManager.getInstance ().emojiInfos [j];
							emojiIndex.index = i;
							emojiIndex.len = 2;
							emojiMsgs.Add (emojiIndex);
							Debug.Log (emojiIndex.emojiInfo.key);
						}
					} else if (regex.Length == 4) {
						if (FindCode4(org, i,regex))
						{
							EmojiIndexInfo emojiIndex = new EmojiIndexInfo ();
							emojiIndex.emojiInfo = EmojiFileManager.getInstance ().emojiInfos [j];
							emojiIndex.index = i;
							emojiIndex.len = 4;
							emojiMsgs.Add (emojiIndex);
							Debug.Log (emojiIndex.emojiInfo.key);
						}
					}
				}
			}

			return emojiMsgs;
		}

		bool FindCode1(string org, int index,char[] regex)
		{
			char v = org[index];
			int rv = regex[0];
			return rv == v;
		}

		bool FindCode2(string org, int index,char[] regex)
		{
			if (index >= org.Length - 1)
				return false;

			if (org[index] == regex[0] && org[index + 1] == regex[1])
			{
				return true;
			}

			return false;
		}

		bool FindCode4(string org, int index,char[] regex)
		{
			if (index >= org.Length - 3)
				return false;


			if (org[index] == regex[0] && org[index + 1] == regex[1] && org[index + 2] == regex[2] && org[index + 3] == regex[3])
			{
				return true;
			}

			return false;
		}

	}
}
