using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public interface IApiRequestor
	{
		/// <summary>
		/// Handles an event feedback command from the ApiHandler.
		/// </summary>
		/// <param name="command"></param>
		void HandleFeedback(ApiClassInfo command);
	}
}
