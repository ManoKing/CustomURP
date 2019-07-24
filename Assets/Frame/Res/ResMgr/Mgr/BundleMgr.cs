using System;
using System.IO;
using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		#region 为了效率硬编码一波

		public static bool LoadManifestBundle()
		{
			var bundle = new Bundle(ResMgr.PlatformName);
			bundle.Load();
			bundle.Retain();
			if (bundle.loadState != LoadState.LoadSuccess)
			{
				Debug.LogError("Manifest bundle error ");
				return false;
			}
			AbManifest = bundle.AssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			if (AbManifest == null) return false;
			Bundle bundle_manifestTxt = LoadBundle("manifest", false);
			if (bundle_manifestTxt.loadState != LoadState.LoadSuccess) return false;
			TextAsset manifest = bundle_manifestTxt.AssetBundle.LoadAsset<TextAsset>("manifest.txt");
			using (var reader = new StringReader(manifest.text))
			{
				ResMgr.ManifestTxt.Load(reader);
				reader.Close();
			}
			bundle_manifestTxt.Release();
			Resources.UnloadAsset(manifest);
			manifest = null;
			return true;
		}

		public static void LoadManifestBundleAsync(System.Action<bool> compeleted)
		{
			var bundle = new BundleAsync(ResMgr.PlatformName, (resObj) =>
			{
				var b = resObj as BundleAsync;
				if (b.loadState != LoadState.LoadSuccess) compeleted(false);
				var abCreateReq = b.AssetBundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
				abCreateReq.completed += (_abCReq) =>
				{
					if (abCreateReq.asset == null)
					{
						compeleted(false);
					}
					else
					{
						AbManifest = abCreateReq.asset as AssetBundleManifest;
						if (AbManifest != null)
						{
							compeleted(false);
						}
						else
						{
							LoadAssetAsync("manifest", (_resObj) =>
							{
								if (b.loadState != LoadState.LoadSuccess)
								{
									compeleted(false);
								}
								else
								{
									if (_resObj.UObj != null)
									{
										var textAsset = _resObj.UObj as TextAsset;
										if (textAsset == null) compeleted(false);
										using (var reader = new StringReader(textAsset.text))
										{
											ResMgr.ManifestTxt.Load(reader);
											reader.Close();
										}
										_resObj.Release();
										Resources.UnloadAsset(textAsset);
										textAsset = null;
										compeleted(true);
									}
								}
							});
						}
					}
				};
			});
			bundle.Load();
			bundle.Retain();
		}

		private static Bundle _LoadBundle(string bundleName)
		{
			Bundle bundle = ResObj.TryGet<Bundle>(bundleName);
			if (bundle == null)
			{
				bundle = new Bundle(bundleName);
				bundle.Load();
				bundle.LoadDeps(false);
			}
			if (!bundle.IsDone)
			{
				Debug.LogWarning("资源正处于异步加载中, 又立刻调用同步加载接口 将把该资源的加载模式转为同步加载  资源id：bundle: " + bundle.id);
				(bundle as BundleAsync).CancelAsyncLoadingAndReLoadSync();
			}
			bundle.Retain();
			return bundle;
		}

		private static Bundle _LoadBundleAsync(string bundleName, Action<ResObj> completed)
		{
			Bundle bundle = ResObj.TryGet<Bundle>(bundleName);
			if (bundle == null)
			{
				bundle = new BundleAsync(bundleName, completed) as Bundle;
				bundle.AddToAsyncQueue();
				bundle.LoadDeps(true);
			}
			else
			{
				if (bundle.IsDone)
				{
					if (completed != null)
					{
						completed.Invoke(bundle);
						completed = null;
					}
				}
				else
				{
					(bundle as BundleAsync).AddCompleted(completed);
				}
			}
			bundle.Retain();
			return bundle;
		}

		#endregion 为了效率硬编码一波

		public static Bundle LoadBundle(string bundleName, bool isCheck = true)
		{
			if (isCheck) { if (!ResMgr.IsContainBundle(bundleName)) return null; }
			return _LoadBundle(bundleName);
		}

		public static Bundle LoadBundleAsync(string bundleName, System.Action<ResObj> completed = null, bool isCheck = true)
		{
			if (isCheck) { if (!ResMgr.IsContainBundle(bundleName)) return null; }
			return _LoadBundleAsync(bundleName, completed);
		}

		public static Bundle TryGetBundleByAssetPath(string assetPath)
		{
			return ResObj.TryGet<Bundle>(ResMgr.GetBundleNameByAssetPath(assetPath));
		}
	}
}