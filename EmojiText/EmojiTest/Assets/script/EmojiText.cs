using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Text;

public class EmojiText : Text {
	private static Dictionary<string,EmojiTextInfo> EmojiIndex = null;

	struct EmojiTextInfo
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
		string emojitext = text;
		Dictionary<int,EmojiTextInfo> emojiDic = null;
		int len = 0;
		if (supportRichText) {
			EmojiFile emojiFile = EmojiFileMgr.getInstance ().emojiFile;
			List<EmojiFile.EmojiInfo> emojiInfos = emojiFile.emojiInfos;
			emojiDic = new Dictionary<int, EmojiTextInfo> ();
			StringBuilder sb = new StringBuilder ();
			List<EmojiDecoder.EmojiMsg> emojiMsgs = EmojiDecoder.instance.EncodeEmoji (emojitext);
			int lastIndex = 0;
			for(int i = 0,count = emojiMsgs.Count; i < count; i++){
				EmojiDecoder.EmojiMsg emojiMsg = emojiMsgs [i];
				EmojiFile.EmojiInfo emojiInfo;
				for(int j = 0;j<emojiInfos.Count;j++){
					emojiInfo = emojiInfos [j];
					if (emojiMsg.encodeName == emojiInfo.key) {
						break;
					}
				}

				EmojiTextInfo emojiTextInfo = new EmojiTextInfo ();
				emojiTextInfo.index = emojiMsg.index - len;

				len += emojiMsg.length - 1;
				sb.Append (emojitext.Substring(lastIndex,emojiMsg.index-lastIndex)).Append('M');
				lastIndex = emojiMsg.index + emojiMsg.length;
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
				Debug.Log (verts [i].uv1.x + "---" + verts [i].uv1.y);
				EmojiTextInfo info;
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
					float pixelOffset = 0;
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
	public class EmojiDecoder
	{
		public static EmojiDecoder instance = new EmojiDecoder();

		#region code
		List<char> code1 = new List<char>() { '\u00a9', '\u00ae', '\u203c', '\u2049', '\u2122', '\u2139', '\u2194', '\u2195', '\u2196', '\u2197', '\u2198', '\u2199', '\u21a9', '\u21aa', '\u231a', '\u231b', '\u23e9', '\u23ea', '\u23eb', '\u23ec', '\u23f0', '\u23f3', '\u24c2', '\u25aa', '\u25ab', '\u25b6', '\u25c0', '\u25fb', '\u25fc', '\u25fd', '\u25fe', '\u2600', '\u2601', '\u260e', '\u2611', '\u2614', '\u2615', '\u261d', '\u263a', '\u2648', '\u2649', '\u264a', '\u264b', '\u264c', '\u264d', '\u264e', '\u264f', '\u2650', '\u2651', '\u2652', '\u2653', '\u2660', '\u2663', '\u2665', '\u2666', '\u2668', '\u267b', '\u267f', '\u2693', '\u26a0', '\u26a1', '\u26aa', '\u26ab', '\u26bd', '\u26be', '\u26c4', '\u26c5', '\u26ce', '\u26d4', '\u26ea', '\u26f2', '\u26f3', '\u26f5', '\u26fa', '\u26fd', '\u2702', '\u2705', '\u2708', '\u2709', '\u270a', '\u270b', '\u270c', '\u270f', '\u2712', '\u2714', '\u2716', '\u2728', '\u2733', '\u2734', '\u2744', '\u2747', '\u274c', '\u274e', '\u2753', '\u2754', '\u2755', '\u2757', '\u2764', '\u2795', '\u2796', '\u2797', '\u27a1', '\u27b0', '\u27bf', '\u2934', '\u2935', '\u2b05', '\u2b06', '\u2b07', '\u2b1b', '\u2b1c', '\u2b50', '\u2b55', '\u3030', '\u303d', '\u3297', '\u3299' };
		List<char[]> code2 = new List<char[]>() 
		{ 
			new char[]{'\u0023','\u20e3',},
			new char[]{'\u0030','\u20e3',},
			new char[]{'\u0031','\u20e3',},
			new char[]{'\u0032','\u20e3',},
			new char[]{'\u0033','\u20e3',},
			new char[]{'\u0034','\u20e3',},
			new char[]{'\u0035','\u20e3',},
			new char[]{'\u0036','\u20e3',},
			new char[]{'\u0037','\u20e3',},
			new char[]{'\u0038','\u20e3',},
			new char[]{'\u0039','\u20e3',},
			new char[]{'\ud83c','\udc04',},
			new char[]{'\ud83c','\udccf',},
			new char[]{'\ud83c','\udd70',},
			new char[]{'\ud83c','\udd71',},
			new char[]{'\ud83c','\udd7e',},
			new char[]{'\ud83c','\udd7f',},
			new char[]{'\ud83c','\udd8e',},
			new char[]{'\ud83c','\udd91',},
			new char[]{'\ud83c','\udd92',},
			new char[]{'\ud83c','\udd93',},
			new char[]{'\ud83c','\udd94',},
			new char[]{'\ud83c','\udd95',},
			new char[]{'\ud83c','\udd96',},
			new char[]{'\ud83c','\udd97',},
			new char[]{'\ud83c','\udd98',},
			new char[]{'\ud83c','\udd99',},
			new char[]{'\ud83c','\udd9a',},
			new char[]{'\ud83c','\ude01',},
			new char[]{'\ud83c','\ude02',},
			new char[]{'\ud83c','\ude1a',},
			new char[]{'\ud83c','\ude2f',},
			new char[]{'\ud83c','\ude32',},
			new char[]{'\ud83c','\ude33',},
			new char[]{'\ud83c','\ude34',},
			new char[]{'\ud83c','\ude35',},
			new char[]{'\ud83c','\ude36',},
			new char[]{'\ud83c','\ude37',},
			new char[]{'\ud83c','\ude38',},
			new char[]{'\ud83c','\ude39',},
			new char[]{'\ud83c','\ude3a',},
			new char[]{'\ud83c','\ude50',},
			new char[]{'\ud83c','\ude51',},
			new char[]{'\ud83c','\udf00',},
			new char[]{'\ud83c','\udf01',},
			new char[]{'\ud83c','\udf02',},
			new char[]{'\ud83c','\udf03',},
			new char[]{'\ud83c','\udf04',},
			new char[]{'\ud83c','\udf05',},
			new char[]{'\ud83c','\udf06',},
			new char[]{'\ud83c','\udf07',},
			new char[]{'\ud83c','\udf08',},
			new char[]{'\ud83c','\udf09',},
			new char[]{'\ud83c','\udf0a',},
			new char[]{'\ud83c','\udf0b',},
			new char[]{'\ud83c','\udf0c',},
			new char[]{'\ud83c','\udf0d',},
			new char[]{'\ud83c','\udf0e',},
			new char[]{'\ud83c','\udf0f',},
			new char[]{'\ud83c','\udf10',},
			new char[]{'\ud83c','\udf11',},
			new char[]{'\ud83c','\udf12',},
			new char[]{'\ud83c','\udf13',},
			new char[]{'\ud83c','\udf14',},
			new char[]{'\ud83c','\udf15',},
			new char[]{'\ud83c','\udf16',},
			new char[]{'\ud83c','\udf17',},
			new char[]{'\ud83c','\udf18',},
			new char[]{'\ud83c','\udf19',},
			new char[]{'\ud83c','\udf1a',},
			new char[]{'\ud83c','\udf1b',},
			new char[]{'\ud83c','\udf1c',},
			new char[]{'\ud83c','\udf1d',},
			new char[]{'\ud83c','\udf1e',},
			new char[]{'\ud83c','\udf1f',},
			new char[]{'\ud83c','\udf20',},
			new char[]{'\ud83c','\udf30',},
			new char[]{'\ud83c','\udf31',},
			new char[]{'\ud83c','\udf32',},
			new char[]{'\ud83c','\udf33',},
			new char[]{'\ud83c','\udf34',},
			new char[]{'\ud83c','\udf35',},
			new char[]{'\ud83c','\udf37',},
			new char[]{'\ud83c','\udf38',},
			new char[]{'\ud83c','\udf39',},
			new char[]{'\ud83c','\udf3a',},
			new char[]{'\ud83c','\udf3b',},
			new char[]{'\ud83c','\udf3c',},
			new char[]{'\ud83c','\udf3d',},
			new char[]{'\ud83c','\udf3e',},
			new char[]{'\ud83c','\udf3f',},
			new char[]{'\ud83c','\udf40',},
			new char[]{'\ud83c','\udf41',},
			new char[]{'\ud83c','\udf42',},
			new char[]{'\ud83c','\udf43',},
			new char[]{'\ud83c','\udf44',},
			new char[]{'\ud83c','\udf45',},
			new char[]{'\ud83c','\udf46',},
			new char[]{'\ud83c','\udf47',},
			new char[]{'\ud83c','\udf48',},
			new char[]{'\ud83c','\udf49',},
			new char[]{'\ud83c','\udf4a',},
			new char[]{'\ud83c','\udf4b',},
			new char[]{'\ud83c','\udf4c',},
			new char[]{'\ud83c','\udf4d',},
			new char[]{'\ud83c','\udf4e',},
			new char[]{'\ud83c','\udf4f',},
			new char[]{'\ud83c','\udf50',},
			new char[]{'\ud83c','\udf51',},
			new char[]{'\ud83c','\udf52',},
			new char[]{'\ud83c','\udf53',},
			new char[]{'\ud83c','\udf54',},
			new char[]{'\ud83c','\udf55',},
			new char[]{'\ud83c','\udf56',},
			new char[]{'\ud83c','\udf57',},
			new char[]{'\ud83c','\udf58',},
			new char[]{'\ud83c','\udf59',},
			new char[]{'\ud83c','\udf5a',},
			new char[]{'\ud83c','\udf5b',},
			new char[]{'\ud83c','\udf5c',},
			new char[]{'\ud83c','\udf5d',},
			new char[]{'\ud83c','\udf5e',},
			new char[]{'\ud83c','\udf5f',},
			new char[]{'\ud83c','\udf60',},
			new char[]{'\ud83c','\udf61',},
			new char[]{'\ud83c','\udf62',},
			new char[]{'\ud83c','\udf63',},
			new char[]{'\ud83c','\udf64',},
			new char[]{'\ud83c','\udf65',},
			new char[]{'\ud83c','\udf66',},
			new char[]{'\ud83c','\udf67',},
			new char[]{'\ud83c','\udf68',},
			new char[]{'\ud83c','\udf69',},
			new char[]{'\ud83c','\udf6a',},
			new char[]{'\ud83c','\udf6b',},
			new char[]{'\ud83c','\udf6c',},
			new char[]{'\ud83c','\udf6d',},
			new char[]{'\ud83c','\udf6e',},
			new char[]{'\ud83c','\udf6f',},
			new char[]{'\ud83c','\udf70',},
			new char[]{'\ud83c','\udf71',},
			new char[]{'\ud83c','\udf72',},
			new char[]{'\ud83c','\udf73',},
			new char[]{'\ud83c','\udf74',},
			new char[]{'\ud83c','\udf75',},
			new char[]{'\ud83c','\udf76',},
			new char[]{'\ud83c','\udf77',},
			new char[]{'\ud83c','\udf78',},
			new char[]{'\ud83c','\udf79',},
			new char[]{'\ud83c','\udf7a',},
			new char[]{'\ud83c','\udf7b',},
			new char[]{'\ud83c','\udf7c',},
			new char[]{'\ud83c','\udf80',},
			new char[]{'\ud83c','\udf81',},
			new char[]{'\ud83c','\udf82',},
			new char[]{'\ud83c','\udf83',},
			new char[]{'\ud83c','\udf84',},
			new char[]{'\ud83c','\udf85',},
			new char[]{'\ud83c','\udf86',},
			new char[]{'\ud83c','\udf87',},
			new char[]{'\ud83c','\udf88',},
			new char[]{'\ud83c','\udf89',},
			new char[]{'\ud83c','\udf8a',},
			new char[]{'\ud83c','\udf8b',},
			new char[]{'\ud83c','\udf8c',},
			new char[]{'\ud83c','\udf8d',},
			new char[]{'\ud83c','\udf8e',},
			new char[]{'\ud83c','\udf8f',},
			new char[]{'\ud83c','\udf90',},
			new char[]{'\ud83c','\udf91',},
			new char[]{'\ud83c','\udf92',},
			new char[]{'\ud83c','\udf93',},
			new char[]{'\ud83c','\udfa0',},
			new char[]{'\ud83c','\udfa1',},
			new char[]{'\ud83c','\udfa2',},
			new char[]{'\ud83c','\udfa3',},
			new char[]{'\ud83c','\udfa4',},
			new char[]{'\ud83c','\udfa5',},
			new char[]{'\ud83c','\udfa6',},
			new char[]{'\ud83c','\udfa7',},
			new char[]{'\ud83c','\udfa8',},
			new char[]{'\ud83c','\udfa9',},
			new char[]{'\ud83c','\udfaa',},
			new char[]{'\ud83c','\udfab',},
			new char[]{'\ud83c','\udfac',},
			new char[]{'\ud83c','\udfad',},
			new char[]{'\ud83c','\udfae',},
			new char[]{'\ud83c','\udfaf',},
			new char[]{'\ud83c','\udfb0',},
			new char[]{'\ud83c','\udfb1',},
			new char[]{'\ud83c','\udfb2',},
			new char[]{'\ud83c','\udfb3',},
			new char[]{'\ud83c','\udfb4',},
			new char[]{'\ud83c','\udfb5',},
			new char[]{'\ud83c','\udfb6',},
			new char[]{'\ud83c','\udfb7',},
			new char[]{'\ud83c','\udfb8',},
			new char[]{'\ud83c','\udfb9',},
			new char[]{'\ud83c','\udfba',},
			new char[]{'\ud83c','\udfbb',},
			new char[]{'\ud83c','\udfbc',},
			new char[]{'\ud83c','\udfbd',},
			new char[]{'\ud83c','\udfbe',},
			new char[]{'\ud83c','\udfbf',},
			new char[]{'\ud83c','\udfc0',},
			new char[]{'\ud83c','\udfc1',},
			new char[]{'\ud83c','\udfc2',},
			new char[]{'\ud83c','\udfc3',},
			new char[]{'\ud83c','\udfc4',},
			new char[]{'\ud83c','\udfc6',},
			new char[]{'\ud83c','\udfc7',},
			new char[]{'\ud83c','\udfc8',},
			new char[]{'\ud83c','\udfc9',},
			new char[]{'\ud83c','\udfca',},
			new char[]{'\ud83c','\udfe0',},
			new char[]{'\ud83c','\udfe1',},
			new char[]{'\ud83c','\udfe2',},
			new char[]{'\ud83c','\udfe3',},
			new char[]{'\ud83c','\udfe4',},
			new char[]{'\ud83c','\udfe5',},
			new char[]{'\ud83c','\udfe6',},
			new char[]{'\ud83c','\udfe7',},
			new char[]{'\ud83c','\udfe8',},
			new char[]{'\ud83c','\udfe9',},
			new char[]{'\ud83c','\udfea',},
			new char[]{'\ud83c','\udfeb',},
			new char[]{'\ud83c','\udfec',},
			new char[]{'\ud83c','\udfed',},
			new char[]{'\ud83c','\udfee',},
			new char[]{'\ud83c','\udfef',},
			new char[]{'\ud83c','\udff0',},
			new char[]{'\ud83d','\udc00',},
			new char[]{'\ud83d','\udc01',},
			new char[]{'\ud83d','\udc02',},
			new char[]{'\ud83d','\udc03',},
			new char[]{'\ud83d','\udc04',},
			new char[]{'\ud83d','\udc05',},
			new char[]{'\ud83d','\udc06',},
			new char[]{'\ud83d','\udc07',},
			new char[]{'\ud83d','\udc08',},
			new char[]{'\ud83d','\udc09',},
			new char[]{'\ud83d','\udc0a',},
			new char[]{'\ud83d','\udc0b',},
			new char[]{'\ud83d','\udc0c',},
			new char[]{'\ud83d','\udc0d',},
			new char[]{'\ud83d','\udc0e',},
			new char[]{'\ud83d','\udc0f',},
			new char[]{'\ud83d','\udc10',},
			new char[]{'\ud83d','\udc11',},
			new char[]{'\ud83d','\udc12',},
			new char[]{'\ud83d','\udc13',},
			new char[]{'\ud83d','\udc14',},
			new char[]{'\ud83d','\udc15',},
			new char[]{'\ud83d','\udc16',},
			new char[]{'\ud83d','\udc17',},
			new char[]{'\ud83d','\udc18',},
			new char[]{'\ud83d','\udc19',},
			new char[]{'\ud83d','\udc1a',},
			new char[]{'\ud83d','\udc1b',},
			new char[]{'\ud83d','\udc1c',},
			new char[]{'\ud83d','\udc1d',},
			new char[]{'\ud83d','\udc1e',},
			new char[]{'\ud83d','\udc1f',},
			new char[]{'\ud83d','\udc20',},
			new char[]{'\ud83d','\udc21',},
			new char[]{'\ud83d','\udc22',},
			new char[]{'\ud83d','\udc23',},
			new char[]{'\ud83d','\udc24',},
			new char[]{'\ud83d','\udc25',},
			new char[]{'\ud83d','\udc26',},
			new char[]{'\ud83d','\udc27',},
			new char[]{'\ud83d','\udc28',},
			new char[]{'\ud83d','\udc29',},
			new char[]{'\ud83d','\udc2a',},
			new char[]{'\ud83d','\udc2b',},
			new char[]{'\ud83d','\udc2c',},
			new char[]{'\ud83d','\udc2d',},
			new char[]{'\ud83d','\udc2e',},
			new char[]{'\ud83d','\udc2f',},
			new char[]{'\ud83d','\udc30',},
			new char[]{'\ud83d','\udc31',},
			new char[]{'\ud83d','\udc32',},
			new char[]{'\ud83d','\udc33',},
			new char[]{'\ud83d','\udc34',},
			new char[]{'\ud83d','\udc35',},
			new char[]{'\ud83d','\udc36',},
			new char[]{'\ud83d','\udc37',},
			new char[]{'\ud83d','\udc38',},
			new char[]{'\ud83d','\udc39',},
			new char[]{'\ud83d','\udc3a',},
			new char[]{'\ud83d','\udc3b',},
			new char[]{'\ud83d','\udc3c',},
			new char[]{'\ud83d','\udc3d',},
			new char[]{'\ud83d','\udc3e',},
			new char[]{'\ud83d','\udc40',},
			new char[]{'\ud83d','\udc42',},
			new char[]{'\ud83d','\udc43',},
			new char[]{'\ud83d','\udc44',},
			new char[]{'\ud83d','\udc45',},
			new char[]{'\ud83d','\udc46',},
			new char[]{'\ud83d','\udc47',},
			new char[]{'\ud83d','\udc48',},
			new char[]{'\ud83d','\udc49',},
			new char[]{'\ud83d','\udc4a',},
			new char[]{'\ud83d','\udc4b',},
			new char[]{'\ud83d','\udc4c',},
			new char[]{'\ud83d','\udc4d',},
			new char[]{'\ud83d','\udc4e',},
			new char[]{'\ud83d','\udc4f',},
			new char[]{'\ud83d','\udc50',},
			new char[]{'\ud83d','\udc51',},
			new char[]{'\ud83d','\udc52',},
			new char[]{'\ud83d','\udc53',},
			new char[]{'\ud83d','\udc54',},
			new char[]{'\ud83d','\udc55',},
			new char[]{'\ud83d','\udc56',},
			new char[]{'\ud83d','\udc57',},
			new char[]{'\ud83d','\udc58',},
			new char[]{'\ud83d','\udc59',},
			new char[]{'\ud83d','\udc5a',},
			new char[]{'\ud83d','\udc5b',},
			new char[]{'\ud83d','\udc5c',},
			new char[]{'\ud83d','\udc5d',},
			new char[]{'\ud83d','\udc5e',},
			new char[]{'\ud83d','\udc5f',},
			new char[]{'\ud83d','\udc60',},
			new char[]{'\ud83d','\udc61',},
			new char[]{'\ud83d','\udc62',},
			new char[]{'\ud83d','\udc63',},
			new char[]{'\ud83d','\udc64',},
			new char[]{'\ud83d','\udc65',},
			new char[]{'\ud83d','\udc66',},
			new char[]{'\ud83d','\udc67',},
			new char[]{'\ud83d','\udc68',},
			new char[]{'\ud83d','\udc69',},
			new char[]{'\ud83d','\udc6a',},
			new char[]{'\ud83d','\udc6b',},
			new char[]{'\ud83d','\udc6c',},
			new char[]{'\ud83d','\udc6d',},
			new char[]{'\ud83d','\udc6e',},
			new char[]{'\ud83d','\udc6f',},
			new char[]{'\ud83d','\udc70',},
			new char[]{'\ud83d','\udc71',},
			new char[]{'\ud83d','\udc72',},
			new char[]{'\ud83d','\udc73',},
			new char[]{'\ud83d','\udc74',},
			new char[]{'\ud83d','\udc75',},
			new char[]{'\ud83d','\udc76',},
			new char[]{'\ud83d','\udc77',},
			new char[]{'\ud83d','\udc78',},
			new char[]{'\ud83d','\udc79',},
			new char[]{'\ud83d','\udc7a',},
			new char[]{'\ud83d','\udc7b',},
			new char[]{'\ud83d','\udc7c',},
			new char[]{'\ud83d','\udc7d',},
			new char[]{'\ud83d','\udc7e',},
			new char[]{'\ud83d','\udc7f',},
			new char[]{'\ud83d','\udc80',},
			new char[]{'\ud83d','\udc81',},
			new char[]{'\ud83d','\udc82',},
			new char[]{'\ud83d','\udc83',},
			new char[]{'\ud83d','\udc84',},
			new char[]{'\ud83d','\udc85',},
			new char[]{'\ud83d','\udc86',},
			new char[]{'\ud83d','\udc87',},
			new char[]{'\ud83d','\udc88',},
			new char[]{'\ud83d','\udc89',},
			new char[]{'\ud83d','\udc8a',},
			new char[]{'\ud83d','\udc8b',},
			new char[]{'\ud83d','\udc8c',},
			new char[]{'\ud83d','\udc8d',},
			new char[]{'\ud83d','\udc8e',},
			new char[]{'\ud83d','\udc8f',},
			new char[]{'\ud83d','\udc90',},
			new char[]{'\ud83d','\udc91',},
			new char[]{'\ud83d','\udc92',},
			new char[]{'\ud83d','\udc93',},
			new char[]{'\ud83d','\udc94',},
			new char[]{'\ud83d','\udc95',},
			new char[]{'\ud83d','\udc96',},
			new char[]{'\ud83d','\udc97',},
			new char[]{'\ud83d','\udc98',},
			new char[]{'\ud83d','\udc99',},
			new char[]{'\ud83d','\udc9a',},
			new char[]{'\ud83d','\udc9b',},
			new char[]{'\ud83d','\udc9c',},
			new char[]{'\ud83d','\udc9d',},
			new char[]{'\ud83d','\udc9e',},
			new char[]{'\ud83d','\udc9f',},
			new char[]{'\ud83d','\udca0',},
			new char[]{'\ud83d','\udca1',},
			new char[]{'\ud83d','\udca2',},
			new char[]{'\ud83d','\udca3',},
			new char[]{'\ud83d','\udca4',},
			new char[]{'\ud83d','\udca5',},
			new char[]{'\ud83d','\udca6',},
			new char[]{'\ud83d','\udca7',},
			new char[]{'\ud83d','\udca8',},
			new char[]{'\ud83d','\udca9',},
			new char[]{'\ud83d','\udcaa',},
			new char[]{'\ud83d','\udcab',},
			new char[]{'\ud83d','\udcac',},
			new char[]{'\ud83d','\udcad',},
			new char[]{'\ud83d','\udcae',},
			new char[]{'\ud83d','\udcaf',},
			new char[]{'\ud83d','\udcb0',},
			new char[]{'\ud83d','\udcb1',},
			new char[]{'\ud83d','\udcb2',},
			new char[]{'\ud83d','\udcb3',},
			new char[]{'\ud83d','\udcb4',},
			new char[]{'\ud83d','\udcb5',},
			new char[]{'\ud83d','\udcb6',},
			new char[]{'\ud83d','\udcb7',},
			new char[]{'\ud83d','\udcb8',},
			new char[]{'\ud83d','\udcb9',},
			new char[]{'\ud83d','\udcba',},
			new char[]{'\ud83d','\udcbb',},
			new char[]{'\ud83d','\udcbc',},
			new char[]{'\ud83d','\udcbd',},
			new char[]{'\ud83d','\udcbe',},
			new char[]{'\ud83d','\udcbf',},
			new char[]{'\ud83d','\udcc0',},
			new char[]{'\ud83d','\udcc1',},
			new char[]{'\ud83d','\udcc2',},
			new char[]{'\ud83d','\udcc3',},
			new char[]{'\ud83d','\udcc4',},
			new char[]{'\ud83d','\udcc5',},
			new char[]{'\ud83d','\udcc6',},
			new char[]{'\ud83d','\udcc7',},
			new char[]{'\ud83d','\udcc8',},
			new char[]{'\ud83d','\udcc9',},
			new char[]{'\ud83d','\udcca',},
			new char[]{'\ud83d','\udccb',},
			new char[]{'\ud83d','\udccc',},
			new char[]{'\ud83d','\udccd',},
			new char[]{'\ud83d','\udcce',},
			new char[]{'\ud83d','\udccf',},
			new char[]{'\ud83d','\udcd0',},
			new char[]{'\ud83d','\udcd1',},
			new char[]{'\ud83d','\udcd2',},
			new char[]{'\ud83d','\udcd3',},
			new char[]{'\ud83d','\udcd4',},
			new char[]{'\ud83d','\udcd5',},
			new char[]{'\ud83d','\udcd6',},
			new char[]{'\ud83d','\udcd7',},
			new char[]{'\ud83d','\udcd8',},
			new char[]{'\ud83d','\udcd9',},
			new char[]{'\ud83d','\udcda',},
			new char[]{'\ud83d','\udcdb',},
			new char[]{'\ud83d','\udcdc',},
			new char[]{'\ud83d','\udcdd',},
			new char[]{'\ud83d','\udcde',},
			new char[]{'\ud83d','\udcdf',},
			new char[]{'\ud83d','\udce0',},
			new char[]{'\ud83d','\udce1',},
			new char[]{'\ud83d','\udce2',},
			new char[]{'\ud83d','\udce3',},
			new char[]{'\ud83d','\udce4',},
			new char[]{'\ud83d','\udce5',},
			new char[]{'\ud83d','\udce6',},
			new char[]{'\ud83d','\udce7',},
			new char[]{'\ud83d','\udce8',},
			new char[]{'\ud83d','\udce9',},
			new char[]{'\ud83d','\udcea',},
			new char[]{'\ud83d','\udceb',},
			new char[]{'\ud83d','\udcec',},
			new char[]{'\ud83d','\udced',},
			new char[]{'\ud83d','\udcee',},
			new char[]{'\ud83d','\udcef',},
			new char[]{'\ud83d','\udcf0',},
			new char[]{'\ud83d','\udcf1',},
			new char[]{'\ud83d','\udcf2',},
			new char[]{'\ud83d','\udcf3',},
			new char[]{'\ud83d','\udcf4',},
			new char[]{'\ud83d','\udcf5',},
			new char[]{'\ud83d','\udcf6',},
			new char[]{'\ud83d','\udcf7',},
			new char[]{'\ud83d','\udcf9',},
			new char[]{'\ud83d','\udcfa',},
			new char[]{'\ud83d','\udcfb',},
			new char[]{'\ud83d','\udcfc',},
			new char[]{'\ud83d','\udd00',},
			new char[]{'\ud83d','\udd01',},
			new char[]{'\ud83d','\udd02',},
			new char[]{'\ud83d','\udd03',},
			new char[]{'\ud83d','\udd04',},
			new char[]{'\ud83d','\udd05',},
			new char[]{'\ud83d','\udd06',},
			new char[]{'\ud83d','\udd07',},
			new char[]{'\ud83d','\udd08',},
			new char[]{'\ud83d','\udd09',},
			new char[]{'\ud83d','\udd0a',},
			new char[]{'\ud83d','\udd0b',},
			new char[]{'\ud83d','\udd0c',},
			new char[]{'\ud83d','\udd0d',},
			new char[]{'\ud83d','\udd0e',},
			new char[]{'\ud83d','\udd0f',},
			new char[]{'\ud83d','\udd10',},
			new char[]{'\ud83d','\udd11',},
			new char[]{'\ud83d','\udd12',},
			new char[]{'\ud83d','\udd13',},
			new char[]{'\ud83d','\udd14',},
			new char[]{'\ud83d','\udd15',},
			new char[]{'\ud83d','\udd16',},
			new char[]{'\ud83d','\udd17',},
			new char[]{'\ud83d','\udd18',},
			new char[]{'\ud83d','\udd19',},
			new char[]{'\ud83d','\udd1a',},
			new char[]{'\ud83d','\udd1b',},
			new char[]{'\ud83d','\udd1c',},
			new char[]{'\ud83d','\udd1d',},
			new char[]{'\ud83d','\udd1e',},
			new char[]{'\ud83d','\udd1f',},
			new char[]{'\ud83d','\udd20',},
			new char[]{'\ud83d','\udd21',},
			new char[]{'\ud83d','\udd22',},
			new char[]{'\ud83d','\udd23',},
			new char[]{'\ud83d','\udd24',},
			new char[]{'\ud83d','\udd25',},
			new char[]{'\ud83d','\udd26',},
			new char[]{'\ud83d','\udd27',},
			new char[]{'\ud83d','\udd28',},
			new char[]{'\ud83d','\udd29',},
			new char[]{'\ud83d','\udd2a',},
			new char[]{'\ud83d','\udd2b',},
			new char[]{'\ud83d','\udd2c',},
			new char[]{'\ud83d','\udd2d',},
			new char[]{'\ud83d','\udd2e',},
			new char[]{'\ud83d','\udd2f',},
			new char[]{'\ud83d','\udd30',},
			new char[]{'\ud83d','\udd31',},
			new char[]{'\ud83d','\udd32',},
			new char[]{'\ud83d','\udd33',},
			new char[]{'\ud83d','\udd34',},
			new char[]{'\ud83d','\udd35',},
			new char[]{'\ud83d','\udd36',},
			new char[]{'\ud83d','\udd37',},
			new char[]{'\ud83d','\udd38',},
			new char[]{'\ud83d','\udd39',},
			new char[]{'\ud83d','\udd3a',},
			new char[]{'\ud83d','\udd3b',},
			new char[]{'\ud83d','\udd3c',},
			new char[]{'\ud83d','\udd3d',},
			new char[]{'\ud83d','\udd50',},
			new char[]{'\ud83d','\udd51',},
			new char[]{'\ud83d','\udd52',},
			new char[]{'\ud83d','\udd53',},
			new char[]{'\ud83d','\udd54',},
			new char[]{'\ud83d','\udd55',},
			new char[]{'\ud83d','\udd56',},
			new char[]{'\ud83d','\udd57',},
			new char[]{'\ud83d','\udd58',},
			new char[]{'\ud83d','\udd59',},
			new char[]{'\ud83d','\udd5a',},
			new char[]{'\ud83d','\udd5b',},
			new char[]{'\ud83d','\udd5c',},
			new char[]{'\ud83d','\udd5d',},
			new char[]{'\ud83d','\udd5e',},
			new char[]{'\ud83d','\udd5f',},
			new char[]{'\ud83d','\udd60',},
			new char[]{'\ud83d','\udd61',},
			new char[]{'\ud83d','\udd62',},
			new char[]{'\ud83d','\udd63',},
			new char[]{'\ud83d','\udd64',},
			new char[]{'\ud83d','\udd65',},
			new char[]{'\ud83d','\udd66',},
			new char[]{'\ud83d','\udd67',},
			new char[]{'\ud83d','\uddfb',},
			new char[]{'\ud83d','\uddfc',},
			new char[]{'\ud83d','\uddfd',},
			new char[]{'\ud83d','\uddfe',},
			new char[]{'\ud83d','\uddff',},
			new char[]{'\ud83d','\ude00',},
			new char[]{'\ud83d','\ude01',},
			new char[]{'\ud83d','\ude02',},
			new char[]{'\ud83d','\ude03',},
			new char[]{'\ud83d','\ude04',},
			new char[]{'\ud83d','\ude05',},
			new char[]{'\ud83d','\ude06',},
			new char[]{'\ud83d','\ude07',},
			new char[]{'\ud83d','\ude08',},
			new char[]{'\ud83d','\ude09',},
			new char[]{'\ud83d','\ude0a',},
			new char[]{'\ud83d','\ude0b',},
			new char[]{'\ud83d','\ude0c',},
			new char[]{'\ud83d','\ude0d',},
			new char[]{'\ud83d','\ude0e',},
			new char[]{'\ud83d','\ude0f',},
			new char[]{'\ud83d','\ude10',},
			new char[]{'\ud83d','\ude11',},
			new char[]{'\ud83d','\ude12',},
			new char[]{'\ud83d','\ude13',},
			new char[]{'\ud83d','\ude14',},
			new char[]{'\ud83d','\ude15',},
			new char[]{'\ud83d','\ude16',},
			new char[]{'\ud83d','\ude17',},
			new char[]{'\ud83d','\ude18',},
			new char[]{'\ud83d','\ude19',},
			new char[]{'\ud83d','\ude1a',},
			new char[]{'\ud83d','\ude1b',},
			new char[]{'\ud83d','\ude1c',},
			new char[]{'\ud83d','\ude1d',},
			new char[]{'\ud83d','\ude1e',},
			new char[]{'\ud83d','\ude1f',},
			new char[]{'\ud83d','\ude20',},
			new char[]{'\ud83d','\ude21',},
			new char[]{'\ud83d','\ude22',},
			new char[]{'\ud83d','\ude23',},
			new char[]{'\ud83d','\ude24',},
			new char[]{'\ud83d','\ude25',},
			new char[]{'\ud83d','\ude26',},
			new char[]{'\ud83d','\ude27',},
			new char[]{'\ud83d','\ude28',},
			new char[]{'\ud83d','\ude29',},
			new char[]{'\ud83d','\ude2a',},
			new char[]{'\ud83d','\ude2b',},
			new char[]{'\ud83d','\ude2c',},
			new char[]{'\ud83d','\ude2d',},
			new char[]{'\ud83d','\ude2e',},
			new char[]{'\ud83d','\ude2f',},
			new char[]{'\ud83d','\ude30',},
			new char[]{'\ud83d','\ude31',},
			new char[]{'\ud83d','\ude32',},
			new char[]{'\ud83d','\ude33',},
			new char[]{'\ud83d','\ude34',},
			new char[]{'\ud83d','\ude35',},
			new char[]{'\ud83d','\ude36',},
			new char[]{'\ud83d','\ude37',},
			new char[]{'\ud83d','\ude38',},
			new char[]{'\ud83d','\ude39',},
			new char[]{'\ud83d','\ude3a',},
			new char[]{'\ud83d','\ude3b',},
			new char[]{'\ud83d','\ude3c',},
			new char[]{'\ud83d','\ude3d',},
			new char[]{'\ud83d','\ude3e',},
			new char[]{'\ud83d','\ude3f',},
			new char[]{'\ud83d','\ude40',},
			new char[]{'\ud83d','\ude45',},
			new char[]{'\ud83d','\ude46',},
			new char[]{'\ud83d','\ude47',},
			new char[]{'\ud83d','\ude48',},
			new char[]{'\ud83d','\ude49',},
			new char[]{'\ud83d','\ude4a',},
			new char[]{'\ud83d','\ude4b',},
			new char[]{'\ud83d','\ude4c',},
			new char[]{'\ud83d','\ude4d',},
			new char[]{'\ud83d','\ude4e',},
			new char[]{'\ud83d','\ude4f',},
			new char[]{'\ud83d','\ude80',},
			new char[]{'\ud83d','\ude81',},
			new char[]{'\ud83d','\ude82',},
			new char[]{'\ud83d','\ude83',},
			new char[]{'\ud83d','\ude84',},
			new char[]{'\ud83d','\ude85',},
			new char[]{'\ud83d','\ude86',},
			new char[]{'\ud83d','\ude87',},
			new char[]{'\ud83d','\ude88',},
			new char[]{'\ud83d','\ude89',},
			new char[]{'\ud83d','\ude8a',},
			new char[]{'\ud83d','\ude8b',},
			new char[]{'\ud83d','\ude8c',},
			new char[]{'\ud83d','\ude8d',},
			new char[]{'\ud83d','\ude8e',},
			new char[]{'\ud83d','\ude8f',},
			new char[]{'\ud83d','\ude90',},
			new char[]{'\ud83d','\ude91',},
			new char[]{'\ud83d','\ude92',},
			new char[]{'\ud83d','\ude93',},
			new char[]{'\ud83d','\ude94',},
			new char[]{'\ud83d','\ude95',},
			new char[]{'\ud83d','\ude96',},
			new char[]{'\ud83d','\ude97',},
			new char[]{'\ud83d','\ude98',},
			new char[]{'\ud83d','\ude99',},
			new char[]{'\ud83d','\ude9a',},
			new char[]{'\ud83d','\ude9b',},
			new char[]{'\ud83d','\ude9c',},
			new char[]{'\ud83d','\ude9d',},
			new char[]{'\ud83d','\ude9e',},
			new char[]{'\ud83d','\ude9f',},
			new char[]{'\ud83d','\udea0',},
			new char[]{'\ud83d','\udea1',},
			new char[]{'\ud83d','\udea2',},
			new char[]{'\ud83d','\udea3',},
			new char[]{'\ud83d','\udea4',},
			new char[]{'\ud83d','\udea5',},
			new char[]{'\ud83d','\udea6',},
			new char[]{'\ud83d','\udea7',},
			new char[]{'\ud83d','\udea8',},
			new char[]{'\ud83d','\udea9',},
			new char[]{'\ud83d','\udeaa',},
			new char[]{'\ud83d','\udeab',},
			new char[]{'\ud83d','\udeac',},
			new char[]{'\ud83d','\udead',},
			new char[]{'\ud83d','\udeae',},
			new char[]{'\ud83d','\udeaf',},
			new char[]{'\ud83d','\udeb0',},
			new char[]{'\ud83d','\udeb1',},
			new char[]{'\ud83d','\udeb2',},
			new char[]{'\ud83d','\udeb3',},
			new char[]{'\ud83d','\udeb4',},
			new char[]{'\ud83d','\udeb5',},
			new char[]{'\ud83d','\udeb6',},
			new char[]{'\ud83d','\udeb7',},
			new char[]{'\ud83d','\udeb8',},
			new char[]{'\ud83d','\udeb9',},
			new char[]{'\ud83d','\udeba',},
			new char[]{'\ud83d','\udebb',},
			new char[]{'\ud83d','\udebc',},
			new char[]{'\ud83d','\udebd',},
			new char[]{'\ud83d','\udebe',},
			new char[]{'\ud83d','\udebf',},
			new char[]{'\ud83d','\udec0',},
			new char[]{'\ud83d','\udec1',},
			new char[]{'\ud83d','\udec2',},
			new char[]{'\ud83d','\udec3',},
			new char[]{'\ud83d','\udec4',},
			new char[]{'\ud83d','\udec5',}

		};
		List<char[]> code4 = new List<char[]>()
		{
			new char[]{'\ud83c','\udde8','\ud83c','\uddf3',},
			new char[]{'\ud83c','\udde9','\ud83c','\uddea',},
			new char[]{'\ud83c','\uddea','\ud83c','\uddf8',},
			new char[]{'\ud83c','\uddeb','\ud83c','\uddf7',},
			new char[]{'\ud83c','\uddec','\ud83c','\udde7',},
			new char[]{'\ud83c','\uddee','\ud83c','\uddf9',},
			new char[]{'\ud83c','\uddef','\ud83c','\uddf5',},
			new char[]{'\ud83c','\uddf0','\ud83c','\uddf7',},
			new char[]{'\ud83c','\uddf7','\ud83c','\uddfa',},
			new char[]{'\ud83c','\uddfa','\ud83c','\uddf8',}
		};
		#endregion
		public struct EmojiMsg
		{
			public int index;
			public int length;
			public string encodeName;
		}

		public List<EmojiMsg> EncodeEmoji(string org)
		{
			List<EmojiMsg> emojiMsgs = new List<EmojiMsg>();
			for (int i = 0; i < org.Length; i++)
			{
				if (FindCode1(org, i))
				{
					EmojiMsg info = new EmojiMsg();
					info.index = i;
					info.length = 1;
					info.encodeName = EncodeName (org, i, 1);
					emojiMsgs.Add (info);
				}
				else if (FindCode2(org, i))
				{
					EmojiMsg info = new EmojiMsg();
					info.index = i;
					info.length = 2;
					info.encodeName = EncodeName (org, i, 2);
					i += 1;
					emojiMsgs.Add (info);
				}
				else if (FindCode4(org, i))
				{
					EmojiMsg info = new EmojiMsg();
					info.index = i;
					info.length = 4;
					info.encodeName = EncodeName (org, i, 4);
					i += 3;
					emojiMsgs.Add (info);
				}
			}

			return emojiMsgs;
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

		bool FindCode1(string org, int index)
		{
			char v = org[index];
			int rv = code1.IndexOf(v);
			return rv != -1;
		}

		bool FindCode2(string org, int index)
		{
			if (index >= org.Length - 1)
				return false;

			for (int i = 0; i < code2.Count; i++)
			{
				char[] tmp = code2[i];

				if (org[index] == tmp[0] && org[index + 1] == tmp[1])
				{
					return true;
				}
			}
			return false;
		}

		bool FindCode4(string org, int index)
		{
			if (index >= org.Length - 3)
				return false;

			for (int i = 0; i < code4.Count; i++)
			{
				char[] tmp = code4[i];

				if (org[index] == tmp[0] && org[index + 1] == tmp[1] && org[index + 2] == tmp[2] && org[index + 3] == tmp[3])
				{
					return true;
				}
			}
			return false;
		}

	}
}
