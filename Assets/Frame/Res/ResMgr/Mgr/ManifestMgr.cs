using System.Collections.Generic;
using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		public static ManifestTxt ManifestTxt = new ManifestTxt();
		public static AssetBundleManifest AbManifest = null;

		private static bool IsInit = false;

		#region 初始化

		public static bool InitAllManifest()
		{
			return LoadManifestBundle();
		}

		public static void InitAllManifestAsync(System.Action<bool> compeleted)
		{
			LoadManifestBundleAsync(compeleted);
		}

		#endregion 初始化

		#region 检测资源路径

		public static bool IsContainAssetPath(string assetPath)
		{
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				return ManifestTxt.IsContainsAsset(assetPath);
			}
			return true;
#endif
			return ManifestTxt.IsContainsAsset(assetPath);
		}

		public static bool IsContainBundle(string bundleName)
		{
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				return ManifestTxt.IsContainsBundle(bundleName);
			}
			return true;
#endif
			return ManifestTxt.IsContainsBundle(bundleName);
		}

		public static string GetBundleNameByAssetPath(string assetPath)
		{
			if (IsContainAssetPath(assetPath))
			{
				return ManifestTxt.GetBundleName(assetPath);
			}
			return null;
		}

		public static string[] GetBundleNamesByAssetPaths(string[] assetPaths)
		{
			var len = assetPaths.Length;
			var dic = GetFromTempDic();
			var list = GetFromTempList();
			for (int i = 0; i < len; i++)
			{
				var bundleName = GetBundleNameByAssetPath(assetPaths[i]);
				if (!dic.ContainsKey(bundleName))
				{
					dic.Add(bundleName, true);
					list.Add(bundleName);
				}
			}
			var bundleNames = list.ToArray();
			ReturnToTempDic(dic);
			ReturnToTempList(list);
			return bundleNames;
		}

		private static Queue<List<string>> inner_list_pools = new Queue<List<string>>();
		private static Queue<Dictionary<string, bool>> inner_dic_pools = new Queue<Dictionary<string, bool>>();

		#region 内部临时变量的队列 对象池

		private static List<string> GetFromTempList()
		{
			if (inner_list_pools.Count == 0)
			{
				inner_list_pools.Enqueue(new List<string>());
			}
			var list = inner_list_pools.Dequeue();
			list.Clear();
			return list;
		}

		private static void ReturnToTempList(List<string> obj)
		{
			inner_list_pools.Enqueue(obj);
		}

		private static Dictionary<string, bool> GetFromTempDic()
		{
			if (inner_dic_pools.Count == 0)
			{
				inner_dic_pools.Enqueue(new Dictionary<string, bool>());
			}
			var list = inner_dic_pools.Dequeue(); ;
			list.Clear();
			return list;
		}

		private static void ReturnToTempDic(Dictionary<string, bool> obj)
		{
			inner_dic_pools.Enqueue(obj);
		}

		#endregion 内部临时变量的队列 对象池

		public static string[] GetFilterBundles(string[] assetPaths, string[] bundleNames)
		{
			List<string> b_list = GetFromTempList();
			Dictionary<string, bool> b_dic = GetFromTempDic();
			if (bundleNames != null)
			{
				b_list.AddRange(bundleNames);
				for (int i = 0; i < bundleNames.Length; i++)
				{
					if (!string.IsNullOrEmpty(bundleNames[i]))
					{
						b_dic.Add(bundleNames[i], true);
					}
				}
			}

			for (int i = 0; i < assetPaths.Length; i++)
			{
				var abNames = AbManifest.GetAllDependencies(ManifestTxt.GetBundleName(assetPaths[i]));
				for (int j = 0; j < abNames.Length; j++)
				{
					if (!b_dic.ContainsKey(abNames[i]))
					{
						if (!string.IsNullOrEmpty(abNames[i]))
						{
							b_dic.Add(abNames[i], true);
							b_list.Add(abNames[i]);
						}
					}
				}
			}
			var array = b_list.ToArray();
			ReturnToTempDic(b_dic);
			ReturnToTempList(b_list);
			return array;
		}

		#endregion 检测资源路径
	}
}