using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
    public sealed class ApiRequestor : IApiRequestor
	{
		/// <summary>
		/// Raised when the API sends event feedback commands.
		/// </summary>
		public event EventHandler<ApiClassInfoEventArgs> OnApiFeedback;

		/// <summary>
		/// Gets/sets the name used for logging.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the string representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new ReprBuilder(this).AppendProperty("Name", Name).ToString();
		}

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
