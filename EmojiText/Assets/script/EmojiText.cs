using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EmojiText : Text {
	private static Dictionary<string,EmojiInfo> EmojiIndex = null;

	struct EmojiInfo
	{
		public float x;
		public float y;
		public float size;
		public int index;
	}

	readonly UIVertex[] m_TempVerts = new UIVertex[4];
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if (font == null)
			return;

		if (EmojiIndex == null) {
			EmojiIndex = new Dictionary<string, EmojiInfo>();

			//load emoji data, and you can overwrite this segment code base on your project.
			TextAsset emojiContent = Resources.Load<TextAsset> ("emoji");
			string[] lines = emojiContent.text.Split ('\n');
			for(int i = 1 ; i < lines.Length; i ++)
			{
				if (! string.IsNullOrEmpty (lines [i])) {
					string[] strs = lines [i].Split ('\t');
					EmojiInfo info;
					info.x = float.Parse (strs [3]);
					info.y = float.Parse (strs [4]);
					info.size = float.Parse (strs [5]);
					info.index = 0;
					EmojiIndex.Add (strs [1], info);
				}
			}
		}
		string emojitext = text;
		Dictionary<int,EmojiInfo> emojiDic = new Dictionary<int, EmojiInfo> ();
		int len = 0;
		if (supportRichText) {
			MatchCollection matches = Regex.Matches (text, "\\[[a-z0-9A-Z]+\\]");
			for (int i = 0; i < matches.Count; i++) {
				EmojiInfo info;
				if (EmojiIndex.TryGetValue (matches [i].Value, out info)) {
					info.index = matches [i].Index - len;
					len += matches [i].Length - 1;
					emojiDic.Add (info.index, info);
					emojitext = emojitext.Replace (matches[i].Value,"M");
				}
			}
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
				Debug.Log (verts [i].uv1.x + "---" + verts [i].uv1.y);
				EmojiInfo info;
				int index = i / 4;
				if (emojiDic.TryGetValue (index, out info)) {

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
					float pixelOffset = emojiDic [index].size / 32 / 2;
					m_TempVerts [0].uv1 = new Vector2 (emojiDic [index].x + pixelOffset, emojiDic [index].y + pixelOffset);
					m_TempVerts [1].uv1 = new Vector2 (emojiDic [index].x - pixelOffset + emojiDic [index].size, emojiDic [index].y + pixelOffset);
					m_TempVerts [2].uv1 = new Vector2 (emojiDic [index].x - pixelOffset + emojiDic [index].size, emojiDic [index].y - pixelOffset + emojiDic [index].size);
					m_TempVerts [3].uv1 = new Vector2 (emojiDic [index].x + pixelOffset, emojiDic [index].y - pixelOffset + emojiDic [index].size);

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
}
