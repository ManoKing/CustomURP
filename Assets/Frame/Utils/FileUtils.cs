using Frame.ThreadMgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Frame.Res
{
	public class FileUtils
	{
		/// <summary>
		/// 线程异步读取文件
		/// </summary>
		/// <param name="relativeStreamingAssetPath"></param>
		/// <param name="compelted"></param>
		/// <returns></returns>
		public static Action ReadFileAsync(string relativeStreamingAssetPath, Action<byte[]> compelted)
		{
			var filePath = PathUtils.TryGetTargetFilePath(relativeStreamingAssetPath);
			ThreadJob<byte[]> task = new ThreadJob<byte[]>(() =>
			{
#if UNITY_EDITOR || UNITY_IPHONE
				var bytes = File.ReadAllBytes(filePath);
				bytes = XOR.Decrypt(bytes);
				return bytes;
#elif UNITY_ANDROID
				var bytes = BetterStreamingAssets.ReadAllBytes(streamingAssetPath);
				bytes = XOR.Decrypt(bytes);
				return bytes;
#else
				var bytes = File.ReadAllBytes(filePath);
				bytes = XOR.Decrypt(bytes);
				return bytes;
#endif
			});
			Action doStart = () =>
			{
				task.ContinueOnUIThread((r) =>
				{
					compelted(r.Result);
				});
				task.Start();
			};
			doStart();
			return () =>
			{
				task.Abort();
			};
		}

		/// <summary>
		/// 同步读取文件
		/// </summary>
		/// <param name="relativeStreamingAssetPath"></param>
		/// <returns></returns>
		public static byte[] ReadFileSync(string relativeStreamingAssetPath)
		{
			byte[] bytes = null;

			var filePath = PathUtils.TryGetTargetFilePath(relativeStreamingAssetPath);

#if UNITY_EDITOR || UNITY_IPHONE
			bytes = File.ReadAllBytes(filePath);
#elif UNITY_ANDROID
			bytes = BetterStreamingAssets.ReadAllBytes(streamingAssetPath);
#endif
			byte[] newbytes = XOR.Decrypt(bytes);
			return newbytes;
		}

		public static void EncryptFilesInPath(string path)
		{
			string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].EndsWith(".meta")) continue;
				byte[] bytes = File.ReadAllBytes(files[i]);
				byte[] newbytes = XOR.Encrypt(bytes);
				File.WriteAllBytes(files[i], newbytes);
			}
		}

		public static List<string> GetResursionFileList(string rootPath)
		{
			var res = new List<string>();
			_GetResursionFileList(rootPath, ref res);
			return res;
		}

		private static void _GetResursionFileList(string rootPath, ref List<string> res)
		{
			if (!Directory.Exists(rootPath))
			{
				Directory.CreateDirectory(rootPath);
			}
			var files = Directory.GetFiles(rootPath, "*");
			foreach (var file in files)
			{
				res.Add(file.Replace("\\", "/"));
			}
			var directories = Directory.GetDirectories(rootPath, "*");
			foreach (var dir in directories)
			{
				_GetResursionFileList(dir, ref res);
			}
		}

		// 写入文本到持久化目录中
		public static bool WriteContentToFile(string path, string content, bool isConver = true)
		{
			CreateDirectoryByFilePath(path);
			if (File.Exists(path))
			{
				if (isConver)
				{
					return false;
				}
				else
				{
					File.Delete(path);
				}
			}
			using (var s = new StreamWriter(path))
			{
				s.Write(content);
				s.Flush();
				s.Close();
			}
			return true;
		}

		public static void CreateDirectoryByFilePath(string path)
		{
			// 创建目录
			//UnityEngine.Debug.Log(" CreateDirectoryOfFile " + path);
			var dirPath = Path.GetDirectoryName(path);
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		// 删除某个目录下所有文件
		public static void EmptyFolder(string folderName)
		{
			foreach (var item in Directory.GetDirectories(folderName, "*", SearchOption.TopDirectoryOnly))
			{
				Directory.Delete(item, true);
			}
			foreach (var item in Directory.GetFiles(folderName, "*", SearchOption.TopDirectoryOnly))
			{
				File.Delete(item);
			}
		}

		public static string GetFileMD5(string fullPath)
		{
			FileStream file = new FileStream(fullPath, FileMode.Open);
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(file);
			file.Close();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}

		// 获取文件大小
		public static int GetFileSize(string fullPath)
		{
			if (File.Exists(fullPath))
				return (int)(new FileInfo(fullPath).Length);
			return 0;
		}

		// 追加内容
		public static void AppendTxt(string filePath, string content)
		{
			using (StreamWriter sw = File.AppendText(filePath))
			{
				sw.WriteLine(content);
				sw.Flush();
				sw.Close();
			}
		}

		public static void CopyFiles(string src, string dst, string withExt = null)
		{
			List<string> strs = new List<string>();
			_GetResursionFileList(src, ref strs);
			foreach (var item in strs)
			{
				if (!item.EndsWith(".meta"))
				{
					if ((!string.IsNullOrEmpty(withExt) && !item.EndsWith(withExt)))
					{
						continue;
					}
					string res = dst + item.Substring(src.Length, item.Length - src.Length);
					res = res.ToLower();
					string dirPath = Path.GetDirectoryName(res).ToLower();
					if (!Directory.Exists(dirPath))
					{
						Directory.CreateDirectory(dirPath);
					}
					if (File.Exists(res))
					{
						File.Delete(res);
					}
					File.Copy(item, res);
				}
			}
		}

		// 判断是否同个文件
		public static bool IsSameFile(string fullPathA, string fullPathB)
		{
			if (File.Exists(fullPathA) && File.Exists(fullPathB))
			{
				return GetFileMD5(fullPathA).CompareTo(GetFileMD5(fullPathB)) == 0;
			}
			return false;
		}
	}
}

//}