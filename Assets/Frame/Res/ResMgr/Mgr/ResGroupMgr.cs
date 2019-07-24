using System;
using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		public static bool isGroupSyncState = false;
		private static bool preIsGroupSyncState = false;

		public static BundleGroup LoadBundleGroup(string[] assetPaths, string[] bundleNames = null)
		{
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				Debug.LogError("[ResMgr] 不能在 非编辑器模式下 并且 非ab包模式下 使用该接口 LoadBundleGroup 可使用ResMgr.IsAssetBundleMode判断模式状态");
				return null;
			}
#endif
			preIsGroupSyncState = isGroupSyncState;
			isGroupSyncState = true;
			var group = new BundleGroup(assetPaths, bundleNames);
			group.Load();
			isGroupSyncState = preIsGroupSyncState;
			return group;
		}

		public static void LoadBundleGroupAsync(string[] assetPaths, string[] bundleNames = null, Action<float> progressing = null, Action<BundleGroup> completed = null)
		{
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				Debug.LogError("[ResMgr] 不能在 非编辑器模式下 并且 非ab包模式下 使用该接口 LoadBundleGroupAsync 可使用ResMgr.IsAssetBundleMode判断模式状态");
				return;
			}
#endif
			var group = new BundleGroupAsync(assetPaths, bundleNames, progressing, completed) as BundleGroupAsync;
			group.AddToAsyncQueue();
		}

		public static AssetGroup LoadAssetGroup(string[] assetPaths, string[] bundleNames = null)
		{
			preIsGroupSyncState = isGroupSyncState;
			isGroupSyncState = true;
			var group = new AssetGroup(assetPaths, bundleNames);
			group.Load();
			isGroupSyncState = preIsGroupSyncState;
			return group;
		}

		public static void LoadAssetGroupAsync(string[] assetPaths, string[] bundleNames = null, Action<float> progressing = null, Action<AssetGroup> completed = null)
		{
			AssetGroup group;
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				group = LoadAssetGroup(assetPaths, bundleNames);
				completed(group);
				return;
			}
#endif
			group = new AssetGroupAsync(assetPaths, bundleNames, progressing, completed) as AssetGroupAsync;
			group.AddToAsyncQueue();
		}

		public static AllAssetGroup LoadAllAssetGroup(string[] assetPaths)
		{
			AllAssetGroup group;
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				Debug.LogError("[ResMgr] 不能在 非编辑器模式下 并且 非ab包模式下 使用该接口 LoadAllAssetGroup 可使用ResMgr.IsAssetBundleMode判断模式状态");
				return null;
			}
#endif
			group = new AllAssetGroup(assetPaths);
			group.Load();
			return group;
		}

		public static void LoadAllAssetGroupAsync(string[] assetPaths, Action<float> progressing = null, Action<AllAssetGroup> completed = null)
		{
			AllAssetGroup group;
#if UNITY_EDITOR
			if (!ResMgr.IsAssetBundleMode)
			{
				Debug.LogError("[ResMgr] 不能在 非编辑器模式下 并且 非ab包模式下 使用该接口 LoadAllAssetGroupAsync 可使用ResMgr.IsAssetBundleMode判断模式状态");
				return;
			}
#endif
			group = new AllAssetGroupAsync(assetPaths, progressing, completed) as AllAssetGroupAsync;
			group.AddToAsyncQueue();
		}
	}
}