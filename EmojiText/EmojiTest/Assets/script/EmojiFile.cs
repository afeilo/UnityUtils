using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version file.
/// 记录版本文件相关的信息
/// </summary>
[CreateAssetMenu(menuName="Assets/Create VersionFile ")]
public class EmojiFile : ScriptableObject {
	public int size;
	public int count;
	public List<EmojiInfo> emojiInfos;
	/// <summary>
	/// AB info.
	/// 记录ab信息
	/// </summary>
	[System.Serializable]
	public class EmojiInfo{
		public string key;
		public int x;
		public int y;
		public int size; 
	}
}
