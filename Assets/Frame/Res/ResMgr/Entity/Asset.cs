using System;
using System.IO;
using UnityEngine;

namespace DH.Frame.Res
{
	public class Asset : ResObj
	{
		protected static Type assetType = typeof(UnityEngine.Object);

		public Asset(string id) : base(id) { }

		internal override void Load()
		{
#if UNITY_EDITOR
			UObj = UnityEditor.AssetDatabase.LoadAssetAtPath(id, assetType);
			IsDone = true;
			loadState = LoadState.LoadSuccess;
			AddToLoaded();
#endif
		}
	}

	public class BundleAsset : Asset
	{
		public Bundle bundle = null;

		public BundleAsset(string id) : base(id)
		{
		}

		internal override void Load()
		{
			bundle = ResMgr.LoadBundle(ResMgr.GetBundleNameByAssetPath(id));
			if (bundle != null && bundle.AssetBundle.isStreamedSceneAssetBundle)
			{
				loadState = LoadState.LoadSuccess;
				AddToLoaded();
				return;
			}

			UObj = bundle.AssetBundle.LoadAsset(Path.GetFileName(id), assetType);
			loadState = LoadState.LoadSuccess;
			AddToLoaded();
			IsDone = true;
		}

		internal override void Unload()
		{
			if (bundle != null) { bundle.Unload(); }
			bundle = null;
			base.Unload();
		}
	}

	public class BundleAssetAsync : Asset, IResObjAsync
	{
		public BundleAsync bundleAsync = null;
		public Bundle bundle = null;
		private AssetBundleRequest _abReq = null;
		private Action<AsyncOperation> asyncOp = null;

		public BundleAssetAsync(string id, Action<ResObj> completed = null) : base(id)
		{
			_completed += completed;
		}

		internal override void Load()
		{
			bundle = ResMgr.LoadBundleAsync(ResMgr.GetBundleNameByAssetPath(id));
			bundleAsync = bundle as BundleAsync;
			if (bundleAsync == null && bundle.IsDone)
			{
				loadState = bundle.loadState;
				UObj = bundle.AssetBundle.LoadAsset(Path.GetFileName(id), assetType);
				Debug.LogWarning("特殊操作 将异步bundle改为同步bundle id: " + id);
				return;
			}

			bundle = null;
			if (bundleAsync == null)
			{
				Debug.LogError("BundleAssetAsync Load: BundleAync load fail, assetPath " + id);
				Debug.LogError("BundleAssetAsync Load: BundleAync load fail, bundleName " + ResMgr.GetBundleNameByAssetPath(id));
			}

			loadState = LoadState.LoadingBundle;
		}

		internal override void Unload()
		{
			if (bundleAsync != null) { bundleAsync.Unload(); }
			bundleAsync = null;
			if (bundle != null) { bundle.Unload(); }
			bundle = null;

			_abReq = null;
			loadState = LoadState.None;
		}

		public override bool IsDone
		{
			get
			{
				if (loadState == LoadState.LoadSuccess || loadState == LoadState.LoadFail)
				{
					return true;
				}
				else if (loadState == LoadState.None)
				{
					return false;
				}

				if (IsError || bundleAsync == null)
				{
					loadState = LoadState.LoadFail;
					return true;
				}
				if (bundleAsync.IsError || bundleAsync.IsDepsError)
				{
					loadState = LoadState.LoadFail;
					return true;
				}

				switch (loadState)
				{
					case LoadState.LoadingBundle:
						{
							if (!bundleAsync.IsDone || !bundleAsync.IsDepsDone)
							{
								return false;
							}
							if (bundleAsync.AssetBundle == null)
							{
								error = "assetbundle == null";
								return true;
							}
							_abReq = bundleAsync.AssetBundle.LoadAssetAsync(Path.GetFileName(id), assetType);
							if (_abReq == null)
							{
								error = "_abReq == null";
								return true;
							}

							if (asyncOp == null)
							{
								asyncOp = (_a) =>
								{
									loadState = LoadState.LoadingAsset;
								};
							}
							_abReq.completed += asyncOp;
							return false;
						}
					case LoadState.LoadingAsset:
						{
							if (_abReq.isDone)
							{
								UObj = _abReq.asset;
								loadState = LoadState.LoadSuccess;
								if (_completed != null) { _completed.Invoke(this); }
								_completed = null;
								return true;
							}
							return false;
						}
					default:
						return false;
				}
			}
		}

		#region 异步相关

		public float Progress
		{
			get
			{
				if (bundle != null)
				{
					return 1.0f;
				}
				if (bundleAsync == null)
				{
					return 0.0f;
				}
				var bundleProgress = bundleAsync.totalPorgress;
				float resProgress = 0.5f * bundleProgress + 0.5f * _abReq.progress;
				return resProgress;
			}
		}

		private Action<ResObj> _completed = null;

		#endregion 异步相关

		public void AddCompleted(Action<ResObj> completed)
		{
			if (IsDone)
			{
				completed(this);
			}
			else
			{
				_completed += completed;
			}
		}

		public void RemoveCompleted(Action<ResObj> completed)
		{
			_completed -= completed;
		}

		public void CancelAsyncLoadingAndReLoadSync()
		{
			RemoveFromLoading();

			if (loadState == LoadState.None)
			{
				RemoveFromToLoad();
				loadState = LoadState.LoadingBundle;
			}

			if (loadState == LoadState.LoadingBundle)
			{
				if (bundleAsync != null)
				{
					bundleAsync.CancelAsyncLoadingAndReLoadSync();
				}
				else
				{
				}
				loadState = LoadState.LoadingAsset;
			}

			if (loadState == LoadState.LoadingAsset)
			{
				if (_abReq != null)
				{
					_abReq.completed -= asyncOp;
					_abReq = null;
				}

				bundle = ResMgr.LoadBundle(ResMgr.GetBundleNameByAssetPath(id));
				UObj = bundle.AssetBundle.LoadAsset(Path.GetFileName(id), assetType);
				if (UObj == null)
				{
					loadState = LoadState.LoadFail;
				}
				else
				{
					loadState = LoadState.LoadSuccess;
					AddToLoaded();
				}
				IsDone = true;
			}
		}
	}
}