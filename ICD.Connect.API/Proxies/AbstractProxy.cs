using System;
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

			foreach (ApiEventInfo eventInfo in info.GetEvents().Where(e => e.SubscribeAction == ApiEventInfo.eSubscribeAction.None))
				ParseEvent(eventInfo);

			foreach (ApiPropertyInfo property in info.GetProperties())
				ParseProperty(property);

			foreach (ApiMethodInfo method in info.GetMethods())
				ParseMethod(method);

			foreach (ApiNodeInfo node in info.GetNodes())
				ParseNode(node);

			foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
				ParseNodeGroup(nodeGroup);
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
		/// Instructs the proxy to clear any initialized values.
		/// </summary>
		public virtual void Deinitialize()
		{
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

		/// <summary>
		/// Builds and raises a set property command.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		protected void SetProperty(string name, object value)
		{
			ApiClassInfo command = ApiCommandBuilder.SetPropertyCommand(name, value);
			SendCommand(command);
		}

		#endregion

		#region Parsing

		/// <summary>
		/// Updates the proxy with event feedback info.
		/// </summary>
		/// <param name="eventInfo"></param>
		private void ParseEvent(ApiEventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("property");

			// The event doesn't have data?
			if (eventInfo.Result == null)
				return;

			// Only care about good results
			if (eventInfo.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			ParseEvent(eventInfo.Name, eventInfo.Result);
		}

		/// <summary>
		/// Updates the proxy with event feedback info.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected virtual void ParseEvent(string name, ApiResult result)
		{
		}

		/// <summary>
		/// Updates the proxy with a property result.
		/// </summary>
		/// <param name="property"></param>
		private void ParseProperty(ApiPropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			// The property doesn't have data?
			if (property.Result == null)
				return;

			// Only care about good results
			if (property.Result.ErrorCode != ApiResult.eErrorCode.Ok)
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

			// The method result doesn't have data?
			if (method.Result == null)
				return;

			// Only care about good results
			if (method.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			ParseMethod(method.Name, method.Result);
		}

		/// <summary>
		/// Updates the proxy with a method result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected virtual void ParseMethod(string name, ApiResult result)
		{
		}

		/// <summary>
		/// Updates the proxy with a node result.
		/// </summary>
		/// <param name="node"></param>
		private void ParseNode(ApiNodeInfo node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			// Only care about good results
			if (node.Result != null && node.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			// If we asked for information about the node at this level the response contains the node result.
			// If we are getting a response for a nested item the response will be null.
			node = node.Result == null ? node : node.Result.GetValue<ApiNodeInfo>();

			ParseNode(node.Name, node);
		}

		/// <summary>
		/// Updates the proxy with a node result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="node"></param>
		protected virtual void ParseNode(string name, ApiNodeInfo node)
		{
		}

		/// <summary>
		/// Updates the proxy with a node group result.
		/// </summary>
		/// <param name="nodeGroup"></param>
		private void ParseNodeGroup(ApiNodeGroupInfo nodeGroup)
		{
			if (nodeGroup == null)
				throw new ArgumentNullException("nodeGroup");

			// Only care about good results
			if (nodeGroup.Result != null && nodeGroup.Result.ErrorCode != ApiResult.eErrorCode.Ok)
				return;

			// If we asked for information about the node group at this level the response contains the node group result.
			// If we are getting a response for a nested item the response will be null.
			nodeGroup = nodeGroup.Result == null ? nodeGroup : nodeGroup.Result.GetValue<ApiNodeGroupInfo>();

			ParseNodeGroup(nodeGroup.Name, nodeGroup);
		}

		/// <summary>
		/// Updates the proxy with a node group result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="nodeGroup"></param>
		protected virtual void ParseNodeGroup(string name, ApiNodeGroupInfo nodeGroup)
		{
		}

		#endregion
	}
}
