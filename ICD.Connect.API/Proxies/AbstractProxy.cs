using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Proxies
{
	public abstract class AbstractProxy : IProxy, IStateDisposable
	{
		/// <summary>
		/// Raised when the proxy makes an API request.
		/// </summary>
		public event EventHandler<ApiClassInfoEventArgs> OnCommand;

		/// <summary>
		/// Returns true if this instance has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Destructor.
		/// </summary>
		~AbstractProxy()
		{
			Dispose(false);
		}

		#region Methods

		/// <summary>
		/// Called to update the proxy with API info.
		/// </summary>
		/// <param name="result"></param>
		public virtual void ParseInfo(ApiClassInfo result)
		{
		}

		/// <summary>
		/// Instructs the proxy to raise commands requesting initial values.
		/// </summary>
		public void Initialize()
		{
			ApiClassInfo command = new ApiClassInfo();
			Initialize(command);

			if (command.IsEmpty)
				return;

			SendCommand(command);
		}

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected virtual void Initialize(ApiClassInfo command)
		{
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Releases resources but also allows for finalizing without touching managed resources.
		/// </summary>
		/// <param name="disposing"></param>
		private void Dispose(bool disposing)
		{
			OnCommand = null;

			if (!IsDisposed)
				DisposeFinal(disposing);
			IsDisposed = IsDisposed || disposing;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void DisposeFinal(bool disposing)
		{
		}

		/// <summary>
		/// Raises the OnCommand event with the given command.
		/// </summary>
		/// <param name="command"></param>
		protected void SendCommand(ApiClassInfo command)
		{
			if (command == null)
				throw new ArgumentNullException();

			OnCommand.Raise(this, new ApiClassInfoEventArgs(command));
		}

		/// <summary>
		/// Builds and raises a call method command.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		protected void CallMethod(string name, params object[] parameters)
		{
			ApiClassInfo command = ApiCommandBuilder.CallMethodCommand(name, parameters);
			SendCommand(command);
		}

		#endregion
	}
}
