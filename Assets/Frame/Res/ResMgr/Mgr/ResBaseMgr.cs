using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		public static void Release(string assetPath)
		{
			var rObj = ResObj.TryGet<Asset>(assetPath);
			if (rObj != null)
			{
				rObj.Release();
			}
		}

		public static void Release(Asset asset)
		{
			if (asset != null)
			{
				asset.Release();
			}
		}

		public static void Retain(string assetPath)
		{
			var rObj = ResObj.TryGet<Asset>(assetPath);
			if (rObj != null)
			{
				rObj.Retain();
			}
		}

		public static T TryGet<T>(string id) where T : ResObj
		{
			return ResObj.TryGet<T>(id) as T;
		}

		public static void Unload(string id)
		{
			var rObj = ResObj.TryGet<ResObj>(id);
			if (rObj != null)
			{
				rObj.Unload();
			}
		}

		public static void Unload(ResObj rObj)
		{
			if (rObj != null)
			{
				rObj.Unload();
			}
		}
	}
}