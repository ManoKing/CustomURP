using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

using System.Text.RegularExpressions;

#endif

namespace DH.Frame.Res
{
	public class ManifestTxt
	{
		private static readonly Dictionary<string, string> abmap = new Dictionary<string, string>();

		private static readonly Dictionary<string, List<string>> ba_map_list = new Dictionary<string, List<string>>();

		private static readonly Dictionary<string, Dictionary<string, string>> ba_map_map_bundleName2assetName2assetPath = new Dictionary<string, Dictionary<string, string>>();

#if UNITY_EDITOR

		public string[] allAssets { get; private set; }

		public string[] allBundles { get; private set; }

#endif

		public void Load(TextReader reader)
		{
			Clear();
			List<string> bundleList = new List<string>();
			List<string> assetList = new List<string>();

			string bundleName = null;
			string line = null;
			while ((line = reader.ReadLine()) != null)
			{
				if (line == string.Empty)
				{
					continue;
				}
				var fields = line.Split(':');
				if (fields.Length > 1)
				{
					bundleName = fields[0];
					bundleList.Add(bundleName);
					ba_map_list.Add(bundleName, new List<string>());
					ba_map_map_bundleName2assetName2assetPath.Add(bundleName, new Dictionary<string, string>());
				}
				else
				{
					string assetPath = line.TrimStart('\t');
					if (assetPath == null)
					{
						UnityEngine.Debug.LogError("Manifest error assetPath: " + assetPath);
					}
					assetList.Add(assetPath);
					abmap[assetPath] = bundleName;
					ba_map_list[bundleName].Add(assetPath);
					var assetName = Path.GetFileNameWithoutExtension(assetPath);
					string _assetPath = string.Empty;
					//Dictionary<string, string> a = new Dictionary<string, string>();
					if (ba_map_map_bundleName2assetName2assetPath[bundleName].ContainsKey(assetName))
					{
						if (ba_map_map_bundleName2assetName2assetPath[bundleName][assetName].CompareTo(assetPath) == 0)
						{
							UnityEngine.Debug.LogError(" 存在同名的子资源\n" + assetName + "\n" + ba_map_map_bundleName2assetName2assetPath[bundleName][assetName] + "\n" + assetPath);
						}
						else
						{
							if (assetPath.EndsWith("prefab"))
							{
								ba_map_map_bundleName2assetName2assetPath[bundleName][assetName] = assetPath;
							}

						}
					}
					else
					{
						ba_map_map_bundleName2assetName2assetPath[bundleName].Add(assetName, assetPath);
					}
				}
			}
#if UNITY_EDITOR
			allBundles = bundleList.ToArray();
			allAssets = assetList.ToArray();
#endif
		}

		public bool IsContainsBundle(string bundleName)
		{
			bundleName = bundleName.ToLower();
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				return false;
			}
#endif

			var isExist = ba_map_list.ContainsKey(bundleName);
#if UNITY_EDITOR
			if (!isExist)
			{
				UnityEngine.Debug.LogError("[ResMgr] 不存在资源 id: " + bundleName);
			}
#endif
			return isExist;
		}

		public bool IsContainsAsset(string assetPath)
		{
			assetPath = assetPath.ToLower();
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				return true;
			}
#endif
			var isExist = abmap.ContainsKey(assetPath);
#if UNITY_EDITOR
			if (!isExist)
			{
				UnityEngine.Debug.LogError("[ResMgr] 不存在资源 id: " + assetPath);
			}
#endif
			return isExist;
		}

		public string GetBundleName(string assetPath)
		{
			string bundleName = string.Empty;
			assetPath = assetPath.ToLower();
			abmap.TryGetValue(assetPath, out bundleName);
#if UNITY_EDITOR
			if (ResMgr.IsAssetBundleMode)
			{
				if (bundleName == null)
				{
					UnityEngine.Debug.LogError("[ResMgr] 不存在对应资源 id: bundle " + bundleName + "assetPath: " + assetPath);
				}
			}
#endif
			return bundleName;
		}

		public string[] GetAllAssetListPathByBundleName(string bundleName)
		{
			if (ba_map_list.ContainsKey(bundleName))
			{
				return ba_map_list[bundleName].ToArray();
			}
			return null;
		}

		public Dictionary<string, string> GetAllAssetInfoInBundle(string bundleName)
		{
			if (ba_map_map_bundleName2assetName2assetPath.ContainsKey(bundleName))
			{
				return ba_map_map_bundleName2assetName2assetPath[bundleName];
			}
			return null;
		}

		private void Clear()
		{
			abmap.Clear();
			ba_map_list.Clear();
			ba_map_map_bundleName2assetName2assetPath.Clear();
#if UNITY_EDITOR
			allAssets = new string[0];
			allBundles = new string[0];
#endif
		}

#if UNITY_EDITOR

		private static bool ContainChinese(string input)
		{
			string pattern = "[\u4e00-\u9fbb]";
			return Regex.IsMatch(input, pattern);
		}

		public bool IsMainifextTxtLegal()
		{
			Dictionary<string, string> checkDic = new Dictionary<string, string>();
			bool isLegal = true;
			foreach (var item in ba_map_list)
			{
				var bundleName = item.Key;
				var assetPaths = item.Value;

				if (bundleName.Contains(" ") || ContainChinese(bundleName))
				{
					UnityEngine.Debug.LogError(" manifest资源检测非法 bundleName: " + bundleName + "bundleName 不能包括空格 中文等");
				}
				checkDic.Clear();

				foreach (var assetPath in assetPaths)
				{
					var name1 = Path.GetFileName(assetPath);
					var name2 = Path.GetFileNameWithoutExtension(assetPath);

					if (checkDic.ContainsKey(name1))
					{
						if (assetPath.CompareTo(checkDic[name1]) == 0)
						{
							if (assetPath.EndsWith("prefab"))
							{
								UnityEngine.Debug.LogError(" manifest资源检测非法同名AssetName\n bundleName: " + bundleName + " \nassetPath1： " + checkDic[name1] + "和 \nassetPath2：" + assetPath + " 同名!");
								isLegal = false;
							}
							else
							{
								UnityEngine.Debug.LogWarning(" manifest资源检测非法同名AssetName\n bundleName: " + bundleName + " \nassetPath1： " + checkDic[name1] + "和 \nassetPath2：" + assetPath + " 同名!");
							}

						}
					}
					if (checkDic.ContainsKey(name2))
					{
						if (assetPath.CompareTo(checkDic[name2]) == 0)
						{
							if (assetPath.EndsWith("prefab"))
							{
								UnityEngine.Debug.LogError(" manifest资源检测非法同名AssetName\n bundleName: " + bundleName + " \nassetPath1： " + checkDic[name2] + "和 \nassetPath2：" + assetPath + " 同名!");
								isLegal = false;
							}
							else
							{
								UnityEngine.Debug.LogWarning(" manifest资源检测非法同名AssetName\n bundleName: " + bundleName + " \nassetPath1： " + checkDic[name2] + "和 \nassetPath2：" + assetPath + " 同名!");
							}
						}
					}
					else
					{
						checkDic.Add(name1, assetPath);
						if (!checkDic.ContainsKey(name2))
							checkDic.Add(name2, assetPath);
					}
				}
			}
			return isLegal;
		}

#endif
	}
}