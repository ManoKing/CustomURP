using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		/// <summary>
		/// get set 接口的字典存放
		/// </summary>
		static Dictionary<string, string> dic_getSet = new Dictionary<string, string>();

		static void addToGetSet(string assetPath, string setId)
		{
			if (dic_getSet.ContainsKey(assetPath))
			{
				dic_getSet[assetPath] = setId;
			}
			else
			{
				dic_getSet.Add(assetPath, setId);
			}
		}

		public static void GetUObjAsync(string assetPath, System.Action<UnityEngine.Object> action)
		{
			string setId = null;
			UnityEngine.Object obj = null; ;
			if (ResObj.TryUObjGetByAssetPath(assetPath, ref setId, ref obj))
			{
				addToGetSet(assetPath, setId);
				action(obj);
			}
			else
			{
				setId = assetPath;
				ResMgr.LoadAssetAsync(assetPath, (resObj) =>
				{
					addToGetSet(assetPath, setId);
					action(resObj.UObj);
				});
			}

		}

		public static UnityEngine.Object GetUObj(string assetPath)
		{
			string setId = null;
			UnityEngine.Object obj = null; ;
			if (ResObj.TryUObjGetByAssetPath(assetPath, ref setId, ref obj))
			{
				addToGetSet(assetPath, setId);
				return obj;
			}
			else
			{
				addToGetSet(assetPath, assetPath);
				var resObj = ResMgr.LoadAsset(assetPath, true);
				return resObj.UObj;
			}
		}

		public static void SetUObj(string assetPath)
		{
			ResMgr.Release(dic_getSet[assetPath]);
		}

	}
}