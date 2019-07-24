using System;

namespace DH.Frame.ThreadMgr
{
	public class UIThreadJob : IThreadJob
	{
		private Action _action;

		public UIThreadJob(Action action)
		{
			_action = action;
		}

		public void Start()
		{
			ThreadJob.RunOnUIThread(_action);
		}
	}
}