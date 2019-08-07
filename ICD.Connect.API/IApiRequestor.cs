using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public interface IApiRequestor
	{
		/// <summary>
		/// Gets the simple contextual name for the API requestor.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Handles an event feedback command from the ApiHandler.
		/// </summary>
		/// <param name="command"></param>
		void HandleFeedback(ApiClassInfo command);
	}
}
