using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class Util : MonoBehaviour {

	// Use this for initialization
	void Start () {
		getSenestiveFile ();
		getSenestiveFile2 ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public static void getSenestiveFile(){
		
		StreamReader sr = File.OpenText(@"Assets\script\mingan.txt");
		FileStream fs = File.OpenWrite(@"Assets\script\SenestiveLib.cs");
		//sb.Append ();
		StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
		sw.Write (@"public class SenestiveLib{
	public static string[] regex = {");
		string line = null;
		bool first = true;
		Debug.Log (sr.ReadLine());
		while ((line = sr.ReadLine ()) != null) {
			StringBuilder sb = new StringBuilder ();
			if(!first)
				sb.Append ("\",");
			int index = line.IndexOf ("=");
			sb.Append("\"");
			if (index > 0) {
				sb.Append (line.Substring (0, index));
				sw.Write (sb.ToString ());
			}
			first = false;
		}
		sw.Write("\"};\r\n}");
		sw.Close();
		sr.Close ();
	}

	public static void getSenestiveFile2(){
		StreamReader sr = File.OpenText(@"Assets\script\mingan.txt");
		FileStream fs = File.OpenWrite(@"Assets\script\RegExpLib.cs");
		//sb.Append ();
		StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
		sw.Write ("public class RegExpLib{\r\n\tpublic static string regex = \"");
		string line = null;
		bool first = true;
		Debug.Log (sr.ReadLine());
		while ((line = sr.ReadLine ()) != null) {
			StringBuilder sb = new StringBuilder ();
			if(!first)
				sb.Append ("|");
			int index = line.IndexOf ("=");
			if (index > 0) {
				sb.Append (line.Substring (0, index));
				sw.Write (sb.ToString ());
			}
			first = false;
		}
		sw.Write("\";}");
		sw.Close();
		sr.Close ();
	}
}
