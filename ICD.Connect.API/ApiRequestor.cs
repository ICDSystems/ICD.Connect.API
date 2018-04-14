using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Info;

namespace ICD.Connect.Krang.Remote.Direct.API
{
    public sealed class ApiRequestor : IApiRequestor
	{
		/// <summary>
		/// Raised when the API sends event feedback commands.
		/// </summary>
		public event EventHandler<ApiClassInfoEventArgs> OnApiFeedback;

		/// <summary>
		/// Handles an event feedback command from the ApiHandler.
		/// </summary>
		/// <param name="command"></param>
		public void HandleFeedback(ApiClassInfo command)
		{
			OnApiFeedback.Raise(this, new ApiClassInfoEventArgs(command));
		}
	}
}
