/*

	Description:Create the Atlas of emojis and its data texture.

	How to use?
	1)
		Put all emojies in Asset/Framework/Resource/Emoji/Input.
		Multi-frame emoji name format : Name_Index.png , Single frame emoji format: Name.png
	2)
		Excute EmojiText->Build Emoji from menu in Unity.
	3)
		It will outputs two textures and a txt in Emoji/Output.
		Drag emoji_tex to "Emoji Texture" and emoji_data to "Emoji Data" in UGUIEmoji material.
	4)
		Repair the value of "Emoji count of every line" base on emoji_tex.png.
	5)
		It will auto copys emoji.txt to Resources, and you can overwrite relevant functions base on your project.
	
	Author:zouchunyi
	E-mail:zouchunyi@kingsoft.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class EmojiBuilder  {

	private const string OutputPath = "Assets/Emoji/Output/";
	private const string InputPath = "Assets/Emoji/";

	private static readonly Vector2[] AtlasSize = new Vector2[]{
		new Vector2(32,32),
		new Vector2(64,64),
		new Vector2(128,128),
		new Vector2(256,256),
		new Vector2(512,512),
		new Vector2(1024,1024),
		new Vector2(2048,2048)
	};

	struct Emoji
	{
		public string key;
		public int x;
		public int y;
		public int size;
	}
	private const int EmojiSize = 32;//the size of emoji.

	[MenuItem("EmojiText/Build Emoji")]
	public static void BuildEmoji()
	{
		List<char> keylist = new List<char> ();
		for (int i = 48; i <= 57; i++) {
			keylist.Add (System.Convert.ToChar(i));//0-9
		}
		for (int i = 65; i <= 90; i++) {
			keylist.Add (System.Convert.ToChar(i));//A-Z
		}
		for (int i = 97; i <= 122; i++) {
			keylist.Add (System.Convert.ToChar(i));//a-z
		}

		//search all emojis and compute they frames.
		Dictionary<string,int> sourceDic = new Dictionary<string,int> ();
		string[] files = Directory.GetFiles (InputPath,"*.png");
		for (int i = 0; i < files.Length; i++) {
			string[] strs = files [i].Split ('/');
			string[] strs1 = strs[strs.Length-1].Split ('.');
			string filename = strs1 [0];
			sourceDic.Add (filename, 1);
		}
			
		//create the directory if it is not exist.
		if (!Directory.Exists (OutputPath)) {
			Directory.CreateDirectory (OutputPath);
		}	

		List<Emoji> emojiDic = new List<Emoji> ();

		int totalFrames = 0;
		foreach (int value in sourceDic.Values) {
			totalFrames += value;
		}
		Vector2 texSize = ComputeAtlasSize (totalFrames);
		Debug.Log (texSize.x + "--" + texSize.y);
		Texture2D newTex = new Texture2D ((int)texSize.x, (int)texSize.y, TextureFormat.ARGB4444, false);
		int x = 0;
		int y = 0;
		int keyindex = 0;
		foreach (string key in sourceDic.Keys) {

			for (int index = 0; index < sourceDic[key]; index++) {
				
				string path = InputPath + key +".png";

				Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D> (path);
				Color[] colors = asset.GetPixels (0); 

				for (int i = 0; i < EmojiSize; i++) {
					for (int j = 0; j < EmojiSize; j++) {
						newTex.SetPixel (x + i, y + j, colors [i + j * EmojiSize]);
					}
				}
				Emoji emoji = new Emoji ();
				emoji.key = key;
				emoji.x = x;
				emoji.y = y;
				emoji.size = EmojiSize;
				emojiDic.Add (emoji);
				x += EmojiSize;
				if (x >= texSize.x) {
					x = 0;
					y += EmojiSize;
				}

			}
		}

		byte[] bytes1 = newTex.EncodeToPNG ();
		string outputfile1 = OutputPath + "emoji_tex.png";
		File.WriteAllBytes (outputfile1, bytes1);

		//byte[] bytes2 = dataTex.EncodeToPNG ();
		//string outputfile2 = OutputPath + "emoji_data.png";
		//File.WriteAllBytes (outputfile2, bytes2);

		//AssetImporter.GetAtPath("Assets/Resources/EmojiFile.asset").assetBundleName = "EmojiFile";
			
		FileStream file = File.Open(@"Assets/EmojiFileManager.cs", FileMode.Create); //初始化文件流
		StringBuilder sb = new StringBuilder ();
		string s = @"using System.Collections.Generic;
public class EmojiFileManager  {
	private static EmojiFileManager emojiFileMgr;
	public Dictionary<string,EmojiInfo> emojiInfos;
	public int size;
	public int count;
	public EmojiFileManager(){
		init ();
	}
	public static EmojiFileManager getInstance(){
		if (emojiFileMgr == null)
			emojiFileMgr = new EmojiFileManager ();
		return emojiFileMgr;
	}
	public void init(){
		size=1024;
		count=845;
";
		sb.Append (s).Append("\t\tsize=").Append((int)texSize.x).Append(";\r\n");
		sb.Append("\t\tcount=").Append(emojiDic.Count).Append(";\r\n");
		sb.Append("\t\temojiInfos = new Dictionary<string,EmojiInfo> (count);\r\n");
		for (int i = 0; i < emojiDic.Count; i++) {
			sb.Append ("\t\temojiInfos.Add (\"").Append ( emojiDic[i].key).Append("\",new EmojiInfo (new char[]{");
			int j = 0;
			while (true) {
				sb.Append (@"'\u").Append ( emojiDic[i].key.Substring (j, 4)).Append("'");
				j += 4;
				if(j <  emojiDic[i].key.Length){
					sb.Append(",");
				}else{
					break;
				}
			}
			sb.Append ("},\"").Append ( emojiDic[i].key).Append("\",").Append(emojiDic[i].x).Append(",")
				.Append(emojiDic[i].y).Append(",").Append(emojiDic[i].size).Append("));\r\n");
			//versionFile.emojiInfos.Add (emojiInfo);
		}
		sb.Append ("\t}\r\n}");
		byte[] data = Encoding.Default.GetBytes(sb.ToString());
		file.Write(data, 0, data.Length);  //向myStream 里写入数据
		file.Flush();  //刷新流中的数据
		file.Close();
		//File.Copy (OutputPath + "emoji.txt","Assets/Resources/emoji.txt",true);
		//AssetDatabase.SaveAssets();
		//AssetDatabase.Refresh ();
		EditorUtility.DisplayDialog ("Success", "Generate Emoji Successful!", "OK");
	}

	private static Vector2 ComputeAtlasSize(int count)
	{
		long total = count * EmojiSize * EmojiSize;
		for (int i = 0; i < AtlasSize.Length; i++) {
			if (total <= AtlasSize [i].x * AtlasSize [i].y) {
				return AtlasSize [i];
			}
		}
		return Vector2.zero;
	}

	private static void FormatTexture() {
		TextureImporter emojiTex = AssetImporter.GetAtPath (OutputPath + "emoji_tex.png") as TextureImporter;
		emojiTex.filterMode = FilterMode.Point;
		emojiTex.mipmapEnabled = false;
		emojiTex.SaveAndReimport ();
	}
}
