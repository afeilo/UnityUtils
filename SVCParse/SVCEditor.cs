using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;

public class SVCEditor : Editor
{
    private ShaderVariantCollection svc;
    readonly public static string ALL_SHADER_VARAINT_DIR = "Assets";
    readonly public static string Tools_SVC_PATH = "Assets/Tools.shadervariants";

    #region FindMaterial

    static List<string> allShaderPathList = new List<string>();
    private static Dictionary<string, List<ShaderInfo>> SvcDic = new Dictionary<string, List<ShaderInfo>>();

    [MenuItem("Tools/解析所有ShaderVariant")]
    public static void ParseShaderVariant()
    {
        allShaderPathList.Clear();
        //先搜集所有keyword到工具类SVC
        toolSVC = new ShaderVariantCollection();
        var shaders = GetAllShaders();
        foreach (var shader in shaders)
        {
            ShaderVariantCollection.ShaderVariant sv = new ShaderVariantCollection.ShaderVariant();
            var shaderPath = AssetDatabase.GUIDToAssetPath(shader);
            sv.shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            toolSVC.Add(sv);
            allShaderPathList.Add(shaderPath);
        }
        
        //放空
        // FileHelper.WriteAllText(Tools_SVC_PATH, "");
        AssetDatabase.DeleteAsset(Tools_SVC_PATH);
        AssetDatabase.CreateAsset(toolSVC, Tools_SVC_PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        var toolText = File.ReadAllText(Tools_SVC_PATH);
        var lines = File.ReadAllLines(Tools_SVC_PATH);
        List<string> newLines = new List<string>();
        foreach (var line in lines)
        {
            if (line.Contains("keywords") || line.Contains("passType"))
                continue;
            if (line.Contains("variants:"))
            {
                newLines.Add(line.Replace("variants:", "variants: []"));
                continue;
            }
            newLines.Add(line);
        }
        File.WriteAllLines(Tools_SVC_PATH, newLines);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SvcDic.Clear();
        ShaderDataDict.Clear();
        ShaderVariantDict.Clear();
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Log.txt");
        int offset = 0;
        var shaderKey = "Compiled shader: ";
        var keywordsKey = "keywords ";
        var nokeywordsKey = "<no keywords>";
        while (true)
        {
            var index = textAsset.text.IndexOf(shaderKey, offset);
            if (index < 0)
                break;
            var shaderStartIndex = index + shaderKey.Length;
            var shaderEndIndex = textAsset.text.IndexOf(",", index);
            var keywordsStartIndex = textAsset.text.IndexOf(keywordsKey, index) + keywordsKey.Length;
            var keywordsEndIndex = textAsset.text.IndexOf("\r\n", index);
            if (keywordsEndIndex < 0)
                keywordsEndIndex = textAsset.text.Length;
            var shaderName = textAsset.text.Substring(shaderStartIndex, shaderEndIndex - shaderStartIndex);
            var keywords = textAsset.text.Substring(keywordsStartIndex, keywordsEndIndex - keywordsStartIndex);
            offset = keywordsEndIndex;

#region create svc

            string[] shaderKeywords = null;
            if (nokeywordsKey.Equals(keywords))
            {
                keywords = "";
            }
            else
            {
                shaderKeywords = keywords.Split(' ');
            }
            var shader = Shader.Find(shaderName);
            if (null == shader)
            {
                Debug.LogError("can't find shader " + shaderName);
                continue;
            }
            var path = AssetDatabase.GetAssetPath(shader);
            if (!allShaderPathList.Contains(path))
            {
                Debug.LogError("不存在shader:" + shader.name);
                continue;
            }

            if (!SvcDic.ContainsKey(shader.name))
            {
                GetShaderVariantEntriesFiltered(shader);
            }

            List<ShaderVariantCollection.ShaderVariant> svlist = null;
            if (!ShaderVariantDict.TryGetValue(shader.name, out svlist))
            {
                svlist = new List<ShaderVariantCollection.ShaderVariant>();
                ShaderVariantDict[shader.name] = svlist;
            }
            List<ShaderInfo> infos;
            if (SvcDic.TryGetValue(shader.name, out infos))
            {
                int _index = -1;
                for (int i = 0; i < infos.Count; i++)
                {
                    var pt = (PassType) infos[i].PassType;
                    if (infos[i].KeyWords.Equals(keywords))
                    {
                        ShaderVariantCollection.ShaderVariant? sv = null;
                        try
                        {
                            if (shaderKeywords != null)
                            {
                                //变体交集 大于0 ，添加到 svcList
                                sv = new ShaderVariantCollection.ShaderVariant(shader, pt, shaderKeywords);
                            }
                            else
                            {
                                sv = new ShaderVariantCollection.ShaderVariant(shader, pt);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogErrorFormat("{0}-当前shader不存在变体:{1}",shader.name,pt, shaderKeywords);
                            continue;
                        }
                        
                        //判断sv 是否存在,不存在则添加
                        if (sv != null)
                        {
                            bool isContain = false;
                            var _sv = (ShaderVariantCollection.ShaderVariant) sv;
                            foreach (var val in svlist)
                            {
                                if (val.passType == _sv.passType &&
                                    System.Linq.Enumerable.SequenceEqual(val.keywords, _sv.keywords))
                                {
                                    isContain = true;
                                    break;
                                }
                            }

                            if (!isContain)
                            {
                                svlist.Add(_sv);
                            }
                        }
                        _index = i;
                    }   
                }

                if (-1 == _index)
                {
                    Debug.LogErrorFormat("{0} can't match keywards: {1}",shader.name, keywords);
                    continue;
                }
            }
            else
            {
                Debug.LogErrorFormat("can't find {0} keywards", shader.name);
            }
#endregion
        }
        
        //所有的svc
        Dictionary<string, ShaderVariantCollection> svcDic = new Dictionary<string, ShaderVariantCollection>();
        foreach (var item in ShaderVariantDict)
        {
            var shader = Shader.Find(item.Key);
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shader));
            var setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            var ae = setting.FindAssetEntry(guid);
            if (null == ae)
            {
                Debug.LogErrorFormat("can't find group {0}", item.Key);
                continue;
            }
            ShaderVariantCollection svc;
            if (!svcDic.TryGetValue(ae.parentGroup.name, out svc))
            {
                svc = new ShaderVariantCollection();
                svcDic.Add(ae.parentGroup.name, svc);
            }
            foreach (var _sv in item.Value)
            {
                svc.Add(_sv);
            }
        }


        foreach (var item in svcDic)
        {
            var path = Path.Combine(ALL_SHADER_VARAINT_DIR, "svc_" + item.Key + ".shadervariants");
            
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(item.Value, path);
        }

        // AssetDatabase.DeleteAsset(Tools_SVC_PATH);
        AssetDatabase.Refresh();
    }
    
