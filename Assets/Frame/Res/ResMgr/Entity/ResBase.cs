using System;
using System.Collections.Generic;

namespace DH.Frame.Res
{
	public enum LoadState
	{
		None,

		ReadingFile,

		LoadingBundle,

		LoadingAsset,

		LoadSuccess,

		LoadFail,
	}

	public interface IResObjAsync
	{
		float Progress { get; }
	}

	public class ResObj
	{
		public ResObj()
		{
		}

		public ResObj(string id)
		{
			this.id = id.ToLower();
			RefCount = 1;
			IsDone = false;
			UObj = null;
		}

		public int RefCount { get; private set; }

		public virtual bool IsDone { get; set; }

		public virtual UnityEngine.Object UObj { get; set; }

		public LoadState loadState = LoadState.None;

		public bool IsError
		{
			get
			{
				return !string.IsNullOrEmpty(error);
			}
		}

		public string error = String.Empty;

		public string id = String.Empty;

		internal virtual void Load()
		{
		}

		internal virtual void Unload()
		{
			RemoveFromLoaded();
			loadState = LoadState.None;
			id = null;
			RefCount = 0;
			IsDone = false;
			UObj = null;
		}

		public void Retain()
		{
			RefCount++;
		}

		public void Release()
		{
			RefCount--;
			if (!dic_dirtyRef.ContainsKey(id))
				dic_dirtyRef.Add(id, this);
		}

		#region 静态域

		private static List<ResObj> list_curFrameDone = new List<ResObj>();

		private static bool b_isExistCurFrameDone = false;

		private static Dictionary<string, ResObj> dic_dirtyRef = new Dictionary<string, ResObj>();

		private static Dictionary<string, ResObj> dic_loaded = new Dictionary<string, ResObj>();

		private static Dictionary<string, ResGroupBase> dic_loadedGroup = new Dictionary<string, ResGroupBase>();

		private static Dictionary<string, UnityEngine.Object> dic_loadedGroup_assetPath2UObject = new Dictionary<string, UnityEngine.Object>();

		private static Dictionary<string, string> dic_loadedGroup_assetPath2GroupId = new Dictionary<string, string>();

		private static Dictionary<string, ResObj> dic_loading = new Dictionary<string, ResObj>();

		private static List<ResObj> list_loading = new List<ResObj>();

		private static List<ResObj> list_toLoad = new List<ResObj>();

		private static Dictionary<string, ResObj> dic_toLoad = new Dictionary<string, ResObj>();

		private static Dictionary<string, ResObj> dic_loadError = new Dictionary<string, ResObj>();

		#endregion 静态域

		internal void AddToLoaded()
		{
			dic_loaded.Add(id, this);
			if (id.StartsWith(ResGroupBase.GroupStr))
			{
				var group = this as ResGroupBase;
				dic_loadedGroup.Add(id, group);
				foreach (var item in group.GetUObjDic())
				{
					dic_loadedGroup_assetPath2UObject.Add(item.Key, item.Value);
					dic_loadedGroup_assetPath2GroupId.Add(item.Key, id);
				}
			}
		}

		internal void RemoveFromLoaded()
		{
			dic_loaded.Remove(id);
			if (id.StartsWith(ResGroupBase.GroupStr))
			{
				var group = this as ResGroupBase;
				dic_loadedGroup.Remove(id);
				foreach (var item in group.GetUObjDic())
				{
					dic_loadedGroup_assetPath2UObject.Remove(item.Key);
					dic_loadedGroup_assetPath2GroupId.Remove(item.Key);
				}
			}
		}

		internal void AddToAsyncQueue()
		{
			if (CurLoadingNum <= MaxAllowLoadingNum)
			{
				Load();
				dic_loading.Add(id, this);
				list_loading.Add(this);
			}
			else
			{
				list_toLoad.Add(this);
				dic_toLoad.Add(id, this);
			}
		}

		internal void RemoveFromLoading()
		{
			dic_loading.Remove(id);
			list_loading.Remove(this);
		}

		internal void RemoveFromToLoad()
		{
			dic_toLoad.Remove(id);
			list_toLoad.Remove(this);
		}

