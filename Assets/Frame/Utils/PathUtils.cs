using System.IO;
using UnityEngine;

namespace DH.Frame.Res
{
	internal class PathUtils
	{
		#region 查找路径

		public static string TryGetTargetFilePath(string relativeStreamingPath)
		{
			string bundlePath = string.Empty;
			if (IsExistPersistentDataFile(relativeStreamingPath))
			{
				bundlePath = GetPersistenDataFilePath(relativeStreamingPath);
			}
			else
			{
#if UNITY_IPHONE || UNITY_IOS || UNITY_EDITOR
				bundlePath = GetStreamingAssetsFilePath(relativeStreamingPath);
#elif UNITY_ANDROID
				bundlePath = GetWWWStreamingAssetsPath(relativeStreamingPath);
#endif
			}

			return bundlePath;
		}

		#endregion 查找路径

		#region 工程目录

		public static string GetProjectPath()
		{
			return Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		}

		public static string GetProjectFilePath(string filePath)
		{
			return GetProjectPath() + filePath;
		}

		public static bool IsExistFileInProject(string filePath)
		{
			return File.Exists(GetProjectFilePath(filePath));
		}

		#endregion 工程目录

		#region 持久化目录

		public static string GetPersistentDataPath()
		{
			return Application.persistentDataPath + "/";
		}

		public static string GetWWWPersistenDataFilePath(string relativeStreamingPath) // 持久化 www 加载
		{
			return "file:///" + GetPersistentDataPath() + relativeStreamingPath;
		}

		public static string GetPersistenDataFilePath(string relativeStreamingPath)
		{
			return GetPersistentDataPath() + relativeStreamingPath;
		}

		public static bool IsExistPersistentDataFile(string relativeStreamingPath)
		{
			return File.Exists(GetPersistenDataFilePath(relativeStreamingPath));
		}

		#endregion 持久化目录

		#region 安装包目录

		public static string GetStreamingAssetsPath()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
        return Application.streamingAssetsPath + Path.DirectorySeparatorChar;
#elif UNITY_EDITOR
			return Application.streamingAssetsPath + "/";
#else
        return "file://" + Application.streamingAssetsPath + "/";
#endif
		}

		public static string GetWWWStreamingAssetsPath(string relativeStreamingPath)
		{
			return GetStreamingAssetsPath() + relativeStreamingPath;
		}

		public static string GetStreamingAssetsFilePath(string relativeStreamingPath)
		{
			return Application.streamingAssetsPath + "/" + relativeStreamingPath;
		}

		#endregion 安装包目录

		public static string GetWWWOriginServerFilePath(string orginServerUrl, string relativeStreamingPath)
		{
			return orginServerUrl + "/" + relativeStreamingPath;
		}

		public static string GetStreamRelativePath(string fullPath,string path2)
		{
			return fullPath.Replace(GetProjectFilePath(path2), "");
		}
	}
}