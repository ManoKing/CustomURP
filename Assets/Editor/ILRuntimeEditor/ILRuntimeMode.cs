using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public class ILRuntimeMode
{
	public static bool IsRelease = false;

	private const string RuntimeMode = "设置/ILRuntime/ILRuntimeMode";
	private const string RuntimeMode1 = "设置/开启ILRuntimeMode";

	private const string Symbol = "ILRuntime";

	static ILRuntimeMode()
	{
		string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		string newSymbolStr = string.Empty;
		string[] symbols = symbolStr.Split(';');
		if (symbolStr.Contains(Symbol))
		{
			IsRelease = true;
		}
		else
		{
			IsRelease = false;
		}
		Menu.SetChecked(RuntimeMode, IsRelease);
		Menu.SetChecked(RuntimeMode1, IsRelease);
	}

	[MenuItem(RuntimeMode, false)]
	[MenuItem(RuntimeMode1, false)]
	public static void SetILRuntimeMode()
	{
		IsRelease = IsRelease ? false : true;
		Menu.SetChecked(RuntimeMode, IsRelease);
		string symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		string newSymbolStr = string.Empty;
		string[] symbols = symbolStr.Split(';');
		if (IsRelease)
		{
			newSymbolStr = symbolStr + ";" + Symbol;
		}
		else
		{
			List<string> temp = new List<string>(symbols);
			for (int i = 0; i < temp.Count; i++)
			{
				if (symbols[i] != Symbol)
				{
					newSymbolStr += (";" + temp[i]);
				}
			}
		}
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newSymbolStr);
	}
}