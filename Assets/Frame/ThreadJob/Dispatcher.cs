using System;
using System.Collections.Generic;
using UnityEngine;

namespace DH.Frame.ThreadMgr
{
	public class ThreadJobDispatcherCenter : MonoBehaviour
	{
		private readonly object _lock = new object();
		private readonly Queue<Action> _queue = new Queue<Action>();

		public void Enqueue(Action action)
		{
			lock (_lock)
			{
				_queue.Enqueue(action);
			}
		}

		public void Update()
		{
			lock (_lock)
			{
				while (_queue.Count > 0)
				{
					_queue.Dequeue().Invoke();
				}
			}
		}

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}