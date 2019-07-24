using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.Frame.Res
{
	public class Bundle : ResObj
	{
		#region 内部方法

		protected AssetBundle _assetBundle = null;

		protected List<Bundle> deps = new List<Bundle>();

		private static Func<string, string> _OverrideBundlePath = (assetBundleName) =>
		  {
			  var relativeStreamingPath = ResMgr.ABFolderName + "/" + assetBundleName;
			  return relativeStreamingPath;
		  };

		private string _ABFilePath = null;

		private bool _isDepsDone = false;

		private bool _isDepsError = false;

		protected string ABFilePath
		{
			get
			{
				if (_ABFilePath == null)
					_ABFilePath = OverrideBundlePath(id);
				return _ABFilePath;
			}
		}

		#endregion 内部方法

		#region 公共方法

		public Bundle(string id) : base(id)
		{
		}

		public static Func<string, string> OverrideBundlePath
		{
			get
			{
				return _OverrideBundlePath;
			}
			set
			{
				_OverrideBundlePath = value;
			}
		}

		public AssetBundle AssetBundle { get { return _assetBundle; } }

		internal bool IsDepsDone
		{
			get
			{
				if (_isDepsDone)
				{
					return true;
				}
				for (int i = 0, max = deps.Count; i < max; i++)
				{
					var item = deps[i];
					if (!item.IsDone)
					{
						return false;
					}
				}
				_isDepsDone = true;
				return true;
			}
		}

		internal bool IsDepsError
		{
			get
			{
				if (_isDepsError)
				{
					return true;
				}
				for (int i = 0, max = deps.Count; i < max; i++)
				{
					var item = deps[i];
					if (item.IsError)
					{
						_isDepsError = true;
						break;
					}
				}
				return _isDepsError;
			}
		}

		public void LoadDeps(bool isAsync = false)
		{
			var strs = ResMgr.AbManifest.GetAllDependencies(id);
			if (strs.Length > 0)
			{
				for (int i = 0; i < strs.Length; i++)
				{
					if (string.IsNullOrEmpty(strs[i])) continue;
					var b = isAsync ? ResMgr.LoadBundleAsync(strs[i], null, false) : ResMgr.LoadBundle(strs[i], false);
					deps.Add(b);
				}
			}
		}

		public void UnloadDeps()
		{
			for (int i = 0; i < deps.Count; i++)
			{
				deps[i].Release();
			}
			deps.Clear();
		}

		internal override void Load()
		{
			byte[] bytes = FileUtils.ReadFileSync(ABFilePath);
			if (bytes == null) return;
			UObj = AssetBundle.LoadFromMemory(bytes);
			_assetBundle = UObj as AssetBundle;
			if (UObj == null)
			{
				error = " AssetBundle Load Failed. bundleName: " + id + " filePath: " + ABFilePath;
				loadState = LoadState.LoadFail;
			}
			else
			{
				loadState = LoadState.LoadSuccess;
				AddToLoaded();
			}
			IsDone = true;
		}

		internal override void Unload()
		{
			if (_assetBundle != null) { _assetBundle.Unload(true); }
			_assetBundle = null;
			deps.Clear();
			base.Unload();
		}

		#endregion 公共方法
	}

	public class BundleAsync : Bundle, IResObjAsync
	{
		#region 内部

		public AssetBundleCreateRequest _abCreateReq;
		private Action _cancel = null;
		private Action<ResObj> _completed = null;

		#endregion 内部

		#region 外部

		internal BundleAsync(string id, Action<ResObj> completed = null) : base(id)
		{
			if (completed != null)
			{
				this._completed += completed;
			}
		}

		public override bool IsDone
		{
			get
			{
				if (loadState == LoadState.LoadSuccess || loadState == LoadState.LoadFail)
				{
					return true;
				}
				else if (loadState == LoadState.None || loadState == LoadState.ReadingFile)
				{
					return false;
				}

				if (_abCreateReq == null)
				{
					loadState = LoadState.LoadFail;
					return true;
				}

				if (IsError || IsDepsError)
				{
					loadState = LoadState.LoadFail;
					return true;
				}
				var _isDone = false;

				switch (loadState)
				{
					case LoadState.LoadingBundle:
						_isDone = _abCreateReq.isDone;
						if (_isDone)
						{
							loadState = LoadState.LoadSuccess;
						}
						break;
				}
				return _isDone;
			}
		}

		public float totalPorgress
		{
			get
			{
				float allBundleProgress = 0.0f;
				allBundleProgress = _abCreateReq.progress;
				if (deps.Count > 0)
				{
					for (int i = 0; i < deps.Count; i++)
					{
						if (deps[i].IsDone)
						{
							allBundleProgress += 1;
						}
						else
						{
							var ba = deps[i] as BundleAsync;
							if (ba != null)
							{
								allBundleProgress += ba.Progress;
							}
						}
					}
				}
				allBundleProgress = allBundleProgress / (deps.Count + 1);
				return allBundleProgress;
			}
		}

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

		internal override void Load()
		{
			loadState = LoadState.ReadingFile;
			_cancel = FileUtils.ReadFileAsync(ABFilePath, (bytes) =>
			{
				if (loadState == LoadState.LoadSuccess || loadState == LoadState.LoadFail)
				{
					Debug.LogWarning("资源已经变为同步加载 并且已经完成 此次回调为此前异步加载的读取回调 已经不需要了 不做处理 bundle id: " + id);
					return;
				}
				if (bytes == null) return;
				loadState = LoadState.LoadingBundle;
				_abCreateReq = AssetBundle.LoadFromMemoryAsync(bytes);
				if (_abCreateReq == null)
				{
					error = " AssetBundle Load Async Failed. bundleName: " + id + " filePath: " + ABFilePath;
				}
				else
				{
					_abCreateReq.completed += (_a) =>
					{
						_assetBundle = _abCreateReq.assetBundle;
						loadState = LoadState.LoadSuccess;
						if (_completed != null) { _completed.Invoke(this); }
						_completed = null;
					};
				}
			});
		}

		internal override void Unload()
		{
			if (_assetBundle != null) { _assetBundle.Unload(true); }
			_assetBundle = null;
			_abCreateReq = null;
		}

		#endregion 外部

		#region 异步相关

		public float Progress
		{
			get
			{
				if (_abCreateReq == null)
					return 0.0f;
				return _abCreateReq.progress;
			}
		}

		public bool CancelAsyncLoadingAndReLoadSync()
		{
			RemoveFromLoading();

			return CancelAsyncLoadingAndReLoadSync_Self() && CancelAsyncLoadingAndReLoadSync_Deps();
		}

		public bool CancelAsyncLoadingAndReLoadSync_Self()
		{
			if (_cancel != null)
			{
				_cancel.Invoke();
			}
			if (loadState == LoadState.None)
			{
				RemoveFromToLoad();
				loadState = LoadState.ReadingFile;
			}
			if (loadState == LoadState.ReadingFile)
			{
				loadState = LoadState.LoadingBundle;
			}
			if (loadState == LoadState.LoadingBundle)
			{
				bool isNeedLoadAB = false;
				if (_abCreateReq != null)
				{
					_abCreateReq.assetBundle.Unload(true);
					if (_abCreateReq.assetBundle == null)
					{
						isNeedLoadAB = true;
					}
					else
					{
						_assetBundle = _abCreateReq.assetBundle;
					}
				}
				else
				{
					isNeedLoadAB = true;
				}
				if (isNeedLoadAB)
				{
					byte[] bytes = FileUtils.ReadFileSync(ABFilePath);
					if (bytes != null)
					{
						UObj = AssetBundle.LoadFromMemory(bytes);
						_assetBundle = UObj as AssetBundle;
					}
				}
				if (_assetBundle != null)
				{
					if (ResMgr.TryGet<Bundle>(id) == null)
					{
						AddToLoaded();
					}
					loadState = LoadState.LoadSuccess;
					return true;
				}
				else
				{
					loadState = LoadState.LoadFail;
					return false;
				}
			}
			return false;
		}

		private bool CancelAsyncLoadingAndReLoadSync_Deps()
		{
			bool isExistFail = false;
			for (int i = 0; i < deps.Count; i++)
			{
				if (!deps[i].IsDone)
				{
					BundleAsync ba = deps[i] as BundleAsync;
					if (ba != null)
					{
						ba.CancelAsyncLoadingAndReLoadSync_Self();
						isExistFail = true;
					}
				}
			}
			return isExistFail;
		}

		#endregion 异步相关
	}
}