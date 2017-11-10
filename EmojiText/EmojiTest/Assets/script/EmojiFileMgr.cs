using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiFileMgr  {

	public EmojiFile emojiFile;
	private static EmojiFileMgr emojiFileMgr;
	public EmojiFileMgr(){
		emojiFile = Resources.Load<EmojiFile> ("EmojiFile");
	}
	public static EmojiFileMgr getInstance(){
		if (emojiFileMgr == null)
			emojiFileMgr = new EmojiFileMgr ();
		return emojiFileMgr;
	}

	public class EmojiInfo{
		public char[] chars;
		public string key;
		public int x;
		public int y;
		public int size; 
	}
}
