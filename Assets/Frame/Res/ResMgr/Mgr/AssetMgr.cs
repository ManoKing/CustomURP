using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		public static Asset LoadAsset(string assetPath, bool isCheck = true)
		{
			if (isCheck) { if (!ResMgr.IsContainAssetPath(assetPath)) { return null; } }
			return _LoadAsset(assetPath);
		}

		public static Asset LoadAssetAsync(string assetPath, System.Action<ResObj> completed = null, bool isCheck = true)
		{
			if (isCheck) { if (!ResMgr.IsContainAssetPath(assetPath)) { return null; } }
			return _LoadAssetAsync(assetPath, completed);
		}

		private static Asset _LoadAsset(string assetPath)
		{
			Asset asset = ResObj.TryGet<Asset>(assetPath);
			if (asset == null)
			{
				if (ResMgr.IsAssetBundleMode)
				{
					asset = new BundleAsset(assetPath) as Asset;
				}
				else
				{
					asset = new Asset(assetPath);
				}
				asset.Load();
			}
			if (!asset.IsDone)
			{
				Debug.LogWarning("资源正处于异步加载中, 又立刻调用同步加载接口 将把该资源的加载模式转为同步加载  资源id：assetPath: " + assetPath);
				(asset as BundleAssetAsync).CancelAsyncLoadingAndReLoadSync();
			}
			asset.Retain();
			return asset;
		}

		private static Asset _LoadAssetAsync(string assetPath, System.Action<ResObj> completed = null)
		{
			Asset asset = ResObj.TryGet<Asset>(assetPath);
			if (asset == null)
			{
				if (ResMgr.IsAssetBundleMode)
				{
					asset = new BundleAssetAsync(assetPath, completed);
					asset.AddToAsyncQueue();
				}
				else
				{
					asset = new Asset(assetPath);
					asset.Load();
					if (completed != null) { completed.Invoke(asset); }
				}
			}
			else
			{
				if (asset.IsDone)
				{
					if (completed != null) { completed.Invoke(asset); }
				}
				else
				{
					(asset as BundleAssetAsync).AddCompleted(completed);
				}
			}
			asset.Retain();
			return asset;
		}
	}
}