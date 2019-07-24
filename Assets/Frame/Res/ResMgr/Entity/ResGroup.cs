using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.Frame.Res
{
	public class ResGroupBase : ResObj
	{
		public static string GroupStr = "ResGroupBase_id_";

		protected static int GuidCounter = 0;

		protected string[] AssetPaths;

		public string[] BundleNames;

		protected Dictionary<string, ResObj> _Loaded_objs;

		protected Dictionary<string, UnityEngine.Object> dic_assetPath2UObj = new Dictionary<string, UnityEngine.Object>();

		public UnityEngine.Object GetUObj(string assetPath)
		{
			UnityEngine.Object obj;
			assetPath = assetPath.ToLower();
			dic_assetPath2UObj.TryGetValue(assetPath, out obj);
			return obj;
		}

		public Dictionary<string, UnityEngine.Object> GetUObjDic()
		{
			return dic_assetPath2UObj;
		}

		internal override void Unload()
		{
			foreach (var item in _Loaded_objs)
			{
				if (item.Value != null) { item.Value.Release(); }
			}
		}
	}

	public class BundleGroup : ResGroupBase
	{

		public BundleGroup(string[] assetPaths, string[] bundleNames = null)
		{
			this.BundleNames = ResMgr.GetFilterBundles(assetPaths, bundleNames);
			this._Loaded_objs = new Dictionary<string, ResObj>();

			this.AssetPaths = assetPaths;
			this.id = GroupStr + (GuidCounter++).ToString();
		}

		internal override void Load()
		{
			loadState = LoadState.LoadSuccess;
			for (int i = 0; i < BundleNames.Length; i++)
			{
				var obj = ResMgr.LoadBundle(BundleNames[i], false);
				if (obj.loadState != LoadState.LoadSuccess)
				{
					loadState = LoadState.LoadFail;
					Debug.LogError(" loadFail bundleName:" + obj.id);
				}
				else
				{
					_Loaded_objs.Add(obj.id, obj);
				}
			}
			IsDone = true;
		}
	}

	public class BundleGroupAsync : BundleGroup, IResObjAsync
	{
		public int totalCount;
		public int curDoneCount;

		public BundleGroupAsync(string[] assetPaths, string[] bundleNames = null, Action<float> progressing = null, Action<BundleGroup> completed = null) : base(assetPaths, bundleNames)
		{
			totalCount = 0;
			curDoneCount = 0;
			_progressing = progressing;
			_completed += completed;
		}

		internal override void Load()
		{
			loadState = LoadState.LoadingBundle;
			var len = BundleNames.Length;
			for (int i = 0; i < len; i++)
			{
				ResMgr.LoadBundleAsync(BundleNames[i], (obj) =>
				{
					curDoneCount++;
					if (obj.loadState != LoadState.LoadSuccess)
					{
						loadState = LoadState.LoadFail;
						Debug.LogError(" loadFail bundleName:" + obj.id);
					}
					else
					{
						_Loaded_objs.Add(obj.id, obj);
					}
				}, false);
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
				if (_progressing != null) { _progressing.Invoke(Progress); }
				bool _isDone = totalCount == curDoneCount;
				if (_isDone)
				{
					if (_completed != null) { _completed.Invoke(this); }
					_completed = null;
				}
				return _isDone;
			}
		}

		#region 异步相关

		public float Progress
		{
			get
			{
				return (float)curDoneCount / (float)totalCount;
			}
		}

		private Action<BundleGroup> _completed = null;

		private Action<float> _progressing = null;

		#endregion 异步相关
	}

	public class AssetGroup : ResGroupBase
	{
		public AssetGroup(string[] assetPaths, string[] bundleNames = null)
		{
#if UNITY_EDITOR
			if (ResMgr.IsAssetBundleMode)
			{
				this.BundleNames = ResMgr.GetFilterBundles(assetPaths, bundleNames);
			}
			else
			{
				this.BundleNames = new string[0];
			}
#else

			this.BundleNames = ResMgr.GetFilterBundles(assetPaths, bundleNames);
#endif
			this._Loaded_objs = new Dictionary<string, ResObj>();

			this.AssetPaths = assetPaths;
			this.id = GroupStr + (GuidCounter++).ToString();
		}

		internal override void Load()
		{
			loadState = LoadState.LoadSuccess;
			for (int i = 0; i < BundleNames.Length; i++)
			{
				var obj = ResMgr.LoadBundle(BundleNames[i], false);
				if (obj.loadState != LoadState.LoadSuccess)
				{
					loadState = LoadState.LoadFail;
					Debug.LogError(" loadFail bundleName:" + obj.id);
				}
				else
				{
					_Loaded_objs.Add(obj.id, obj);
				}
			}

			for (int i = 0; i < AssetPaths.Length; i++)
			{
				var obj = ResMgr.LoadAsset(AssetPaths[i], false);
				if (obj.loadState != LoadState.LoadSuccess)
				{
					loadState = LoadState.LoadFail;
					Debug.LogError(" assetPaths assetPaths:" + obj.id);
				}
				else
				{
#if UNITY_EDITOR
					if(!_Loaded_objs.ContainsKey(obj.id))
					{
						_Loaded_objs.Add(obj.id, obj);
						dic_assetPath2UObj.Add(obj.id, obj.UObj);
					}
					else
					{
						Debug.LogError("传入了相同的 assetPath, 必须在编辑器模式下解决该bug, 否则ab包模式下会报错 " + obj.id);
					}
#else
					_Loaded_objs.Add(obj.id, obj);
					dic_assetPath2UObj.Add(obj.id, obj.UObj);
#endif
				}
			}
			IsDone = true;
		}
	}

	public class AssetGroupAsync : AssetGroup, IResObjAsync
	{
		public int totalCount;
		public int curDoneCount;

		public AssetGroupAsync(string[] assetPaths, string[] bundleNames = null, Action<float> progressing = null, Action<AssetGroup> completed = null) : base(assetPaths, bundleNames)
		{
			totalCount = 0;
			curDoneCount = 0;
			_progressing = progressing;
			_completed += completed;
		}

		internal override void Load()
		{
			loadState = LoadState.LoadingBundle;
			var len = BundleNames.Length;
			for (int i = 0; i < len; i++)
			{
				ResMgr.LoadBundleAsync(BundleNames[i], (obj) =>
				{
					if (obj.loadState != LoadState.LoadSuccess)
					{
						loadState = LoadState.LoadFail;
						Debug.LogError(" loadFail bundleName:" + obj.id);
					}
					else
					{
						_Loaded_objs.Add(obj.id, obj);
					}
					curDoneCount++;
				}, false);
			}
			totalCount += len;

			len = AssetPaths.Length;
			for (int i = 0; i < len; i++)
			{
				ResMgr.LoadAssetAsync(AssetPaths[i], (obj) =>
				{
					if (obj.loadState != LoadState.LoadSuccess)
					{
						loadState = LoadState.LoadFail;
						Debug.LogError(" assetPaths assetPaths:" + obj.id);
					}
					else
					{
						_Loaded_objs.Add(obj.id, obj);
						dic_assetPath2UObj.Add(obj.id, obj.UObj);
					}
					curDoneCount++;
				}, false);
			}
			totalCount += len;
		}

		public override bool IsDone
		{
			get
			{
				if (loadState == LoadState.LoadSuccess || loadState == LoadState.LoadFail)
				{
					return true;
				}
				if (_progressing != null) { _progressing.Invoke(Progress); }
				bool _isDone = totalCount == curDoneCount;
				if (_isDone)
				{
					if (_completed != null) { _completed.Invoke(this); }
					_completed = null;
				}
				return _isDone;
			}
		}

#region 异步相关

		public float Progress
		{
			get
			{
				return (float)curDoneCount / (float)totalCount;
			}
		}

		private Action<AssetGroup> _completed = null;

		private Action<float> _progressing = null;

#endregion 异步相关
	}

	public class AllAssetGroup : ResGroupBase
	{
		public AllAssetGroup(string[] assetPaths)
		{
			this._Loaded_objs = new Dictionary<string, ResObj>();

			this.AssetPaths = assetPaths;
			this.BundleNames = ResMgr.GetBundleNamesByAssetPaths(assetPaths);

			this.id = GroupStr + (GuidCounter++).ToString();
		}

		internal override void Load()
		{
			Bundle bundle = null;
			var len = BundleNames.Length;
			for (int i = 0; i < len; i++)
			{
				bundle = ResMgr.LoadBundle(BundleNames[i], false);
				if (bundle.loadState != LoadState.LoadSuccess)
				{
					loadState = LoadState.LoadFail;
					Debug.LogError(" loadFail bundleName:" + bundle.id + " bundle state: " + bundle.loadState);
					return;
				}
				else
				{
					_Loaded_objs.Add(bundle.id, bundle);

					if (bundle.AssetBundle.isStreamedSceneAssetBundle)
					{
						continue;
					}

					var dic_assetName2assetPathInBundle = ResMgr.ManifestTxt.GetAllAssetInfoInBundle(bundle.id);

					if (dic_assetName2assetPathInBundle == null)
					{
						Debug.LogError(" 获取包里资源列表失败 包id " + bundle.id);
						continue;
					}

					var all_assetObject = bundle.AssetBundle.LoadAllAssets();
					for (int j = 0; j < all_assetObject.Length; j++)
					{
						var assetName = all_assetObject[j].name.ToLower();
						if (dic_assetName2assetPathInBundle.ContainsKey(assetName))
						{
							dic_assetPath2UObj[dic_assetName2assetPathInBundle[assetName]] = all_assetObject[j];
						}
					}
				}
			}
			loadState = LoadState.LoadSuccess;
			IsDone = true;
		}
	}

	public class AllAssetGroupAsync : AllAssetGroup, IResObjAsync
	{
		private Bundle[] bundles;
		private AssetBundleRequest[] abReqs;

		public AllAssetGroupAsync(string[] assetPaths, Action<float> progressing = null, Action<AllAssetGroupAsync> completed = null) : base(assetPaths)
		{
			totalCount = 0;
			curDoneCount = 0;
			_progressing = progressing;
			_completed += completed;
		}

		internal override void Load()
		{
			var len = BundleNames.Length;
			totalCount = len * 2;
			bundles = new Bundle[len];
			abReqs = new AssetBundleRequest[len];
			loadState = LoadState.LoadingBundle;
			for (int i = 0; i < len; i++)
			{
				ResMgr.LoadBundleAsync(BundleNames[i], (obj) =>
				{
					bundles[curDoneCount] = obj as Bundle;
					curDoneCount++;
				}, false);
			}
		}

#region 异步相关

		public int totalCount;
		public int curDoneCount;

		public override bool IsDone
		{
			get
			{
				if (loadState == LoadState.LoadSuccess || loadState == LoadState.LoadFail)
				{
					return true;
				}
				if (_progressing != null) { _progressing.Invoke(Progress); }
				bool _isDone = totalCount == curDoneCount;
				if (_isDone)
				{
					loadState = LoadState.LoadSuccess;
					if (_completed != null)
					{
						_completed.Invoke(this);
					}
					_completed = null;
					return true;
				}

				if (curDoneCount * 2 == totalCount && loadState != LoadState.LoadingAsset)
				{
					loadState = LoadState.LoadingAsset;

					for (int i = 0; i < curDoneCount; i++)
					{
						var bundle = bundles[i];
						var abReq = bundle.AssetBundle.LoadAllAssetsAsync();
						var dic_assetName2assetPathInBundle = ResMgr.ManifestTxt.GetAllAssetInfoInBundle(bundle.id);
						abReq.completed += (_abReq) =>
						{
							var all_assetObject = abReq.allAssets;
							for (int j = 0; j < all_assetObject.Length; j++)
							{
								var assetName = all_assetObject[j].name.ToLower();
								if (dic_assetName2assetPathInBundle.ContainsKey(assetName))
								{
									dic_assetPath2UObj[dic_assetName2assetPathInBundle[assetName]] = all_assetObject[j];
								}
							}
							curDoneCount++;
						};
					}
				}
				return false;
			}
		}

		public float Progress
		{
			get
			{
				float sum = curDoneCount;
				for (int i = 0; i < abReqs.Length; i++)
				{
					if (abReqs[i] != null)
					{
						if (!abReqs[i].isDone)
						{
							sum += abReqs[i].progress;
						}
					}
				}
				return ((float)sum) / (float)totalCount;
			}
		}

		private Action<AllAssetGroupAsync> _completed = null;

		public Action<float> _progressing;

#endregion 异步相关
	}
}