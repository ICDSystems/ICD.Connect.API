using System;
using System.Collections.Generic;
using System.Linq;
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
		/// <param name="info"></param>
		public void ParseInfo(ApiClassInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			foreach (ApiPropertyInfo property in info.GetProperties())
				ParseProperty(property);

			foreach (ApiMethodInfo method in info.GetMethods())
				ParseMethod(method);
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
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Updates the proxy with a property result.
		/// </summary>
		/// <param name="property"></param>
		private void ParseProperty(ApiPropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			// Only care about good results
			if (property.Result == null || property.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			ParseProperty(property.Name, property.Result);
		}

		/// <summary>
		/// Updates the proxy with a property result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected virtual void ParseProperty(string name, ApiResult result)
		{
		}

		/// <summary>
		/// Updates the proxy with a method result.
		/// </summary>
		/// <param name="method"></param>
		private void ParseMethod(ApiMethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			// Only care about good results
			if (method.Result == null || method.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			ParseMethod(method.Name, method.Result);
		}

		/// <summary>
		/// Updates the proxy with a method result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		private void ParseMethod(string name, ApiResult result)
		{
		}

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected virtual void Initialize(ApiClassInfo command)
		{
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
