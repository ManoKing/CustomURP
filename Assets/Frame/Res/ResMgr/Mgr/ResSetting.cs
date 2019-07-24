using UnityEngine;
using System.IO;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace DH.Frame.Res
{
	public partial class ResMgr : MonoBehaviour
	{
#if UNITY_EDITOR
		public static bool IsShowErrorLog = true;
		public static bool IsShowAllLog = true;
#endif

		public static bool IsAssetBundleMode
		{
			get
			{
#if UNITY_EDITOR
				if (activeBundleMode == -1)
					activeBundleMode = EditorPrefs.GetBool(kActiveBundleMode, true) ? 1 : 0;
				return activeBundleMode != 0;
#else
				return true;
#endif
			}
			set
			{
				int newValue = value ? 1 : 0;
				if (newValue != activeBundleMode)
				{
					activeBundleMode = newValue;
#if UNITY_EDITOR
					EditorPrefs.SetBool(kActiveBundleMode, value);
#endif
				}
			}
		}

		private const string kActiveBundleMode = "ActiveBundleMode";
		private static int activeBundleMode = -1;

		public static int MaxLoadingCount = 3;

		private const string _assetBundlesFolderName = "AssetBundles";

		private static string _platformName = null;

		public static string PlatformName
		{
			get
			{
				if (_platformName != null)
				{
					return _platformName;
				}
#if UNITY_EDITOR
				switch (EditorUserBuildSettings.activeBuildTarget)
				{
					case BuildTarget.StandaloneOSX:
						_platformName = "OSX";
						break;

					case BuildTarget.StandaloneWindows:
						_platformName = "Windows";
						break;

					case BuildTarget.StandaloneWindows64:
						_platformName = "Windows";
						break;

					case BuildTarget.iOS:
						_platformName = "iOS";
						break;

					case BuildTarget.Android:
						_platformName = "Android";
						break;

					default:
						Debug.LogError("获取不到运行平台类型");
						break;
				}
#else
				switch (Application.platform)
				{
					case RuntimePlatform.OSXPlayer:
						_platformName = "OSX";
						break;

					case RuntimePlatform.OSXEditor:
						_platformName = "OSX";
						break;

					case RuntimePlatform.WindowsPlayer:
						_platformName = "Windows";
						break;

					case RuntimePlatform.WindowsEditor:
						_platformName = "Windows";
						break;

					case RuntimePlatform.IPhonePlayer:
						_platformName = "iOS";
						break;

					case RuntimePlatform.Android:
						_platformName = "Android";
						break;
				}
#endif
				_platformName = _platformName.ToLower();
				return _platformName;
			}
		}

		public static string ABFolderName
		{
			get
			{
				if (_abFolderName == null)
					_abFolderName = Path.Combine(_assetBundlesFolderName, PlatformName);
				return _abFolderName;
			}
		}

		private static string _abFolderName = null;

		public static string ABStreamingFilePath
		{
			get
			{
				if (_abFilePath == null)
				{
					_abFilePath = PathUtils.GetStreamingAssetsFilePath(ABFolderName);
				}
				return _abFilePath;
			}
		}

		private static string _abFilePath = null;
	}
}