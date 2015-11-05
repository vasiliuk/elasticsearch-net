﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Nest
{
	public class SnapshotObservable : IDisposable, IObservable<ISnapshotStatusResponse>
	{
		private readonly IElasticClient _elasticClient;
		private readonly ISnapshotRequest _snapshotRequest;
		private readonly TimeSpan _interval = TimeSpan.FromSeconds(2);
		private Timer _timer;
		private bool _disposed;
		private readonly SnapshotStatusHumbleObject _snapshotStatusHumbleObject;
		private EventHandler<SnapshotNextEventArgs> _nextEventHandler;
		private EventHandler<SnapshotCompletedEventArgs> _completedEentHandler;
		private EventHandler<SnapshotErrorEventArgs> _errorEventHandler;

		public SnapshotObservable(IElasticClient elasticClient, ISnapshotRequest snapshotRequest)
		{
			elasticClient.ThrowIfNull("elasticClient");
			snapshotRequest.ThrowIfNull("snapshotRequest");

			_elasticClient = elasticClient;
			_snapshotRequest = snapshotRequest;
			_snapshotStatusHumbleObject = new SnapshotStatusHumbleObject(elasticClient, snapshotRequest);
			_snapshotStatusHumbleObject.Completed += StopTimer;
			_snapshotStatusHumbleObject.Error += StopTimer;
		}

		public SnapshotObservable(IElasticClient elasticClient, ISnapshotRequest snapshotRequest, TimeSpan interval)
			: this(elasticClient, snapshotRequest)
		{
			interval.ThrowIfNull("interval");
			if (interval.Ticks < 0) throw new ArgumentOutOfRangeException("interval");

			_interval = interval;
		}

		public IDisposable Subscribe(IObserver<ISnapshotStatusResponse> observer)
		{
			observer.ThrowIfNull("observer");

			try
			{
				_snapshotRequest.RequestParameters.WaitForCompletion(false);
				var snapshotResponse = this._elasticClient.Snapshot(_snapshotRequest);

				if (!snapshotResponse.IsValid)
					throw new SnapshotException(snapshotResponse.ConnectionStatus, "Can't create snapshot");

				EventHandler<SnapshotNextEventArgs> onNext = (sender, args) => observer.OnNext(args.SnapshotStatusResponse);
				EventHandler<SnapshotCompletedEventArgs> onCompleted = (sender, args) => observer.OnCompleted();
				EventHandler<SnapshotErrorEventArgs> onError = (sender, args) => observer.OnError(args.Exception);

				_nextEventHandler = onNext;
				_completedEentHandler = onCompleted;
				_errorEventHandler = onError;

				_snapshotStatusHumbleObject.Next += onNext;
				_snapshotStatusHumbleObject.Completed += onCompleted;
				_snapshotStatusHumbleObject.Error += onError;

#if NETFXCORE
                var dueTime = (int)_interval.TotalMilliseconds; // probable overflow
#else
                var dueTime = (long)_interval.TotalMilliseconds;
#endif

                _timer = new Timer(Snapshot, observer, dueTime, Timeout.Infinite);
			}
			catch (Exception exception)
			{
				observer.OnError(exception);
			}

			return this;
		}

		private void Snapshot(object state)
		{
			var observer = state as IObserver<ISnapshotStatusResponse>;

			if (observer == null) throw new ArgumentException("state");

			try
			{
				var watch = new Stopwatch();
				watch.Start();

				_snapshotStatusHumbleObject.CheckStatus();

#if NETFXCORE
                var msec = (int)((long)_interval.TotalMilliseconds - watch.ElapsedMilliseconds); // probable overflow
#else
                var msec = (long)_interval.TotalMilliseconds - watch.ElapsedMilliseconds;
#endif

                _timer.Change(Math.Max(0, msec), Timeout.Infinite);
			}
			catch (Exception exception)
			{
				observer.OnError(exception);
				StopTimer(null, null);
			}
		}

		private void StopTimer(object sender, EventArgs restoreCompletedEventArgs)
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (_timer != null) _timer.Dispose();

			if (_snapshotStatusHumbleObject != null)
			{
				_snapshotStatusHumbleObject.Next -= _nextEventHandler;
				_snapshotStatusHumbleObject.Completed -= _completedEentHandler;
				_snapshotStatusHumbleObject.Error -= _errorEventHandler;

				_snapshotStatusHumbleObject.Completed -= StopTimer;
				_snapshotStatusHumbleObject.Error -= StopTimer;
			}

			_disposed = true;
		}

		~SnapshotObservable()
		{
			Dispose(false);
		}
	}

	public class SnapshotNextEventArgs : EventArgs
	{
		public ISnapshotStatusResponse SnapshotStatusResponse { get; private set; }

		public SnapshotNextEventArgs(ISnapshotStatusResponse snapshotStatusResponse)
		{
			SnapshotStatusResponse = snapshotStatusResponse;
		}
	}

	public class SnapshotCompletedEventArgs : EventArgs
	{
		public ISnapshotStatusResponse SnapshotStatusResponse { get; private set; }

		public SnapshotCompletedEventArgs(ISnapshotStatusResponse snapshotStatusResponse)
		{
			SnapshotStatusResponse = snapshotStatusResponse;
		}
	}

	public class SnapshotErrorEventArgs : EventArgs
	{
		public Exception Exception { get; private set; }

		public SnapshotErrorEventArgs(Exception exception)
		{
			Exception = exception;
		}
	}

	public class SnapshotStatusHumbleObject
	{
		private readonly IElasticClient _elasticClient;
		private readonly ISnapshotRequest _snapshotRequest;

		public event EventHandler<SnapshotCompletedEventArgs> Completed;
		public event EventHandler<SnapshotErrorEventArgs> Error;
		public event EventHandler<SnapshotNextEventArgs> Next;

		public SnapshotStatusHumbleObject(IElasticClient elasticClient, ISnapshotRequest snapshotRequest)
		{
			elasticClient.ThrowIfNull("elasticClient");
			snapshotRequest.ThrowIfNull("snapshotRequest");

			_elasticClient = elasticClient;
			_snapshotRequest = snapshotRequest;
		}

		public void CheckStatus()
		{
			try
			{
				var snapshotStatusResponse =
					_elasticClient.SnapshotStatus(new SnapshotStatusRequest(_snapshotRequest.Repository,
						_snapshotRequest.Snapshot));

				if (!snapshotStatusResponse.IsValid)
					throw new SnapshotException(snapshotStatusResponse.ConnectionStatus, "Can't check snapshot status");

				if (snapshotStatusResponse.Snapshots.All(s => s.ShardsStats.Done == s.ShardsStats.Total))
				{
					OnCompleted(new SnapshotCompletedEventArgs(snapshotStatusResponse));
					return;
				}

				OnNext(new SnapshotNextEventArgs(snapshotStatusResponse));
			}
			catch (Exception exception)
			{
				OnError(new SnapshotErrorEventArgs(exception));
			}
		}

		protected virtual void OnNext(SnapshotNextEventArgs nextEventArgs)
		{
			var handler = Next;
			if (handler != null) handler(this, nextEventArgs);
		}

		protected virtual void OnCompleted(SnapshotCompletedEventArgs completedEventArgs)
		{
			var handler = Completed;
			if (handler != null) handler(this, completedEventArgs);
		}

		protected virtual void OnError(SnapshotErrorEventArgs errorEventArgs)
		{
			var handler = Error;
			if (handler != null) handler(this, errorEventArgs);
		}
	}

}