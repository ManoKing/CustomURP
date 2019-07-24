using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
	private static bool sAppQuit = false;
	private static T sOnly;

	public static T Instance
	{
		get
		{
			if (sOnly == null && !sAppQuit)
			{
				GameObject obj = new GameObject(typeof(T).Name);
				DontDestroyOnLoad(obj);
				//obj.hideFlags = HideFlags.HideInHierarchy;
				sOnly = obj.AddComponent<T>();
			}

			return sOnly;
		}
	}

	protected virtual void Exit()
	{
	}

	private void OnApplicationQuit()
	{
		Exit();
		sAppQuit = true;
	}
}
