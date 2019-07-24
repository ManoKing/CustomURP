using UnityEngine;

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
		private static ResMgr instance;

		public static bool Init()
		{
			CheckInstance();
			if (ResMgr.IsAssetBundleMode)
			{
				return InitAllManifest();
			}
			return true;
		}

		public static void InitAsync(System.Action<bool> completed)
		{
			CheckInstance();
			if (ResMgr.IsAssetBundleMode)
			{
				InitAllManifestAsync(completed);
			}
			else
			{
				if (completed != null) { completed.Invoke(true); }
			}
		}

		private static void CheckInstance()
		{
			if (instance == null)
			{
				BetterStreamingAssets.Initialize();
				var go = new GameObject("ResMgr");
				DontDestroyOnLoad(go);
				instance = go.AddComponent<ResMgr>();
			}
		}

		private System.Collections.IEnumerator _GC()
		{
			System.GC.Collect();
			yield return 0;
			yield return Resources.UnloadUnusedAssets();
		}

		private void Update()
		{
			if (ResObj.Update())
			{
			}
		}
	}
}