		internal void AddToLoadError()
		{
			dic_loadError.Add(id, this);
		}

		public static void TryUnloadUnUsed()
		{
			foreach (var item in dic_dirtyRef)
			{
				if (item.Value.RefCount == 0)
				{
					item.Value.Unload();
				}
			}
		}

		static internal bool Update()
		{
			if (dic_loading.Count > 0)
			{
				b_isExistCurFrameDone = false;
				list_curFrameDone.Clear();
				for (int i = 0; i < list_loading.Count; i++)
				{
					var resObj = list_loading[i];
					if (resObj.IsDone)
					{
						b_isExistCurFrameDone = true;
						list_curFrameDone.Add(resObj);
					}
				}

				if (b_isExistCurFrameDone)
				{
					for (int i = 0; i < list_curFrameDone.Count; i++)
					{
						dic_loading.Remove(list_curFrameDone[i].id);
						list_loading.Remove(list_curFrameDone[i]);

						if (list_curFrameDone[i].loadState == LoadState.LoadSuccess)
						{
							if (list_curFrameDone[i].id.StartsWith(ResGroupBase.GroupStr))
							{
								var group = list_curFrameDone[i] as ResGroupBase;
								dic_loadedGroup.Add(group.id, group);
								foreach (var item in group.GetUObjDic())
								{
									dic_loadedGroup_assetPath2UObject.Add(item.Key, item.Value);
									dic_loadedGroup_assetPath2GroupId.Add(item.Key, group.id);
								}
							}
							dic_loaded.Add(list_curFrameDone[i].id, list_curFrameDone[i]);
						}
						else
						{
							list_curFrameDone[i].AddToLoadError();
						}
					}

					if (list_toLoad.Count > 0)
					{
						for (int i = 0; i < list_curFrameDone.Count; i++)
						{
							var item = list_toLoad[0];
							list_toLoad.RemoveAt(0);
							dic_toLoad.Remove(item.id);
							dic_loading.Add(item.id, item);
							list_loading.Add(item);
							item.Load();
							if (list_toLoad.Count == 0)
							{
								break;
							}
						}
					}
				}
			}
			return b_isExistCurFrameDone;
		}

		internal static T TryGet<T>(string id) where T : ResObj
		{
			ResObj obj;
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(id))
			{
				UnityEngine.Debug.LogError("id不能为空");
				return null;
			}
#endif
			id = id.ToLower();
			dic_loaded.TryGetValue(id, out obj);
			if (obj == null)
				dic_loading.TryGetValue(id, out obj);
			if (obj == null)
				dic_toLoad.TryGetValue(id, out obj);
			return obj as T;
		}

		internal static bool IsDealing<T>(string id) where T : ResObj
		{
			return TryGet<T>(id) == null;
		}

		internal static int CurLoadingNum
		{
			get
			{
				return dic_loading.Count;
			}
		}

		internal static int MaxAllowLoadingNum = 5;

		internal static void ResetAll()
		{
			b_isExistCurFrameDone = false;
			dic_dirtyRef.Clear();
			dic_loaded.Clear();
			dic_loadedGroup.Clear();
			dic_loadError.Clear();
			dic_loading.Clear();
			list_loading.Clear();
			list_curFrameDone.Clear();
			list_toLoad.Clear();
			dic_toLoad.Clear();
		}

		internal static bool TryUObjGetByAssetPath(string assetPath, ref string id, ref UnityEngine.Object uobj)
		{
			ResObj resObj = null;
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(assetPath))
			{
				UnityEngine.Debug.LogError("id不能为空");
				return false;
			}
#endif
			assetPath = assetPath.ToLower();

			if(dic_loadedGroup_assetPath2UObject.TryGetValue(assetPath,out uobj))
			{
				id = dic_loadedGroup_assetPath2GroupId[assetPath];
				dic_loaded[id].Retain();//addref
				return true;
			}
			if(dic_loaded.TryGetValue(assetPath, out resObj))
			{
				id = assetPath;
				uobj = resObj.UObj;
				resObj.Retain();//addref
				return true;
			}

			return false;
		}

	}
}