    public class ShaderInfo
    {
        public int PassType;
        public string KeyWords;
    }
    

    //shader数据的缓存
    static Dictionary<string, ShaderData> ShaderDataDict = new Dictionary<string, ShaderData>();

    static Dictionary<string, List<ShaderVariantCollection.ShaderVariant>> ShaderVariantDict =
        new Dictionary<string, List<ShaderVariantCollection.ShaderVariant>>();

    //添加Material计算
    static List<string> passShaderList = new List<string>();

    static MethodInfo GetShaderVariantEntries = null;

    static ShaderVariantCollection toolSVC = null;

    /// <summary>
    /// 获取keyword
    /// </summary>
    /// <param name="shader"></param>
    static void GetShaderVariantEntriesFiltered(Shader shader)
    {
        //2019.3接口
//            internal static void GetShaderVariantEntriesFiltered(
//                Shader                  shader,                     0
//                int                     maxEntries,                 1
//                string[]                filterKeywords,             2
//                ShaderVariantCollection excludeCollection,          3
//                out int[]               passTypes,                  4
//                out string[]            keywordLists,               5
//                out string[]            remainingKeywords)          6
        if (GetShaderVariantEntries == null)
        {
            GetShaderVariantEntries = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        if (toolSVC != null && !SvcDic.ContainsKey(shader.name))
        {
            var _passtypes = new int[] { };
            var _keywords = new string[] { };
            var _remainingKeywords = new string[] { };
            var filterKeywords = new string[] { };
            object[] args = new object[]
                {shader, 256, filterKeywords, toolSVC, _passtypes, _keywords, _remainingKeywords};
            GetShaderVariantEntries.Invoke(null, args);

            List<ShaderInfo> infos = new List<ShaderInfo>();
            var passtypes = args[4] as int[];
            var kws = args[5] as string[];
            // infos.Add(new ShaderInfo(){PassTypes = 0, KeyWords = ""});
            for (int i = 0; i < passtypes.Length; i++)
            {
                infos.Add(new ShaderInfo(){PassType = passtypes[i], KeyWords = kws[i]});
            }
            SvcDic.Add(shader.name, infos);
            // passTypes = passtypes;
            // //key word
            // keywordLists = new string[passtypes.Length][];
            // for (int i = 0; i < passtypes.Length; i++)
            // {
            //     keywordLists[i] = kws[i].Split(' ');
            // }
            //
            // //Remaning key word
            // var rnkws = args[6] as string[];
            // remainingKeywords = rnkws;
        }
    }

    #endregion


    #region AssetBundle

    /// <summary>
    /// 预制体路径
    /// </summary>
    /// <returns></returns>
    public static List<string> GetPrefabPath()
    {
        var directories = new List<string> {"Assets/Sources/Prefab"};
        return directories;
    }

    /// <summary>
    /// Sources 路径
    /// </summary>
    /// <returns></returns>
    public static List<string> GetSourcesPath()
    {
        var directories = new List<string> {"Assets"};
        return directories;
    }

    /// <summary>
    /// Sources/Prefab下所有prefab
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllPrefabs()
    {
        var paths = GetPrefabPath().ToArray();
        var assets = AssetDatabase.FindAssets("t:Prefab").ToList();
        return assets;
    }

    /// <summary>
    /// Sources下所有shader
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllShaders()
    {
        var paths = GetSourcesPath().ToArray();
        var shaders = AssetDatabase.FindAssets("t:Shader").ToList();
        return shaders;
    }

    /// <summary>
    /// Sources下所有meterial
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllMaterials()
    {
        var paths = GetSourcesPath().ToArray();
        var materials = AssetDatabase.FindAssets("t:Material", paths).ToList();
        return materials;
    }

    #endregion
}

static public class LApplication
{
    public const string CommonABName = "common";
}