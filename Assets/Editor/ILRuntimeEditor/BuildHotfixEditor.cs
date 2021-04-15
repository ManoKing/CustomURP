using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
	public const string isMixCodeKey = "isMixCode";

	private const string HotfixDll = "Library/ScriptAssemblies/Game.dll";
	private const string HotfixPdb = "Library/ScriptAssemblies/Game.pdb";

	public static string IL_HotFixFile_Dll = "Assets/Res/Base/Dll/hotfix.dll.bytes";
	public static string IL_HotFixFile_PDB = "Assets/Res/Base/Dll/hotfix.pdb.bytes";


	//private const string RuntimeMode = "设置/加密/是否自动混淆DLL";
	private const string ScriptAssembliesDir = "";

	static Startup()
	{
		///TODO 调用为了防止没有注册过 编译完成事件，调用两次为了防止改变其状态值
		//EditorApplication.ExecuteMenuItem(RuntimeMode);
		//EditorApplication.ExecuteMenuItem(RuntimeMode);
	}

	public static bool CompareFile(string filePath1, string filePath2)
	{

		if(!File.Exists(filePath1) || !File.Exists(filePath2))
		{
			return false;
		}

		//计算第一个文件的哈希值
		HashAlgorithm hash = HashAlgorithm.Create();
		var stream_1 = new System.IO.FileStream(filePath1, System.IO.FileMode.Open);
		byte[] hashByte_1 = hash.ComputeHash(stream_1);
		stream_1.Close();
		//计算第二个文件的哈希值
		var stream_2 = new System.IO.FileStream(filePath2, System.IO.FileMode.Open);
		byte[] hashByte_2 = hash.ComputeHash(stream_2);
		stream_2.Close();
		return BitConverter.ToString(hashByte_1) == BitConverter.ToString(hashByte_2);
	}

	public static void CopyDll()
	{

		//if (true)
		//{
		//	return;
		//	AssetDatabase.Refresh();
		//}

		var folder = Path.GetDirectoryName(IL_HotFixFile_Dll);

		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}

		if (!CompareFile(HotfixDll, IL_HotFixFile_Dll))
		{
			if(File.Exists(HotfixDll))
			{
				File.Copy(HotfixDll, IL_HotFixFile_Dll, true);
				File.Copy(HotfixPdb, IL_HotFixFile_PDB, true);
				Debug.Log($"成功复制 Hotfix.dll, Hotfix.pdb到 目录 " + folder);
			}
		}

		if (true)
		{
			return;
			AssetDatabase.Refresh();
		}

		//TODO 注册 unity编译成功后 反射调用的方法


	}

	public static void MixCode()
	{
        /*
		var directoryInfo = new DirectoryInfo(Application.dataPath);
		var exePath = $@"{directoryInfo.Parent}\3rdLib\Eazfuscator.NET\Eazfuscator.NET.exe";
		var output = $@"{directoryInfo.Parent}\{DH.Frame.GamePath.IL_HotFixFile_Dll}";
		var reference = $@"{directoryInfo.Parent}\Temp\bin\Debug";
		var input = $@"{directoryInfo.Parent}\Library\ScriptAssemblies\{HotfixDll}";
		Debug.Log($@"{exePath}   {output}   {reference}  {input}");
		ProcessCommand(exePath, $@" -o {output} {input}  --probing-paths {reference}");
		//混淆代码的时候已经移动过去了
		//File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "DH_Game.dll.bytes"), true);
		File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), DH.Frame.GamePath.IL_HotFixFile_PDB, true);
		AssetDatabase.Refresh();
		Debug.Log($"复制Hotfix.dll, Hotfix.pdb到Assets/DLL/完成");
        */
	}
	//
	//[MenuItem(RuntimeMode)]
	//public static void MixModel()
	//{
	//	var isMixCode = !Menu.GetChecked(RuntimeMode);
	//	EditorPrefs.SetBool(isMixCodeKey, isMixCode);
	//	CompilingFinishedCallback.RemoveAll();
	//	if (isMixCode)
	//	{
	//		CompilingFinishedCallback.Set<Startup>("MixCode");
	//	}
	//	else
	//	{
	//		CompilingFinishedCallback.Set<Startup>("CopyDll");
	//	}
	//	Menu.SetChecked(RuntimeMode, isMixCode);
	//}
	//
	//[MenuItem(RuntimeMode, true)]
	//public static bool MixModelCheck()
	//{
	//	var isMixCode = EditorPrefs.GetBool(isMixCodeKey);
	//	Menu.SetChecked(RuntimeMode, isMixCode);
	//	return true;
	//}

	public static void ProcessCommand(string command, string argument)
	{
		System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
		info.Arguments = argument;
		info.CreateNoWindow = false;
		info.ErrorDialog = true;
		info.UseShellExecute = true;

		if (info.UseShellExecute)
		{
			info.RedirectStandardOutput = false;
			info.RedirectStandardError = false;
			info.RedirectStandardInput = false;
		}
		else
		{
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
			info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
		}

		System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

		if (!info.UseShellExecute)
		{
			Debug.Log(process.StandardOutput);
			Debug.Log(process.StandardError);
		}

		process.WaitForExit();
		process.Close();
	}
}