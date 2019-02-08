using ICD.Connect.API.Info;

namespace ICD.Connect.API.EventArguments
{
	public interface IApiEventArgs
	{
		/// <summary>
		/// Gets the name of the API event associated with these arguments.
		/// </summary>
		string EventName { get; }

		/// <summary>
		/// Builds an API result for the args.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		void BuildResult(object sender, ApiResult result);

		/// <summary>
		/// Builds an API result for the args.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventInfo"></param>
		void BuildResult(object sender, ApiEventInfo eventInfo);
	}
}
