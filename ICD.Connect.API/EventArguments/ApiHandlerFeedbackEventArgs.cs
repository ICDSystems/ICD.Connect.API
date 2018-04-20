using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.EventArguments
{
	public sealed class ApiHandlerFeedbackEventArgs : GenericEventArgs<ApiClassInfo>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public ApiHandlerFeedbackEventArgs(ApiClassInfo data)
			: base(data)
		{
		}
	}
}
