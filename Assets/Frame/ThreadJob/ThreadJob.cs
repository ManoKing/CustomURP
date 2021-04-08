using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frame.ThreadMgr
{
	public class ThreadJob : IThreadJob
	{
		protected IThreadJob _nextJob;

		protected ThreadEx _thread;

		private static ThreadJobDispatcherCenter _dispatcher;

		public ThreadJob(Action action)
		{
			Action _action = () =>
			{
				try
				{
					action();
				}
				catch (Exception e)
				{
					Debug.LogError("ThreadJob Excepetion Message: " + e.Message);
					State = ThreadJobState.Faulted;
				}
			};

			_initJob(_action);
		}

		protected ThreadJob()
		{
		}

		public ThreadJobState State
		{
			get; protected set;
		}

		public static void Init()
		{
			if (_dispatcher == null)
			{
				_dispatcher = new GameObject("ThreadDispatcherCenter").AddComponent<ThreadJobDispatcherCenter>();
			}
		}

		public static ThreadJob Run(Action action)
		{
			ThreadJob job = new ThreadJob(action);
			job.Start();
			return job;
		}

		public static ThreadJob<TResult> Run<TResult>(Func<TResult> action)
		{
			ThreadJob<TResult> job = new ThreadJob<TResult>(action);
			job.Start();
			return job;
		}

		public static void RunOnUIThread(Action action)
		{
			_dispatcher.Enqueue(action);
		}

		public static void WaitAll(params ThreadJob[] unityTasks)
		{
			foreach (ThreadJob job in unityTasks)
			{
				job.Wait();
			}
		}

		public static void WaitAll(IEnumerable<ThreadJob> unityTasks)
		{
			foreach (ThreadJob job in unityTasks)
			{
				job.Wait();
			}
		}

		public void Abort()
		{
			if (State == ThreadJobState.Running)
			{
				_thread.Abort();
				State = ThreadJobState.Aborted;
			}
		}

		public void ContinueOnUIThread(Action<ThreadJob> action)
		{
			Action _action = () =>
			{
				action(this);
			};

			_nextJob = new UIThreadJob(_action);
		}

		public ThreadJob ContinueWith(Action<ThreadJob> action)
		{
			Action _action = () =>
			{
				try
				{
					action(this);
				}
				catch (Exception e)
				{
					Debug.LogError("UnityTask Excepetion Message: " + e.Message);
					State = ThreadJobState.Faulted;

					throw;
				}
			};
			ThreadJob job = new ThreadJob(_action);
			_nextJob = job;

			return job;
		}

		public ThreadJob<TResult> ContinueWith<TResult>(Func<ThreadJob, TResult> func)
		{
			Func<TResult> _func = () =>
			{
				return func(this);
			};
			ThreadJob<TResult> job = new ThreadJob<TResult>(_func);
			_nextJob = job;

			return job;
		}

		public void Start()
		{
			if (State == ThreadJobState.Created)
			{
				State = ThreadJobState.Running;
				_thread.Start();
			}
		}

		public void Wait()
		{
			if (State == ThreadJobState.Running)
			{
				_thread.Join();
			}
		}

		protected void _initJob(Action action)
		{
			action += () =>
			{
				if (State != ThreadJobState.Aborted && State != ThreadJobState.Faulted)
				{
					State = ThreadJobState.Finished;
				}

				if (_nextJob != null && State != ThreadJobState.Aborted)
				{
					_nextJob.Start();
				}
			};
			_thread = new ThreadEx(action);
			State = ThreadJobState.Created;
		}
	}

	public class ThreadJob<TResult> : ThreadJob
	{
		public ThreadJob(Func<TResult> func)
		{
			Action _action = () =>
			{
				try
				{
					Result = func();
				}
				catch (Exception e)
				{
					Debug.LogError("UnityTask Excepetion Message: " + e.Message);
					State = ThreadJobState.Faulted;
				}
			};

			_initJob(_action);
		}

		public TResult Result
		{
			get; set;
		}

		public void ContinueOnUIThread(Action<ThreadJob<TResult>> action)
		{
			Action _action = () =>
			{
				action(this);
			};
			_nextJob = new UIThreadJob(_action);
		}

		public ThreadJob ContinueWith(Action<ThreadJob<TResult>> action)
		{
			Action _action = () =>
			{
				action(this);
			};

			ThreadJob job = new ThreadJob(_action);
			_nextJob = job;

			return job;
		}

		public ThreadJob<UResult> ContinueWith<UResult>(Func<ThreadJob<TResult>, UResult> function)
		{
			Func<UResult> wrapperFunc = () =>
			{
				return function(this);
			};

			ThreadJob<UResult> job = new ThreadJob<UResult>(wrapperFunc);
			_nextJob = job;

			return job;
		}
	}
}